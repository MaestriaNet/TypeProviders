using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ClosedXML.Excel;
using Maestria.TypeProviders.Attributes;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Maestria.TypeProviders.Generators
{
    [Generator]
    public class ExcelProviderGenerator : ISourceGenerator
    {
        private StringBuilder _log = new StringBuilder();

        private void Log(string value)
        {
            // _log.AppendLine(value);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            Log($"Execute: {DateTime.Now}");
                
            if (context.SyntaxReceiver is not SyntaxReceiver receiver)
                return;
            
            var classSymbols = GetClassSymbols(context, receiver);
            var classNames = new Dictionary<string, int>();
            
            foreach (var classSymbol in classSymbols)
            {
                Log($"Class Symbol: {classSymbol.Name}");
                classNames.TryGetValue(classSymbol.Name, out var i);
                var name = i == 0 ? classSymbol.Name : $"{classSymbol.Name}{i + 1}";
                classNames[classSymbol.Name] = i + 1;
                context.AddSource($"{name}.ExcelTypeProvider.g.cs",
                    SourceText.From(CreateExcelProvider(classSymbol), Encoding.UTF8));
            }

            if (_log.Length > 0)
            {
                var source = SourceText.From($"/*\n{_log}*/", Encoding.UTF8);
                context.AddSource("Debug.cs", source);
            }
        }

        private string CreateExcelProvider(INamedTypeSymbol classSymbol)
        {
            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            var columns = GetExcelColumns();
             
             var source = new StringBuilder(
$@"using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ClosedXML.Excel;
using Maestria.FluentCast;

namespace {namespaceName}
{{
    public partial class {classSymbol.Name}
    {{
");
             foreach (var column in columns)
             {
                 source.Append($"        {column.GetSourceCode()}\r\n");
             }

             source.Append(@$"
    }}

    public static partial class {classSymbol.Name}Factory
    {{
        public static IEnumerable<{classSymbol.Name}> Load(Stream input)
        {{
            using var workbook = new XLWorkbook(input);
            return Load(workbook);
        }}

        public static IEnumerable<{classSymbol.Name}> Load(string filePath)
        {{
            using var workbook = new XLWorkbook(filePath);
            return Load(workbook);
        }}

        public static IEnumerable<{classSymbol.Name}> Load(XLWorkbook workbook)
        {{
            var result = new List<{classSymbol.Name}>();
            var sheet = workbook.Worksheet(1);
            foreach (var row in sheet.Rows(2, sheet.Rows().Count()))
            {{
");
            foreach (var column in columns)
            {
                source.Append($"                var {column.PropertyName.ToCamelCase()}Value = row.Cell(sheet.ColumnByName(\"{column.SourceName}\")).Value;\r\n");
            }
            
            source.Append(@$"                result.Add(new {classSymbol.Name}
                {{
");
            foreach (var column in columns)
            {
                source.Append($"                    {column.PropertyName} = {column.GetCastSourceCode(column.PropertyName.ToCamelCase() + "Value")},\r\n");
            }
            source.Append("                });\r\n");

            source.Append(@$"            }}
            return result;
        }}
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

        private static IEnumerable<Models.Field> GetExcelColumns()
        {
            const string filePath = @"C:\sources\open-source\maestria\TypeProviders\resources\Excel.xlsx";
            using var workbook = new XLWorkbook(filePath);
            var sheet = workbook.Worksheet("Plan1");
            for (var i = 1; i <= sheet.ColumnUsedCount(); i++)
                yield return new Models.Field
                {
                    SourceName = sheet.Row(1).Cell(i).Value.ToString(),
                    PropertyName = sheet.Row(1).Cell(i).Value.ToString().Replace(" ", ""),
                    DataType = GetFieldDataType(sheet.Row(2).Cell(i).DataType)
                };
        }

        private static string GetFieldDataType(XLDataType dataType) => dataType switch
        {
            XLDataType.Boolean => "bool",
            XLDataType.Number => "int",
            XLDataType.Text => "string",
            XLDataType.DateTime => "DateTime",
            XLDataType.TimeSpan => "TimeSpan",
            _ => throw new ArgumentOutOfRangeException(nameof(dataType), $"Not expected excel column data type: {dataType}")
        };
    }
}