namespace Maestria.TypeProviders.Generators.Models
{
    public class Field
    {
        public string Name { get; set; }
        public string DataType { get; set; }

        public string GetSourceCode() => $"public {DataType} {Name} {{ get; set; }}";
    }
}