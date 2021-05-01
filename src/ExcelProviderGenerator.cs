using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClosedXML.Excel;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Maestria.TypeProviders
{
    [Generator]
    public class ExcelProviderGenerator : ISourceGenerator
    {
        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            var sourceText = SourceText.From(@"
        namespace GeneratedClass
        {
            public class SeattleCompanies
            {
                public string ForTheCloud => ""Microsoft"";
                public string ForTheTwoDayShipping => ""Amazon"";
                public string ForTheExpenses => ""Concur"";
            }
        }", Encoding.UTF8);
            context.AddSource("SeattleCompanies.cs", sourceText);
            
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                return;
            
            
            
            var classSymbols = GetClassSymbols(context, receiver);
            var classNames = new Dictionary<string, int>();
            
            foreach (var classSymbol in classSymbols)
            {
                classNames.TryGetValue(classSymbol.Name, out var i);
                var name = i == 0 ? classSymbol.Name : $"{classSymbol.Name}{i + 1}";
                classNames[classSymbol.Name] = i + 1;
                context.AddSource($"{name}.ExcelTypeProvider.g.cs",
                    SourceText.From(CreateExcelProvider(classSymbol), Encoding.UTF8));
            }
        }

        private static string CreateExcelProvider(INamedTypeSymbol classSymbol)
        {
            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            //var memberList = GetMembers(classSymbol, false);
            var columns = GetExcelColumns();
            
            var source = new StringBuilder($@"namespace {namespaceName}
{{
    public partial class {classSymbol.Name} 
    {{");
            foreach (var column in columns)
            {
                source.Append($@"/* {column.Name}: {column.DataType} */");
            }
            source.Append(@"
    }}
}}");
            return source.ToString();
        }

        private static IEnumerable<INamedTypeSymbol> GetClassSymbols(GeneratorExecutionContext context, SyntaxReceiver receiver)
        {
            var compilation = context.Compilation;

            /*return receiver.CandidateClasses
                .Select(x => compilation
                    .GetSemanticModel(x.SyntaxTree)
                    .GetDeclaredSymbol(x))
                .Where(x => HasAttribute(x, nameof(ExcelProviderAttribute)));*/
            return from clazz in receiver.CandidateClasses
                let model = compilation.GetSemanticModel(clazz.SyntaxTree)
                select model.GetDeclaredSymbol(clazz)! into classSymbol
                where HasAttribute(classSymbol, nameof(ExcelProviderAttribute))
                select classSymbol;
        }
        
        private static bool HasAttribute(ISymbol symbol, string name) => symbol
            .GetAttributes()
            .Any(x => x.AttributeClass?.Name == name);

        private static IEnumerable<(string Name, XLDataType DataType)> GetExcelColumns()
        {
            return new List<(string Name, XLDataType DataType)>();
            /*const string filePath = @"C:\sources\open-source\maestria\TypeProviders\resources\Excel.xlsx";
            using var workbook = new XLWorkbook(filePath);
            var sheet = workbook.Worksheet("Plan1");
            for (var i = 1; i <= sheet.ColumnUsedCount(); i++)
                yield return (
                    Name: sheet.Row(1).Cell(i).Value.ToString(), 
                    DataType: sheet.Row(2).Cell(i).DataType);*/
        }
    }
}