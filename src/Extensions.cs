using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Maestria.TypeProviders
{
    public static class Extensions
    {
        public static string WithFirstCharLower(this string value) => char.ToLowerInvariant(value[0]) + value.Substring(1);

        public static SourceText GetEmbeddedSourceCode(string resourceName)
        {
            var assembly = Assembly.GetExecutingAssembly();
            using var stream = assembly.GetManifestResourceStream(resourceName);
            using var reader = new StreamReader(stream);
            return SourceText.From(reader.ReadToEnd(), Encoding.UTF8);
        }

        public static bool IsGlobalNamespace(this string @namespace) => @namespace == "<global namespace>";

        public static bool HasAttribute(this ISymbol symbol, string typeFullName) => symbol
            .GetAttributes()
            .Any(x => x.AttributeClass?.ToDisplayString() == typeFullName);
    }
}