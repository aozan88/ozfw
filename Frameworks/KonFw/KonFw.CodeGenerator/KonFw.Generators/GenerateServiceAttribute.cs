namespace KonFw.Generators;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
public class GenerateServiceAttribute : Attribute
{
    public GenerateServiceAttribute(string? template)
    {
    }
}
