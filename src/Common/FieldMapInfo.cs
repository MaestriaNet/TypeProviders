using ClosedXML.Excel;

namespace Maestria.TypeProviders.Common;

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
    /// .NET strong data type to map value, when null, DataType its end with "?"
    /// </summary>
    /// <value></value>
    public string DataType { get; set; }

    /// <summary>
    /// Source code to declarate property
    /// </summary>
    /// <returns></returns>
    public string GetSourceCodeToProperty() => $"public {DataType} {PropertyName} {{ get; set; }}";

    public bool IsNullable => DataType.EndsWith("?");
    public string InlineDataType => IsNullable ? DataType.Substring(0, DataType.Length - 1) : DataType;

    /// <summary>
    /// Source code to cast <paramref name="cellValue"/> value to <see cref="DataType"/> value
    /// </summary>
    /// <param name="cellValue">Objeto do tipo <see cref="IXLCell"/></param>
    /// <returns></returns>
    public string GetCastSourceCode(string cellValue) => $"{cellValue}.GetValue<{DataType}>()";
}