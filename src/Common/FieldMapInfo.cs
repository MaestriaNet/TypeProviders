namespace Maestria.TypeProviders.Common
{
    public class FieldMapInfo
    {
        /// <summary>
        /// Name of field to get value in source provider (Identity at source)
        /// </summary>
        /// <value></value>
        public string SourceName { get; set; }

        /// <summary>
        /// Name to create strong property (Identity to use)
        /// </summary>
        /// <value></value>
        public string PropertyName { get; set; }

        /// <summary>
        /// .NET strong data type to map value
        /// </summary>
        /// <value></value>
        public string DataType { get; set; }

        /// <summary>
        /// Source code to declarate property
        /// </summary>
        /// <returns></returns>
        public string GetSourceCodeToProperty() => $"public {DataType} {PropertyName} {{ get; set; }}";

        /// <summary>
        /// Source code to cast <paramref name="sourceExpression"/> value to <see cref="DataType"/> value
        /// </summary>
        /// <param name="sourceExpression"></param>
        /// <returns></returns>
        public string GetCastSourceCode(string sourceExpression) => DataType switch
        {
            "bool" => $"{sourceExpression}.ToBoolean()",
            "bool?" => $"{sourceExpression}.ToBooleanSafe()",
            "int" => $"{sourceExpression}.ToInt32()",
            "int?" => $"{sourceExpression}.ToInt32Safe()",
            "decimal" => $"{sourceExpression}.ToDecimal()",
            "decimal?" => $"{sourceExpression}.ToDecimalSafe()",
            "string" => $"{sourceExpression}.ToString()",
            "DateTime" => $"{sourceExpression}.ToDateTime()",
            "DateTime?" => $"{sourceExpression}.ToDateTimeSafe()",
            "TimeSpan" => $"{sourceExpression}.ToTimeSpan()",
            "TimeSpan?" => $"{sourceExpression}.ToTimeSpanSafe()",
            _ => $"null /* Skiped assignment {sourceExpression}: {DataType} */"
        };
    }
}