using System;
using System.IO;
using System.Threading.Tasks;
using Scriban;

class Program
{
    static async Task Main(string[] args)
    {
        // Argümanla çalıştırılırsa model tabanlı üretim (bkz. Cli/CodeGenCli.cs); argümansız eski akış devam eder
        if (args.Length > 0)
        {
            Environment.ExitCode = OzFw.ScribanCodeGen.Cli.CodeGenCli.Run(args);
            return;
        }

        // Hangi şablonlar çalıştırılacak
        var templates = new[]
        {
            ("interface.sbn", "Services/Interfaces", "I{{ entity_name }}Service.cs"),
            ("service.sbn", "Services", "{{ entity_name }}Service.cs"),
            ("controller.sbn", "Controllers", "{{ entity_name }}Controller.cs"),
            ("automapper.sbn", "Mappings", "{{ entity_name }}Profile.cs")
        };

        // Üretilecek entity'ler
        var entities = new[] { "Product", "Order", "Customer" };

        // Namespace’in temeli
        string namespaceBase = "MyApp";

        // Her entity için şablonları işle
        foreach (var entityName in entities)
        {
            foreach (var (templateFile, outputFolder, outputNameTemplate) in templates)
            {
                // Önce çalışma dizinindeki Templates, yoksa çıktı klasörüne kopyalanan Templates
                var templatePath = Path.Combine("Templates", templateFile);
                if (!File.Exists(templatePath))
                    templatePath = Path.Combine(AppContext.BaseDirectory, "Templates", templateFile);
                var templateText = await File.ReadAllTextAsync(templatePath);
                var template = Template.Parse(templateText);

                var result = template.Render(new
                {
                    entity_name = entityName,
                    @namespace = $"{namespaceBase}.{outputFolder.Replace("/", ".")}"
                });

                var outputFileName = outputNameTemplate.Replace("{{ entity_name }}", entityName);
                var outputPath = Path.Combine("Generated", outputFolder);
                Directory.CreateDirectory(outputPath);
                //eksik using direktifleri ve eksik model tanımları nedeniyle dosya silmede hata olabilir
                var fullPath = Path.Combine(outputPath, outputFileName);

                if (File.Exists(fullPath))
                {
                    Console.WriteLine($"[Üzerine Yazılıyor] {outputFileName}");
                }
                else
                {
                    Console.WriteLine($"[Oluşturuluyor] {outputFileName}");
                }

                await File.WriteAllTextAsync(fullPath, result);
            }
        }

        Console.WriteLine("\n✅ Kod üretimi tamamlandı.");
    }
}
