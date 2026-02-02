using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Text;
using ClosedXML.Excel;
using Maestria.Extensions;
using Maestria.Extensions.FluentCast;
using Maestria.TypeProviders.Common;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Maestria.TypeProviders.Excel
{
    [Generator]
    public class ExcelGenerator : IIncrementalGenerator
    {
        private static readonly char[] UpperCharsAfterWhenGenerateDotnetPropertyName = { ' ', '/', '\\', '-', '_' };
        
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
#if DEBUG
            if (!System.Diagnostics.Debugger.IsAttached)
                System.Diagnostics.Debugger.Launch();
#endif
            // Registrar a fonte de atributo
            context.RegisterPostInitializationOutput(ctx =>
                ctx.AddSource("ExcelProviderAttribute.cs",
                    Extensions.GetEmbeddedSourceCode("Maestria.TypeProviders.Excel.ExcelProviderAttribute.cs")));

            // Provider para classes com atributo ExcelProvider
            var classProvider = context.SyntaxProvider
                .CreateSyntaxProvider(
                    predicate: (syntax, _) => IsCandidateClass(syntax),
                    transform: (syntaxContext, _) => GetSemanticTarget(syntaxContext))
                .Where(x => x is not null)
                .Collect();

            // // Provider para compilation
            var compilationProvider = context.CompilationProvider;
            //
            // // Combinar os providers
            var combined = classProvider.Combine(compilationProvider);
            //
            context.RegisterSourceOutput(combined, Execute);
        }

        private static bool IsCandidateClass(SyntaxNode syntax)
        {
            return syntax is ClassDeclarationSyntax classDecl && 
                   classDecl.AttributeLists.Count > 0;
        }

        private static INamedTypeSymbol GetSemanticTarget(GeneratorSyntaxContext context)
        {
            var classDecl = (ClassDeclarationSyntax)context.Node;
            var symbol = context.SemanticModel.GetDeclaredSymbol(classDecl);
            
            if (symbol?.HasAttribute(ExcelProviderAttribute.TypeFullName) == true)
                return symbol;
            
            return null;
        }

        private void Execute(SourceProductionContext context, (ImmutableArray<INamedTypeSymbol> classes, Compilation compilation) pair)
        {
            var (classes, _) = pair;

            if (classes.Length == 0)
                return;

            // Adicionar ExcelExtensions uma única vez
            context.AddSource("ExcelExtensions.cs",
                Extensions.GetEmbeddedSourceCode("Maestria.TypeProviders.Excel.ExcelExtensions.cs"));

            
            // context.ReportDiagnostic(Diagnostic.Create(DiagnosticErrors.Generic, classes[0].Locations.First(), "aaa", "bbb"));
            // Gerar código para cada classe
            foreach (var classSymbol in classes)
            {
                try
                {
                    var name = classSymbol.ToDisplayString();
                    var sourceCode = GenerateSourceCode(classSymbol);
                    context.AddSource(
                        $"{name}.ExcelTypeProvider.cs",
                        SourceText.From(sourceCode, Encoding.UTF8));
                }
                catch (Exception e)
                {
                    foreach (var location in classSymbol.Locations)
                        context.ReportDiagnostic(Diagnostic.Create(DiagnosticErrors.Generic, location, classSymbol.Name, e.ToString().Replace(Environment.NewLine, " ===> ")));
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


        private static IEnumerable<FieldMapInfo> GetExcelColumns(INamedTypeSymbol classSymbol, ExcelProviderAttribute attributes)
        {
            var templatePath = attributes.TemplatePath;
            if (!File.Exists(templatePath))
            {
                var location = classSymbol.Locations.FirstOrDefault();
                var basePath = string.IsNullOrWhiteSpace(location?.GetLineSpan().Path) ? "" : Path.GetDirectoryName(location.GetLineSpan().Path);
                templatePath = Path.Combine(basePath, templatePath);
                if (!File.Exists(templatePath))
                    throw new FileNotFoundException($"Excel template file not found: {attributes.TemplatePath}");
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
                .Where(x => !x.Cell(columnIndex).IsEmpty())
                .ToArray();
            if (rows.Length < 2)
                return "object";

            var valueRows = new IXLRow[rows.Length - 1];
            Array.Copy(rows, 1, valueRows, 0, valueRows.Length);

            var cell = valueRows[0].Cell(columnIndex);
            if (cell.DataType == XLDataType.Text)
                return "string";

            var hasNull = valueRows.Any(x => x.Cell(columnIndex).IsEmpty());
            if (cell.DataType == XLDataType.Number)
            {
                var allNonEmptyIsInt = valueRows
                    .Select(x => x.Cell(columnIndex))
                    .Where(x => !x.IsEmpty())
                    .All(x => x.DataType == XLDataType.Number && x.TryGetValue<long>(out _));
                
                return $"{(allNonEmptyIsInt ? "int" : "decimal")}{(hasNull ? "?" : string.Empty)}";
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