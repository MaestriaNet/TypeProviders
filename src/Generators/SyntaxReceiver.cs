using System.Collections.Generic;
using System.Linq;
using Maestria.TypeProviders.Core;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.CSharp;

namespace Maestria.TypeProviders.Generators
{
    public class SyntaxReceiver : ISyntaxContextReceiver
    {
        public IList<ClassDeclarationSyntax> CandidateClasses { get; } = new List<ClassDeclarationSyntax>();

        public void OnVisitSyntaxNode(GeneratorSyntaxContext context)
        {
            if (context.Node is ClassDeclarationSyntax {AttributeLists: {Count: > 0}} classDeclarationSyntax)
            {
                var symbol = context.SemanticModel.GetDeclaredSymbol(classDeclarationSyntax);
                if (symbol!.GetAttributes().Any(x =>
                    x.AttributeClass!.ToDisplayString() == ExcelProviderAttribute.TypeFullName))
                    CandidateClasses.Add(classDeclarationSyntax);
            }
        }
    }
}