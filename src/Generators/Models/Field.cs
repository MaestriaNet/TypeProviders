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
            "bool?" => $"{variableName}.ToBooleanSafe()",
            "int" => $"{variableName}.ToInt32()",
            "int?" => $"{variableName}.ToInt32Safe()",
            "decimal" => $"{variableName}.ToDecimal()",
            "decimal?" => $"{variableName}.ToDecimalSafe()",
            "string" => $"{variableName}.ToString()",
            "DateTime" => $"{variableName}.ToDateTime()",
            "DateTime?" => $"{variableName}.ToDateTimeSafe()",
            _ => $"/* Skiped assignment {variableName}: {DataType} */"
        };
    }
}