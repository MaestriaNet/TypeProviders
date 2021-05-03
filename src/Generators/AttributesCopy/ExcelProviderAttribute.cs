namespace Maestria.TypeProviders.Generators.AttributesCopy
{
    /// <summary>
    /// Equivalent copy by Maestria.TypeProviders.Core.ExcelProviderAttribute.
    /// TODO: Fix it resolving "GeneratePathProperty" at "ProjectReference ..\Core\Core.csproj" in "Generators.csproj". When execute by consumer app it's necessary all assets compiled embbed in Generator assembly.
    /// </summary>
    internal class ExcelProviderCopyAttribute
    {
        /// <summary>
        /// File path to load Excel template and generate source code. Default location is source code of attribute analyze and suporte relative path with format "..\..\folder\file.xlsx"
        /// </summary>
        public string TemplatePath { get; set; }
    }
}