using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using ClosedXML.Excel;
using Maestria.Extensions;
using Maestria.Extensions.FluentCast;
using Maestria.TypeProviders.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace Maestria.TypeProviders.Excel
{
    [Generator]
    public class ExcelGenerator : ISourceGenerator
    {
        private static readonly char[] UpperCharsAfterWhenGenerateDotnetPropertyName = { ' ', '/', '\\', '-', '_' };
        public void Initialize(GeneratorInitializationContext context)
        {
/*#if DEBUG
            if (!Debugger.IsAttached)
                Debugger.Launch();
#endif*/
            context.RegisterForSyntaxNotifications(() => new ExcelContextReceiver());
            context.RegisterForPostInitialization(ctx =>
                ctx.AddSource("ExcelProviderAttribute.cs",
                    Extensions.GetEmbeddedSourceCode("Maestria.TypeProviders.Excel.ExcelProviderAttribute.cs")));
        }

        public void Execute(GeneratorExecutionContext context)
        {
            if (context.SyntaxContextReceiver is not ExcelContextReceiver receiver)
                return;

            var classSymbols = GetCandidateClassSymbols(context, receiver);

            if (classSymbols.Any())
                context.AddSource("ExcelExtensions.cs",
                    Extensions.GetEmbeddedSourceCode("Maestria.TypeProviders.Excel.ExcelExtensions.cs"));

            foreach (var classSymbol in classSymbols)
            {
                try
                {
                    var name = classSymbol.ToDisplayString();
                    context.AddSource(
                        $"{name}.ExcelTypeProvider.cs",
                        SourceText.From(GenerateSourceCode(classSymbol), Encoding.UTF8));
                }
                catch (Exception e)
                {
                    context.ReportDiagnostic(Diagnostic.Create(DiagnosticErrors.Generic, classSymbol.Locations.First(), classSymbol.Name, e.Message));
                }
            }
        }

        private string GenerateSourceCode(INamedTypeSymbol classSymbol)
        {
            var attribute = GetExcelProviderAttribute(classSymbol);
            var columns = GetExcelColumns(classSymbol, attribute);

            var namespaceName = classSymbol.ContainingNamespace.ToDisplayString();
            var isGlobalNamespace = namespaceName.IsGlobalNamespace();

            var options = new TemplateEngineOptions(columns, isGlobalNamespace, namespaceName, classSymbol.Name);
            var template = new TemplateEngine(options).Generate();
            return template;
        }

        private ExcelProviderAttribute GetExcelProviderAttribute(INamedTypeSymbol classSymbol)
        {
            var attributes = classSymbol.GetAttributes()
                .Single(x => x.AttributeClass!.ToDisplayString() == ExcelProviderAttribute.TypeFullName)
                .NamedArguments;

            var templatePathAtt = attributes.FirstOrDefault(x => x.Key == nameof(ExcelProviderAttribute.TemplatePath));
            var sheetPositionAtt = attributes.FirstOrDefault(x => x.Key == nameof(ExcelProviderAttribute.SheetPosition));
            var sheetNameAtt = attributes.FirstOrDefault(x => x.Key == nameof(ExcelProviderAttribute.SheetName));

            var templatePath = templatePathAtt.Value.Value.ToString();
            var sheetPosition = sheetPositionAtt.Value.Value.ToInt32Safe();
            var sheetName = sheetNameAtt.Value.Value.ToStringSafe();

            var result = new ExcelProviderAttribute();
            result.TemplatePath = templatePath;
            result.SheetName = sheetName;
            if (sheetPosition.HasValue)
                result.SheetPosition = sheetPosition.Value;
            return result;
        }

        private static IEnumerable<INamedTypeSymbol> GetCandidateClassSymbols(GeneratorExecutionContext context, ExcelContextReceiver receiver)
        {
            var compilation = context.Compilation;
            return receiver.CandidateClasses
                .Select(x => compilation
                    .GetSemanticModel(x.SyntaxTree)
                    .GetDeclaredSymbol(x))
                .Where(x => x.HasAttribute(ExcelProviderAttribute.TypeFullName));
        }

        private static IEnumerable<FieldMapInfo> GetExcelColumns(INamedTypeSymbol classSymbol, ExcelProviderAttribute attributes)
        {
            var templatePath = attributes.TemplatePath;
            if (!File.Exists(templatePath))
            {
                var location = classSymbol.Locations.FirstOrDefault();
                var basePath = string.IsNullOrWhiteSpace(location?.GetLineSpan().Path) ? "" : Path.GetDirectoryName(location.GetLineSpan().Path);
                templatePath = Path.Combine(basePath, templatePath);
                if (!File.Exists(templatePath))
                    throw new ArgumentException($"Excel template file not found: {attributes.TemplatePath}");
            }

            using var workbook = ExcelExtensions.OpenWorkbook(templatePath);
            var sheet = attributes.SheetName.IsNullOrEmpty()
                ? workbook.Worksheet(attributes.SheetPosition)
                : workbook.Worksheet(attributes.SheetName);
            for (var i = 1; i <= sheet.ColumnUsedCount(); i++)
            {
                var headerCell = sheet.Row(1).Cell(i);
                yield return new FieldMapInfo
                {
                    SourceName = headerCell.Value.ToString(),
                    PropertyName = GetValidDotnetPropertyName(headerCell.Value.ToString()),
                    DataType = GetFieldDataType(sheet, i)
                };
            }
        }

        private static string GetValidDotnetPropertyName(string columnName)
        {
            foreach (var findChar in UpperCharsAfterWhenGenerateDotnetPropertyName)
            {
                var pos = columnName.IndexOf(findChar);
                while (pos > -1 && pos < columnName.Length - 1)
                {
                    var tempChar = columnName[pos + 1];
                    columnName = columnName.Remove(pos + 1, 1).Insert(pos + 1, tempChar.ToString().ToUpperInvariant());
                    pos = columnName.IndexOf(findChar, pos + 1);
                }
            }

            return columnName
                .Replace(" ", "")
                .Replace("-", "")
                .Replace("/", "_")
                .Replace(@"\", "_");
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
                _ => throw new ArgumentOutOfRangeException(nameof(cell.DataType),
                    $"Not expected excel column data type: {cell.DataType}")
            };
            if (hasNull)
                dataType += "?";
            return dataType;
        }
    }
}