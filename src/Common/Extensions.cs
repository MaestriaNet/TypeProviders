using System.Linq;
using Microsoft.CodeAnalysis;

namespace Maestria.TypeProviders.Common
{
    public static class Extensions
    {
        public static bool HasAttribute(this ISymbol symbol, string typeFullName) => symbol
            .GetAttributes()
            .Any(x => x.AttributeClass?.ToDisplayString() == typeFullName);
    }
}