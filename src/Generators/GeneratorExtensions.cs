namespace Maestria.TypeProviders.Generators
{
    public static class GeneratorExtensions
    {
        public static string ToCamelCase(this string value) => char.ToLowerInvariant(value[0]) + value.Substring(1);
    }
}