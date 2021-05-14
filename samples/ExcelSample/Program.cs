using System;
using System.Collections.Immutable;
using System.Globalization;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using Maestria.FluentCast;
using Maestria.TypeProviders.Excel;

namespace ExcelSample
{
    [ExcelProviderAttribute(TemplatePath = @"../../resources/Excel.xlsx")]
    public partial class MyExcelData
    {
    }

    static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Maestria Type Provider Sample");
            Console.WriteLine();

            LoadExcelWithMaestria();
            //LoadExcelWithClosedXml();
        }

        private static void LoadExcelWithMaestria()
        {
            var exeDirectory = Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);

            Console.WriteLine("Excel.xlsx Maestria TypeProvider load");
            var filePath = Path.Combine(exeDirectory, @"../../../../../resources/Excel.xlsx");
            var data = MyExcelDataFactory.Load(filePath);
            foreach (var item in data)
                Console.WriteLine($"{item.Id}\t{item.Name}\t\t{item.Value:C2}\t\t{item.BirthDate:yyyy-MM-dd}");
        }

        private static void LoadExcelWithClosedXml()
        {
            const string filePath = @"..\..\..\..\..\resources\Excel.xlsx";
            using var workbook = new XLWorkbook(filePath);
            var sheet = workbook.Worksheet(1);

            Console.WriteLine("ColumnUsedCount: " + sheet.ColumnUsedCount());
            for (var i = 1; i <= sheet.ColumnUsedCount(); i++)
            {
                var headerCell = sheet.Row(1).Cell(i);
                var valueCell = sheet.Row(2).Cell(i);
                Console.WriteLine($"Column {i}: {headerCell.Value} = {valueCell.DataType} | {GetFieldDataType(sheet, i)}");
            }

            foreach (var row in sheet.Rows(2, sheet.Rows().Count()))
            {
                var id = row.Cell(sheet.ColumnByName("Id")).Value.ToInt32Safe();
                var name = row.Cell(sheet.ColumnByName("Name")).Value.ToStringSafe();
                var valor = row.Cell(sheet.ColumnByName("Valor")).Value.ToDecimalSafe();
                Console.WriteLine($"{id}\t| {name}\t| {valor?.ToString("C2")}");
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
