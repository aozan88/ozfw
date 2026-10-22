using KonFwBaseModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Newtonsoft.Json.Linq;
using System.Collections;
using System.Globalization;
using System.Linq.Dynamic.Core;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text.Json;

namespace KonFwBaseHelpers
{
    public static class QueryableExtensions
    {
        private static readonly CultureInfo TrCulture = new("tr-TR");

        /// <summary>
        /// Filtre ağacını, sıralamayı ve sayfalamayı uygular, sonucu BaseResponseDto olarak döner.
        /// Alan adları TRequest (DTO) üzerinden entity property'lerine eşlenir (bkz. DtoEntityMapper).
        /// TotalTableCount: filtresiz toplam, TotalRecordCount: filtre sonrası toplam, Count: dönen sayfadaki kayıt sayısı.
        /// </summary>
        public static async Task<BaseResponseDto<TEntity>> ApplyQuery<TEntity, TRequest>(
            this IQueryable<TEntity> query, BaseRequestDto<TRequest> request, CancellationToken cancellationToken = default)
            where TEntity : class
        {
            var dtoType = typeof(TRequest) == typeof(TEntity) ? null : typeof(TRequest);

            var totalTableCount = await CountAsync(query, cancellationToken);
            query = query.ApplyFilter(request.Filter, dtoType);
            var totalRecordCount = await CountAsync(query, cancellationToken);
            query = query.ApplySorting(request.Sort, dtoType);
            query = query.ApplyPaging(request.Page, request.Size);
            var items = await ToListAsync(query, cancellationToken);

            return new BaseResponseDto<TEntity>
            {
                IsSuccess = true,
                PageNumber = request.Page,
                Size = request.Size,
                Count = items.Count,
                TotalRecordCount = totalRecordCount,
                TotalTableCount = totalTableCount,
                ClientId = request.ClientId,
                ObjectValue = items,
                SortList = request.Sort?.ToList() ?? new List<Sort>()
            };
        }

        public static IQueryable<T> ApplyFilter<T>(this IQueryable<T> query, SearchFilter? filter, Type? dtoType = null)
        {
            if (filter == null)
                return query;

            var parameter = Expression.Parameter(typeof(T), "x");
            var body = BuildNode(parameter, typeof(T), filter, dtoType);
            return body == null ? query : query.Where(Expression.Lambda<Func<T, bool>>(body, parameter));
        }

        public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, IEnumerable<Sort>? sorts, Type? dtoType = null)
        {
            if (sorts == null)
                return query;

            var first = true;
            foreach (var sort in sorts.Where(s => !string.IsNullOrWhiteSpace(s.Field)))
            {
                var path = DtoEntityMapper.ResolveEntityPath(dtoType, typeof(T), sort.Field);
                var parameter = Expression.Parameter(typeof(T), "x");
                var member = BuildMemberPath(parameter, typeof(T), path);
                var keySelector = Expression.Lambda(member, parameter);

                var methodName = first
                    ? (sort.Dir == SortType.DESC ? nameof(Queryable.OrderByDescending) : nameof(Queryable.OrderBy))
                    : (sort.Dir == SortType.DESC ? nameof(Queryable.ThenByDescending) : nameof(Queryable.ThenBy));

                var method = typeof(Queryable).GetMethods()
                    .First(m => m.Name == methodName && m.GetParameters().Length == 2)
                    .MakeGenericMethod(typeof(T), member.Type);

                query = (IQueryable<T>)method.Invoke(null, new object[] { query, keySelector })!;
                first = false;
            }
            return query;
        }

        /// <param name="page">1 tabanlı sayfa numarası</param>
        /// <param name="size">Sayfa boyutu; 0 veya negatif ise sayfalama yapılmaz</param>
        public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int size)
        {
            if (size <= 0)
                return query;
            var skip = (Math.Max(page, 1) - 1) * size;
            return query.Skip(skip).Take(size);
        }

        #region Filtre ağacı

        private static Expression? BuildNode(ParameterExpression parameter, Type entityType, SearchFilter node, Type? dtoType)
        {
            if (node.Filters != null && node.Filters.Count > 0)
            {
                var children = node.Filters
                    .Select(f => BuildNode(parameter, entityType, f, dtoType))
                    .Where(e => e != null)
                    .ToList();
                if (children.Count == 0)
                    return null;

                var isOr = string.Equals(node.Logic, "or", StringComparison.OrdinalIgnoreCase);
                return children.Aggregate((l, r) => isOr ? Expression.OrElse(l!, r!) : Expression.AndAlso(l!, r!));
            }

            if (string.IsNullOrWhiteSpace(node.Field))
                return null;
            if (string.IsNullOrWhiteSpace(node.Operator))
                throw new QueryFieldException($"'{node.Field}' alanı için operatör belirtilmemiş.");

            var path = DtoEntityMapper.ResolveEntityPath(dtoType, entityType, node.Field);
            var segments = path.Split('.', StringSplitOptions.RemoveEmptyEntries);
            return BuildPathPredicate(parameter, segments, 0, node);
        }

        /// <summary>
        /// Yol üzerindeki koleksiyonlar için Any(...) üretir. "Kalemler.Any.Ad" ya da "Kalemler.Ad" (varsayılan Any)
        /// ve "Kalemler.All.Ad" desteklenir.
        /// </summary>
        private static Expression BuildPathPredicate(Expression instance, string[] segments, int index, SearchFilter node)
        {
            var current = instance;
            for (var i = index; i < segments.Length; i++)
            {
                var prop = FindProperty(current.Type, segments[i])
                    ?? throw new QueryFieldException($"'{node.Field}' alanı bulunamadı ('{current.Type.Name}.{segments[i]}').");
                current = Expression.Property(current, prop);

                var elementType = GetCollectionElementType(current.Type);
                if (elementType != null && i < segments.Length - 1)
                {
                    var next = i + 1;
                    var quantifier = nameof(Enumerable.Any);
                    if (segments[next].Equals("Any", StringComparison.OrdinalIgnoreCase) ||
                        segments[next].Equals("All", StringComparison.OrdinalIgnoreCase))
                    {
                        quantifier = segments[next].Equals("All", StringComparison.OrdinalIgnoreCase) ? nameof(Enumerable.All) : nameof(Enumerable.Any);
                        next++;
                    }
                    if (next >= segments.Length)
                        throw new QueryFieldException($"'{node.Field}' alanında koleksiyondan sonra bir property bekleniyor.");

                    var itemParam = Expression.Parameter(elementType, "i");
                    var inner = BuildPathPredicate(itemParam, segments, next, node);
                    var lambda = Expression.Lambda(inner, itemParam);
                    var method = typeof(Enumerable).GetMethods()
                        .First(m => m.Name == quantifier && m.GetParameters().Length == 2)
                        .MakeGenericMethod(elementType);
                    return Expression.Call(method, current, lambda);
                }
            }
            return BuildComparison(current, node);
        }

        private static Expression BuildComparison(Expression member, SearchFilter node)
        {
            var op = node.Operator!.ToLowerInvariant();
            var type = member.Type;
            var underlying = Nullable.GetUnderlyingType(type) ?? type;
            var isString = underlying == typeof(string);

            try
            {
                switch (op)
                {
                    case "isnull":
                        return Expression.Equal(member, Expression.Constant(null, type));
                    case "isnotnull":
                        return Expression.NotEqual(member, Expression.Constant(null, type));

                    case "contains":
                    case "doesnotcontain":
                    case "startswith":
                    case "endswith":
                        {
                            var text = ToLowerTr(ConvertValue(node.Value, typeof(string)) as string ?? string.Empty);
                            var target = isString ? LowerMember(member) : Expression.Call(member, typeof(object).GetMethod(nameof(ToString))!);
                            var methodName = op switch
                            {
                                "startswith" => nameof(string.StartsWith),
                                "endswith" => nameof(string.EndsWith),
                                _ => nameof(string.Contains)
                            };
                            var call = Expression.Call(target, typeof(string).GetMethod(methodName, new[] { typeof(string) })!, Parameter(text, typeof(string)));
                            var notNull = Expression.NotEqual(member, Expression.Constant(null, type));
                            Expression result = op == "doesnotcontain" ? Expression.Not(call) : call;
                            return type.IsValueType && Nullable.GetUnderlyingType(type) == null ? result : Expression.AndAlso(notNull, result);
                        }

                    case "in":
                        {
                            var values = ToList(node.Value).Select(v => ConvertValue(v, type)).ToList();
                            if (values.Count == 0)
                                return Expression.Constant(false);
                            var listType = typeof(List<>).MakeGenericType(type);
                            var list = (IList)Activator.CreateInstance(listType)!;
                            foreach (var v in values)
                                list.Add(isString ? ToLowerTr(v as string) : v);
                            var contains = listType.GetMethod(nameof(List<object>.Contains))!;
                            return Expression.Call(Parameter(list, listType), contains, isString ? LowerMember(member) : member);
                        }

                    case "between":
                        {
                            var values = ToList(node.Value);
                            if (values.Count != 2)
                                throw new QueryFieldException($"'{node.Field}' için 'between' iki değer bekler.");
                            var from = Parameter(ConvertValue(values[0], type), type);
                            var to = Parameter(ConvertValue(values[1], type), type);
                            return Expression.AndAlso(Expression.GreaterThanOrEqual(member, from), Expression.LessThanOrEqual(member, to));
                        }
                }

                var value = ConvertValue(node.Value, type);
                Expression left = member;
                Expression right = Parameter(isString ? ToLowerTr(value as string) : value, type);
                if (isString && (op == "eq" || op == "neq"))
                    left = LowerMember(member);

                return op switch
                {
                    "eq" => Expression.Equal(left, right),
                    "neq" => Expression.NotEqual(left, right),
                    "lt" => Expression.LessThan(left, right),
                    "lte" => Expression.LessThanOrEqual(left, right),
                    "gt" => Expression.GreaterThan(left, right),
                    "gte" => Expression.GreaterThanOrEqual(left, right),
                    _ => throw new QueryFieldException($"'{node.Operator}' operatörü desteklenmiyor.")
                };
            }
            catch (QueryFieldException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new QueryFieldException($"'{node.Field}' alanı için filtre oluşturulamadı: {ex.Message}", ex);
            }
        }

        #endregion

        #region Yardımcılar

        private static PropertyInfo? FindProperty(Type type, string name) =>
            type.GetProperty(name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);

        private static Expression BuildMemberPath(Expression parameter, Type type, string path)
        {
            Expression current = parameter;
            foreach (var segment in path.Split('.', StringSplitOptions.RemoveEmptyEntries))
            {
                var prop = FindProperty(current.Type, segment)
                    ?? throw new QueryFieldException($"Sıralama alanı bulunamadı: '{path}' ('{current.Type.Name}.{segment}').");
                current = Expression.Property(current, prop);
            }
            return current;
        }

        private static Type? GetCollectionElementType(Type type)
        {
            if (type == typeof(string))
                return null;
            var enumerable = type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                ? type
                : type.GetInterfaces().FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));
            return enumerable?.GetGenericArguments()[0];
        }

        private static Expression LowerMember(Expression member) =>
            Expression.Call(member, typeof(string).GetMethod(nameof(string.ToLower), Type.EmptyTypes)!);

        private static string? ToLowerTr(string? value) => value?.ToLower(TrCulture);

        /// <summary>
        /// Değeri sabit yerine kapalı bir değişken olarak verir; EF sorguyu parametreli üretir ve plan önbelleği şişmez.
        /// </summary>
        private static Expression Parameter(object? value, Type type)
        {
            var boxType = typeof(StrongBox<>).MakeGenericType(type);
            var box = Activator.CreateInstance(boxType, value);
            return Expression.Field(Expression.Constant(box), nameof(StrongBox<object>.Value));
        }

        private static List<object?> ToList(object? value)
        {
            switch (value)
            {
                case null:
                    return new List<object?>();
                case JsonElement je when je.ValueKind == JsonValueKind.Array:
                    return je.EnumerateArray().Select(e => (object?)e).ToList();
                case JArray ja:
                    return ja.Select(t => (object?)t).ToList();
                case string s:
                    return new List<object?> { s };
                case IEnumerable e:
                    return e.Cast<object?>().ToList();
                default:
                    return new List<object?> { value };
            }
        }

        private static object? ConvertValue(object? value, Type targetType)
        {
            var underlying = Nullable.GetUnderlyingType(targetType) ?? targetType;

            value = value switch
            {
                JsonElement je => je.ValueKind switch
                {
                    JsonValueKind.Null or JsonValueKind.Undefined => null,
                    JsonValueKind.String => je.GetString(),
                    JsonValueKind.True => true,
                    JsonValueKind.False => false,
                    _ => je.GetRawText()
                },
                JValue jv => jv.Value,
                JToken jt => jt.ToString(),
                _ => value
            };

            if (value == null)
            {
                if (underlying.IsValueType && Nullable.GetUnderlyingType(targetType) == null)
                    throw new QueryFieldException($"'{targetType.Name}' tipi için null değer verilemez.");
                return null;
            }

            if (underlying.IsInstanceOfType(value))
                return value;

            var text = Convert.ToString(value, CultureInfo.InvariantCulture)!;

            if (underlying == typeof(string))
                return text;
            if (underlying == typeof(Guid))
                return Guid.Parse(text);
            if (underlying.IsEnum)
                return int.TryParse(text, out var n) ? Enum.ToObject(underlying, n) : Enum.Parse(underlying, text, ignoreCase: true);
            if (underlying == typeof(DateTime))
                return DateTime.Parse(text, CultureInfo.InvariantCulture, DateTimeStyles.RoundtripKind);
            if (underlying == typeof(DateTimeOffset))
                return DateTimeOffset.Parse(text, CultureInfo.InvariantCulture);
            if (underlying == typeof(DateOnly))
                return DateOnly.Parse(text, CultureInfo.InvariantCulture);
            if (underlying == typeof(bool))
                return bool.Parse(text);

            return Convert.ChangeType(text, underlying, CultureInfo.InvariantCulture);
        }

        private static async Task<int> CountAsync<T>(IQueryable<T> query, CancellationToken ct) =>
            query.Provider is IAsyncQueryProvider ? await EntityFrameworkQueryableExtensions.CountAsync(query, ct) : query.Count();

        private static async Task<List<T>> ToListAsync<T>(IQueryable<T> query, CancellationToken ct) =>
            query.Provider is IAsyncQueryProvider ? await EntityFrameworkQueryableExtensions.ToListAsync(query, ct) : query.ToList();

        #endregion

        #region Eski API (System.Linq.Dynamic.Core)

        [Obsolete("ApplyQuery kullanın.")]
        public static async Task<BaseResponseDto<T>> ToFilterView<T>(
            this IQueryable<T> query, BaseRequestDto<T> filter)
        {
            try
            {
                var totalRecordCount = query.Count();
                query = Filter(query, filter.Filter);
                var count = query.Count();

                if (filter.Sort != null)
                {
                    query = Sort(query, filter.Sort);
                }

                query = Limit(query, filter.Size, filter.Page);

                return await Task.FromResult(new BaseResponseDto<T>
                {
                    ObjectValue = query.ToList(),
                    IsSuccess = true,
                    PageNumber = filter.Page,
                    Size = filter.Size,
                    TotalRecordCount = totalRecordCount,
                    Count = count,
                    ClientId = filter.ClientId
                });
            }
            catch (Exception ex)
            {
                return new BaseResponseDto<T>
                {
                    IsSuccess = false,
                    Mesaj = ex.Message,
                    ClientId = filter.ClientId
                };
            }
        }

        private static IQueryable<T> Filter<T>(IQueryable<T> queryable, SearchFilter? filter)
        {
            if (filter?.Logic != null)
            {
                var filters = GetAllFilters(filter);
                var values = filters.Select(f => f.Value is string ? f.Value.ToString()?.ToLower(TrCulture) : f.Value).ToArray();
                var where = Transform(filter, filters);
                queryable = queryable.Where(where, values);
            }
            return queryable;
        }

        private static IQueryable<T> Sort<T>(IQueryable<T> queryable, IEnumerable<Sort> sort)
        {
            if (sort.Any())
            {
                var ordering = string.Join(",", sort.Select(s => $"{s.Field} {s.Dir}"));
                return queryable.OrderBy(ordering);
            }
            return queryable;
        }

        private static IQueryable<T> Limit<T>(IQueryable<T> queryable, int limit, int offset)
        {
            int skip = limit * (offset - 1);
            return queryable.Skip(skip).Take(limit);
        }

        private static readonly IDictionary<string, string> Operators = new Dictionary<string, string>
        {
            {"eq", "="},
            {"neq", "!="},
            {"lt", "<"},
            {"lte", "<="},
            {"gt", ">"},
            {"gte", ">="},
            {"startswith", "StartsWith"},
            {"endswith", "EndsWith"},
            {"contains", "Contains"},
            {"doesnotcontain", "Contains"},
        };

        public static IList<SearchFilter> GetAllFilters(SearchFilter filter)
        {
            var filters = new List<SearchFilter>();
            GetFilters(filter, filters);
            return filters;
        }

        private static void GetFilters(SearchFilter filter, IList<SearchFilter> filters)
        {
            if (filter.Filters != null && filter.Filters.Any())
            {
                foreach (var item in filter.Filters)
                {
                    GetFilters(item, filters);
                }
            }
            else
            {
                filters.Add(filter);
            }
        }

        public static string Transform(SearchFilter filter, IList<SearchFilter> filters)
        {
            if (filter.Filters != null && filter.Filters.Any())
            {
                return "(" + String.Join(" " + filter.Logic + " ", filter.Filters.Select(f => Transform(f, filters)).ToArray()) + ")";
            }
            int index = filters.IndexOf(filter);
            var comparison = Operators[filter.Operator!];
            var split = filter.Field!.Split(".").ToList();
            var allowed = new string[] { "Any", "All" };
            var pred = split.FirstOrDefault(x => allowed.Contains(x));
            if (split.Count > 1 && pred != null)
            {
                var beforePred = split.Take(split.IndexOf(pred)).ToArray();
                var afterPred = split.Skip(split.IndexOf(pred) + 1).ToArray();

                if (filter.Operator == "doesnotcontain")
                {
                    return filter.Value is string
                        ? $"{string.Join('.', beforePred)}.{pred}({string.Join('.', afterPred)}.ToLower().{comparison}(@{index}))"
                        : $"{string.Join('.', beforePred)}.{pred}({string.Join('.', afterPred)}.ToString().{comparison}(@{index}))";
                }

                if (comparison is "StartsWith" or "EndsWith" or "Contains")
                {
                    return filter.Value is string
                        ? $"{string.Join('.', beforePred)}.{pred}({string.Join('.', afterPred)}.ToLower().{comparison}(@{index}))"
                        : $"{string.Join('.', beforePred)}.{pred}({string.Join('.', afterPred)}.ToString().{comparison}(@{index}))";
                }

                return filter.Value is string
                    ? $"{string.Join('.', beforePred)}.{pred}({string.Join('.', afterPred)}.ToLower(){comparison}(@{index}))"
                    : $"{string.Join('.', beforePred)}.{pred}({string.Join('.', afterPred)}{comparison}(@{index}))";
            }
            else
            {
                if (filter.Operator == "doesnotcontain")
                {
                    return String.Format("({0} != null && !{0}.ToString().{1}(@{2}))",
                        filter.Field, comparison, index);
                }

                if (comparison == "StartsWith" ||
                    comparison == "EndsWith" ||
                    comparison == "Contains")
                {
                    return String.Format("{0} != null && {0}.ToLower().{1}(@{2})",
                    filter.Field, comparison, index);
                }

                return String.Format("{0}" + (filter.Value is string ? ".ToLower()" : "") + "{1} @{2}", filter.Field, comparison, index);
            }
        }

        #endregion
    }
}
