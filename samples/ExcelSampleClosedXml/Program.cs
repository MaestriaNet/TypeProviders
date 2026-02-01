using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using ClosedXML.Excel;
using ExcelSampleClosedXml;
using Maestria.Extensions.FluentCast;

namespace ExcelSample
{
    static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Maestria Type Provider Sample");
            Console.WriteLine();

            var currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(currentDir, @"../../../../../resources/Excel.xlsx");

            LoadExcelWithClosedXml(filePath);
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
                    var id = row.Cell(sheet.ColumnByName("Id")).GetValue<int>();
                    var name = row.Cell(sheet.ColumnByName("Name")).GetText();
                    var valor = row.Cell(sheet.ColumnByName("Value")).TryGetValue<decimal>(out var outVal) ? outVal : (decimal?)null;
                    var prop = row.Cell(sheet.ColumnByName("Prop")).GetValue<int>();
                    var calculatedProp = row.Cell(sheet.ColumnByName("Calculated Prop")).GetValue<int>();
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
                .Where(x => x.Cell(columnIndex).Value.ToString() != string.Empty)
                .ToImmutableArray();
            if (rows.Length < 2)
                return "object";

            var cell = rows[1].Cell(columnIndex);
            if (cell.DataType == XLDataType.Text)
                return "string";

            var hasNull = sheet.RowsUsed().Any(x => x.Cell(columnIndex).CachedValue.ToString() == string.Empty);
            if (cell.DataType == XLDataType.Number)
            {
                var isDecimal = rows.Any(x => x.Cell(columnIndex).CachedValue.ToInt32Safe() != x.Cell(columnIndex).CachedValue.ToDecimalSafe());

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
