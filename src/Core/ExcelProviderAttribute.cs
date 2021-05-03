using System;

namespace Maestria.TypeProviders.Core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExcelProviderAttribute : Attribute
    {
        public const string TypeFullName = "Maestria.TypeProviders.Core.ExcelProviderAttribute";

        /// <summary>
        /// File path to load Excel template and generate source code. Default location is source code of attribute analyze and suporte relative path with format "..\..\folder\file.xlsx"
        /// </summary>
        public string TemplatePath { get; set; }
    }
}