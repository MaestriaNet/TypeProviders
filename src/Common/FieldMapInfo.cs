namespace Maestria.TypeProviders.Common
{
    public class FieldMapInfo
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
            "TimeSpan" => $"{variableName}.ToTimeSpan()",
            "TimeSpan?" => $"{variableName}.ToTimeSpanSafe()",
            _ => $"null /* Skiped assignment {variableName}: {DataType} */"
        };
    }
}