using KonFwBaseModels;
using KonFwBaseModels.ResponseModels;
using System.Globalization;
using System.Linq.Dynamic.Core;

namespace KonFwBaseHelpers
{
    public static class QueryableExtensions
    {
        public static FilterResponse<T> ToFilterView<T>(
            this IQueryable<T> query, BaseRequestDto<T> filter)
        {
            // filter
            query = Filter(query, filter.Filter);
            var count = query.Count();
            var pageNumber = count / filter.Size;

            if (filter.Size != 0)
            {
                if (count % filter.Size != 0)
                {
                    pageNumber++;
                }
            }

            //sort
            if (filter.Sort != null)
            {
                query = Sort(query, filter.Sort);
            }

            query = Limit(query, filter.Size, filter.Page);

            var PageNumberAndQueryList = new FilterResponse<T> { Query = query, PageNumber = pageNumber, Count = count };
            // return the final query
            return PageNumberAndQueryList;
        }

        private static IQueryable<T> Filter<T>(
    IQueryable<T> queryable, SearchFilter filter)
        {
            if ((filter != null) && (filter?.Logic != null))
            {
                var filters = GetAllFilters(filter);
                var values = filters.Select(f => f.Value is string ? f.Value.ToString()?.ToLower(new CultureInfo("tr-TR")) : f.Value).ToArray();
                var where = Transform(filter, filters);

                queryable = queryable.Where(where, values);
            }

            return queryable;
        }


        private static IQueryable<T> Sort<T>(
            IQueryable<T> queryable, IEnumerable<Sort> sort)
        {
            if (sort != null && sort.Any())
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

        private static readonly IDictionary<string, string>
        Operators = new Dictionary<string, string>
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
            var comparison = Operators[filter.Operator];

            var split = filter.Field.Split(".").ToList();
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
    }

}
