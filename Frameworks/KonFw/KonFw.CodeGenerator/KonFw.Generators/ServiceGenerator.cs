using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
using System.Diagnostics;
using System.Text;

namespace KonFw.Generators;

[Generator]
public class ServiceGenerator : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
        context.RegisterForSyntaxNotifications(() => new AttributeSyntaxReceiver<GenerateServiceAttribute>());
    }

    public void Execute(GeneratorExecutionContext context)
    {
        if (context.SyntaxReceiver is not AttributeSyntaxReceiver<GenerateServiceAttribute> syntaxReceiver)
        {
            return;
        }

        foreach (var classSyntax in syntaxReceiver.Classes)
        {
            var model = context.Compilation.GetSemanticModel(classSyntax.SyntaxTree);
            var symbol = model.GetDeclaredSymbol(classSyntax);

            // Tüm GenerateService attribute'larını bul.
            var attributes = classSyntax.AttributeLists
                .SelectMany(list => list.Attributes)
                .Where(attr =>
                    model.GetTypeInfo(attr).Type?.ToDisplayString() == typeof(GenerateServiceAttribute).FullName)
                .ToList();

            foreach (var attribute in attributes)
            {
                var templateParameter = attribute.ArgumentList?.Arguments.FirstOrDefault()?.GetLastToken().ValueText;

                var overridenTemplate = templateParameter != null
                    ? context.AdditionalFiles.FirstOrDefault(x => x.Path.EndsWith(templateParameter))?.GetText()?.ToString()
                    : null;

                var sourceCode = GetSourceCodeFor(symbol, overridenTemplate);

                context.AddSource(
                    $"{symbol.Name}{templateParameter ?? "Controller"}.g.cs",
                    SourceText.From(sourceCode, Encoding.UTF8));
            }
        }
    }

    private string GetSourceCodeFor(INamedTypeSymbol symbol, string? template)
    {
        return template
            .Replace("{{" + nameof(DefaultTemplateParameters.ClassName) + "}}", symbol.Name)
            .Replace("{{" + nameof(DefaultTemplateParameters.Namespace) + "}}", GetNamespaceRecursively(symbol.ContainingNamespace))
            .Replace("{{" + nameof(DefaultTemplateParameters.PrefferredNamespace) + "}}", symbol.ContainingAssembly.Name)
            ;
    }


    private string GetNamespaceRecursively(INamespaceSymbol symbol)
    {
        if (symbol.ContainingNamespace == null)
        {
            return symbol.Name;
        }

        return (GetNamespaceRecursively(symbol.ContainingNamespace) + "." + symbol.Name).Trim('.');
    }

}
