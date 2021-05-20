using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis.Text;

namespace Maestria.TypeProviders
{
    public static class GeneratorExtensions
    {
        public static string ToCamelCase(this string value) => char.ToLowerInvariant(value[0]) + value.Substring(1);

        public static SourceText GetEmbeddedSourceCode(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            return SourceText.From(reader.ReadToEnd(), Encoding.UTF8);
        }
    }
}