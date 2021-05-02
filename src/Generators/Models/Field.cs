using DocumentFormat.OpenXml.Drawing.Charts;

namespace Maestria.TypeProviders.Generators.Models
{
    public class Field
    {
        public string SourceName { get; set; }
        public string PropertyName { get; set; }
        public string DataType { get; set; }

        public string GetSourceCode() => $"public {DataType} {PropertyName} {{ get; set; }}";

        public string GetCastSourceCode(string variableName) => DataType switch
        {
            "bool" => $"{variableName}.ToBoolean()",
            "int" => $"{variableName}.ToInt32()",
            "string" => $"{variableName}.ToString()",
            "DateTime" => $"{variableName}.ToDateTime()",
            _ => $"/* Skiped assignment {variableName}: {DataType} */"
        };
    }
}