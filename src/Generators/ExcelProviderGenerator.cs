﻿using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using ClosedXML.Excel;
using Maestria.FluentCast;
using Maestria.TypeProviders.Core;
using Maestria.TypeProviders.Generators.AttributesCopy;
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
            _log.AppendLine(value);
        }

        public void Initialize(GeneratorInitializationContext context)
        {
            context.RegisterForSyntaxNotifications(() => new SyntaxReceiver());
        }

        public void Execute(GeneratorExecutionContext context)
        {
            Log($"Execute: {DateTime.Now}");
            Log(new string('-', 50));
            
            if (context.SyntaxContextReceiver is not SyntaxReceiver receiver)
                return;

            var classSymbols = GetClassSymbols(context, receiver);
            foreach (var classSymbol in classSymbols)
            {
                Log($"Class Symbol: {classSymbol.ToDisplayString()}");
                var name = classSymbol.ToDisplayString();
                context.AddSource(
                    $"{name}.ExcelTypeProvider.cs",
                    SourceText.From(CreateExcelProvider(classSymbol), Encoding.UTF8));
            }
            
            context.AddSource("ExcelExtensions.cs", SourceText.From(GetExcelExtensionsSourceCode(), Encoding.UTF8));

            if (_log.Length > 0)
            {
                var source = SourceText.From($"/*\n{_log}*/", Encoding.UTF8);
                context.AddSource("Debug.cs", source);
            }
        }

        private ExcelProviderCopyAttribute GetExcelProviderAttribute(INamedTypeSymbol symbol)
        {
            var location = symbol.Locations.FirstOrDefault();
            var basePath = string.IsNullOrWhiteSpace(location?.GetLineSpan().Path) ? "" : Path.GetDirectoryName(location.GetLineSpan().Path); 
            Log($"basePath: {basePath}");
            
            var attributes = symbol.GetAttributes()
                .Single(x => x.AttributeClass!.ToDisplayString() == ExcelProviderAttribute.TypeFullName)
                .NamedArguments;

            var templatePathAtt =
                attributes.FirstOrDefault(x => x.Key == nameof(ExcelProviderCopyAttribute.TemplatePath));
            var templatePath = templatePathAtt.Value.Value.ToString();

            if (!File.Exists(templatePath))
                templatePath = Path.Combine(basePath, templatePath);
            
            var result = new ExcelProviderCopyAttribute();
            result.TemplatePath = templatePath;
            return result;
        }

        private string CreateExcelProvider(INamedTypeSymbol classSymbol)
        {
            var attribute = GetExcelProviderAttribute(classSymbol);
            Log($"TemplatePath: {attribute.TemplatePath}");
            if (!File.Exists(attribute.TemplatePath))
                throw new ArgumentException($"Excel template file not found: {attribute.TemplatePath}");
            
            
            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            var columns = GetExcelColumns(attribute.TemplatePath);
             
             var source = new StringBuilder($@"
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using ClosedXML.Excel;
using Maestria.FluentCast;
using Maestria.TypeProviders.Generators;

namespace {namespaceName}
{{
    public partial class {classSymbol.Name}
    {{
");
             foreach (var column in columns) 
                 source.Append($"        {column.GetSourceCode()}\r\n");

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

            return receiver.CandidateClasses
                .Select(x => compilation
                    .GetSemanticModel(x.SyntaxTree)
                    .GetDeclaredSymbol(x))
                .Where(x => HasAttribute(x, ExcelProviderAttribute.TypeFullName));
        }
        
        private static bool HasAttribute(ISymbol symbol, string typeFullName) => symbol
            .GetAttributes()
            .Any(x => x.AttributeClass?.ToDisplayString() == typeFullName);

        private static IEnumerable<Models.Field> GetExcelColumns(string filePath)
        {
            using var workbook = new XLWorkbook(filePath);
            var sheet = workbook.Worksheet("Plan1");
            for (var i = 1; i <= sheet.ColumnUsedCount(); i++)
            {
                var headerCell = sheet.Row(1).Cell(i);
                yield return new Models.Field
                {
                    SourceName = headerCell.Value.ToString(),
                    PropertyName = headerCell.Value.ToString().Replace(" ", ""),
                    DataType = GetFieldDataType(sheet, i)
                };
            }
        }

        private static string GetFieldDataType(IXLWorksheet sheet, int columnIndex)
        {
            var rows = sheet.RowsUsed()
                .Where(x =>
                    x.Cell(columnIndex).Value != null && 
                    x.Cell(columnIndex).Value.ToString() != string.Empty)
                .ToImmutableArray();
            if (rows.Length < 2)
                return "object";
            
            var cell = rows[1].Cell(columnIndex);
            if (cell.DataType == XLDataType.Text)
                return "string";
            
            var hasNull = sheet.RowsUsed().Any(x => 
                x.Cell(columnIndex).CachedValue == null ||
                x.Cell(columnIndex).CachedValue.ToString() == string.Empty);
            if (cell.DataType == XLDataType.Number)
            {
                var isDecimal = rows.Any(x =>
                    x.Cell(columnIndex).CachedValue != null &&
                    x.Cell(columnIndex).CachedValue.ToInt32Safe() != x.Cell(columnIndex).CachedValue.ToDecimalSafe());

                return (isDecimal ? "decimal" : "int") + (hasNull ? "?" : "");
            }
            
            var dataType = cell.DataType switch
            {
                XLDataType.Boolean => "bool",
                XLDataType.DateTime => "DateTime",
                XLDataType.TimeSpan => "TimeSpan",
                _ => throw new ArgumentOutOfRangeException(nameof(cell.DataType), $"Not expected excel column data type: {cell.DataType}")
            };
            if (hasNull)
                dataType += "?";
            return dataType;
        }

        private static string GetExcelExtensionsSourceCode()
        {
            var source = new StringBuilder();
            source.AppendLine(@"
using System;
using ClosedXML.Excel;

namespace Maestria.TypeProviders.Generators
{
    public static class ExcelExtensions
    {
        public static int ColumnUsedCount(this IXLWorksheet sheet)
        {
            return sheet.LastColumnUsed().ColumnNumber();
        }
    
        public static int ColumnByName(this IXLWorksheet sheet, string columnName)
        {
            if (string.IsNullOrWhiteSpace(columnName)) throw new ArgumentNullException(nameof(columnName), ""Informe o nome da coluna para obter posição!"");
            if (sheet.RowCount() <= 0) return 0;
            for (var i = 1; i <= sheet.ColumnUsedCount(); i++)
                if (sheet.Row(1).Cell(i).Value.ToString()?.ToUpper() == columnName.ToUpper()) return i;
            return 0;
        }
    }
}
");
            return source.ToString();
        }
    }
}