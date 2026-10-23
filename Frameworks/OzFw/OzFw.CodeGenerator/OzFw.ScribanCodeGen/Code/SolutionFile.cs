using System.Text;
using System.Text.RegularExpressions;

namespace OzFw.ScribanCodeGen.Code
{
    /// <summary>
    /// .sln dosyasına proje ekler. "dotnet sln add"in aksine mevcut klasör yapısını kullanır
    /// (ör. Project/backend/Services altındaki "Services") ve yalnızca sln'de tanımlı yapılandırmaları
    /// (Debug|Any CPU, Release|Any CPU) ekler; diğer projelere dokunmaz. Zaten ekli projeler atlanır.
    /// </summary>
    public static class SolutionFile
    {
        private const string FolderType = "2150E333-8FDC-42A3-9474-1A3956D46DE8";
        private const string CSharpType = "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC";

        private static readonly Regex ProjectRegex = new(@"^Project\(""\{(?<type>[^}]+)\}""\) = ""(?<name>[^""]+)"", ""(?<path>[^""]+)"", ""\{(?<id>[^}]+)\}""", RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex NestedRegex = new(@"\{(?<child>[0-9A-Fa-f-]+)\} = \{(?<parent>[0-9A-Fa-f-]+)\}", RegexOptions.Compiled);

        private sealed record Entry(string Type, string Name, string Path, string Id);

        /// <param name="solutionFolder">"Services/Ornek": ilk parça mevcut klasörün adı, son parça modül klasörü</param>
        /// <param name="projects">sln dizinine göreli csproj yolları</param>
        /// <returns>Güncellenmiş içerik ve eklenen proje sayısı</returns>
        public static (string Content, int Added) AddProjects(string content, string solutionFolder, IEnumerable<string> projects, Func<Guid>? newGuid = null)
        {
            newGuid ??= Guid.NewGuid;
            var nl = content.Contains("\r\n") ? "\r\n" : "\n";
            string NewId() => newGuid().ToString("D").ToUpperInvariant();

            var entries = ProjectRegex.Matches(content)
                .Select(m => new Entry(m.Groups["type"].Value, m.Groups["name"].Value, m.Groups["path"].Value, m.Groups["id"].Value.ToUpperInvariant()))
                .ToList();
            var nested = NestedSection(content).ToDictionary(n => n.Child, n => n.Parent, StringComparer.OrdinalIgnoreCase);

            var toAdd = projects
                .Select(p => p.Replace('/', '\\'))
                .Where(p => !entries.Any(e => string.Equals(e.Path, p, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            if (toAdd.Count == 0)
                return (content, 0);

            var newProjects = new StringBuilder();
            var newNested = new List<(string Child, string Parent)>();

            // Klasör zinciri: ilk parça mevcut (ya da kökte yeni) klasör, sonrakiler onun altında
            string? parentId = null;
            foreach (var folderName in solutionFolder.Split('/', '\\', StringSplitOptions.RemoveEmptyEntries))
            {
                var candidates = entries
                    .Where(e => e.Type.Equals(FolderType, StringComparison.OrdinalIgnoreCase) && e.Name == folderName)
                    .Where(e => parentId == null || (nested.TryGetValue(e.Id, out var p) && p.Equals(parentId, StringComparison.OrdinalIgnoreCase)))
                    // Aynı adlı birden fazla klasör varsa en çok alt öğesi olan
                    .OrderByDescending(e => nested.Count(n => n.Value.Equals(e.Id, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                if (candidates.Count > 0)
                {
                    parentId = candidates[0].Id;
                    continue;
                }

                var folder = new Entry(FolderType, folderName, folderName, NewId());
                entries.Add(folder);
                newProjects.Append($"Project(\"{{{FolderType}}}\") = \"{folderName}\", \"{folderName}\", \"{{{folder.Id}}}\"{nl}EndProject{nl}");
                if (parentId != null)
                {
                    newNested.Add((folder.Id, parentId));
                    nested[folder.Id] = parentId;
                }
                parentId = folder.Id;
            }

            var configs = Regex.Match(content, @"GlobalSection\(SolutionConfigurationPlatforms\) = preSolution(?<body>.*?)EndGlobalSection", RegexOptions.Singleline)
                .Groups["body"].Value
                .Split('\n').Select(l => l.Trim()).Where(l => l.Contains('='))
                .Select(l => l.Split('=')[0].Trim()).ToList();
            if (configs.Count == 0)
                configs = new List<string> { "Debug|Any CPU", "Release|Any CPU" };

            var newConfigs = new StringBuilder();
            foreach (var path in toAdd)
            {
                var id = NewId();
                var name = Path.GetFileNameWithoutExtension(path);
                newProjects.Append($"Project(\"{{{CSharpType}}}\") = \"{name}\", \"{path}\", \"{{{id}}}\"{nl}EndProject{nl}");
                foreach (var cfg in configs)
                {
                    newConfigs.Append($"\t\t{{{id}}}.{cfg}.ActiveCfg = {cfg}{nl}");
                    newConfigs.Append($"\t\t{{{id}}}.{cfg}.Build.0 = {cfg}{nl}");
                }
                if (parentId != null)
                    newNested.Add((id, parentId));
            }

            var globalIdx = Regex.Match(content, @"^Global\s*$", RegexOptions.Multiline).Index;
            content = content.Insert(globalIdx, newProjects.ToString());

            content = InsertIntoSection(content, "ProjectConfigurationPlatforms", "postSolution", newConfigs.ToString(), nl);
            var nestedText = string.Concat(newNested.Select(n => $"\t\t{{{n.Child}}} = {{{n.Parent}}}{nl}"));
            if (nestedText.Length > 0)
                content = InsertIntoSection(content, "NestedProjects", "preSolution", nestedText, nl);

            return (content, toAdd.Count);
        }

        private static IEnumerable<(string Child, string Parent)> NestedSection(string content)
        {
            var m = Regex.Match(content, @"GlobalSection\(NestedProjects\) = preSolution(?<body>.*?)EndGlobalSection", RegexOptions.Singleline);
            return m.Success
                ? NestedRegex.Matches(m.Groups["body"].Value).Select(x => (x.Groups["child"].Value.ToUpperInvariant(), x.Groups["parent"].Value.ToUpperInvariant()))
                : Enumerable.Empty<(string, string)>();
        }

        private static string InsertIntoSection(string content, string section, string phase, string lines, string nl)
        {
            var m = Regex.Match(content, $@"GlobalSection\({section}\) = {phase}.*?(?<end>^\s*EndGlobalSection)", RegexOptions.Singleline | RegexOptions.Multiline);
            if (m.Success)
                return content.Insert(m.Groups["end"].Index, lines);

            var endGlobal = Regex.Match(content, @"^EndGlobal\s*$", RegexOptions.Multiline).Index;
            return content.Insert(endGlobal, $"\tGlobalSection({section}) = {phase}{nl}{lines}\tEndGlobalSection{nl}");
        }
    }
}
