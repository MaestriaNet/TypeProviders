using System;

namespace Maestria.TypeProviders.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class ExcelProviderAttribute : Attribute
    {
        public ExcelProviderAttribute(string templatePath)
        {
            TemplatePath = templatePath;
        }

        public string TemplatePath { get; set; }
    }
}