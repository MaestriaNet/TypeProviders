using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Maestria.Extensions.FluentCast;
using Maestria.TypeProviders.Excel;

// Top level statement support
[ExcelProvider(TemplatePath = @"../../resources/Excel.xlsx")]
public partial class MyExcelDataGlobalNamespace
{
}

namespace ExcelSample
{
    // Default is used sheet 1
    [ExcelProvider(TemplatePath = @"../../resources/Excel.xlsx")]
    public partial class MyExcelData
    {
    }

    // Its possible set "SheetName" or "SheePosition" to load data struct of another page
    [ExcelProvider(TemplatePath = @"../../resources/Excel.xlsx", SheetName = "Plan2")]
    public partial class MyExcelDataSheet2
    {
    }

    static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Maestria Type Provider Sample");
            Console.WriteLine();

            var currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(currentDir, @"../../../../../resources/Excel.xlsx");

            LoadExcelWithMaestria(filePath);
            //LoadExcelWithClosedXml(filePath);
        }

        private static void LoadExcelWithMaestria(string filePath)
        {
            Console.WriteLine("Excel.xlsx Maestria TypeProvider load");

            // Load first page
            var sheet1 = MyExcelDataFactory.Load(filePath);
            Console.WriteLine("Sheet1");
            Console.WriteLine($"| {"Id",3} | {"Name",-10} | {"Value",10} | {"BirthDate",10} | {"Prop",5} | {"Calculated Prop",15} | {"Prop_Sample_AB",15}");
            foreach (var item in sheet1)
                Console.WriteLine($"| {item.Id,3} | {item.Name,-10} | {item.Value,10:C2} | {item.BirthDate,10:yyyy-MM-dd} | {item.Prop,5} | {item.CalculatedProp,15} | {item.Prop_Sample_AB,15}");

            Console.WriteLine(new string('-', 100));

            // Load second page
            var sheet2 = MyExcelDataSheet2Factory.Load(filePath, "Plan2");
            Console.WriteLine("Sheet2");
            Console.WriteLine($"| {"Id",3} | {"Name",-10} | {"Value",10} | {"BirthDate",10} | {"Prop",5} | {"Calculated Prop",15} | {"Prop Sheet 2",15} |");
            foreach (var item in sheet2)
                Console.WriteLine($"| {item.Id,3} | {item.Name,-10} | {item.Value,10:C2} | {item.BirthDate,10:yyyy-MM-dd} | {item.Prop,5} | {item.CalculatedProp,15} | {item.PropSheet2,15} |");
        }

        private static void LoadExcelWithClosedXml(string filePath)
        {
            Console.WriteLine("Excel.xlsx ClosedXML load");

            FileStream file;
            try
            {
                file = new FileStream(filePath, FileMode.Open, FileAccess.Read);
            }
            catch
            {
                // Necessárias mais permissões de acesso ao arquivo já em uso pelo sistema operacional
                file = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            }
            using var workbook = new XLWorkbook(file);
            try
            {
                var sheet = workbook.Worksheet(1);

                Console.WriteLine("ColumnUsedCount: " + sheet.LastColumnUsed().ColumnNumber());
                Console.WriteLine("RowUsedCount: " + sheet.LastRowUsed().RowNumber());
                for (var i = 1; i <= sheet.ColumnUsedCount(); i++)
                {
                    var headerCell = sheet.Row(1).Cell(i);
                    var valueCell = sheet.Row(2).Cell(i);
                    Console.WriteLine($"Column {i}: {headerCell.Value} = {valueCell.DataType} | {GetFieldDataType(sheet, i)}");
                }

                foreach (var row in sheet.Rows(2, sheet.RowsUsed().Count()))
                {
                    var id = row.Cell(sheet.ColumnByName("Id")).Value.ToInt32Safe();
                    var name = row.Cell(sheet.ColumnByName("Name")).Value.ToStringSafe();
                    var valor = row.Cell(sheet.ColumnByName("Value")).Value.ToDecimalSafe();
                    var prop = row.Cell(sheet.ColumnByName("Prop")).Value.ToInt32();
                    var calculatedProp = row.Cell(sheet.ColumnByName("Calculated Prop")).Value.ToInt32();
                    Console.WriteLine($"| {id,3} | {name,-10} | {valor,10:C2} | {prop,5} | {calculatedProp,5} |");
                }
            }
            finally
            {
                file.Dispose();
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
    }
}
