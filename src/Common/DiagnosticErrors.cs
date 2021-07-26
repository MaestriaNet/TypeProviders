using Microsoft.CodeAnalysis;

namespace Maestria.TypeProviders.Common
{
    public static class DiagnosticErrors
    {
        public static readonly DiagnosticDescriptor Generic = new DiagnosticDescriptor(
            id: "MTP001",
            title: "Cannot generate source code for class",
            messageFormat: "Cannot generate source code for class {0}. {1}.",
            category: "MaestriaTypeProviders",
            defaultSeverity: DiagnosticSeverity.Error,
            isEnabledByDefault: true);
    }
}