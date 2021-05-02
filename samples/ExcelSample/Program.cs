using System;
using System.Globalization;
using System.Linq;
using ClosedXML.Excel;
using Maestria.FluentCast;
using Maestria.TypeProviders.Attributes;

namespace ExcelSample
{
    [ExcelProvider]
    public partial class MyExcelData
    {
    }
    
    static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Excel Sample");
            const string filePath = @"C:\sources\open-source\maestria\TypeProviders\resources\Excel.xlsx"; 
            var culture = CultureInfo.GetCultureInfo("pt-BR");
            using var workbook = new XLWorkbook(filePath);
            var sheet = workbook.Worksheet("Plan1");

            //Console.WriteLine("Id DataType: " + sheet.Column(1).Cell(2).DataType);
            //Console.WriteLine("Name DataType: " + sheet.Column(2).Cell(2).DataType);
            
            Console.WriteLine("ColumnUsedCount: " + sheet.ColumnUsedCount());
            for (var i = 1; i <= sheet.ColumnUsedCount(); i++)
                Console.WriteLine($"Column {i}: {sheet.Row(1).Cell(i).Value} = {sheet.Row(2).Cell(i).DataType}");

            foreach (var row in sheet.Rows(2, sheet.Rows().Count()))
            {
                var id = row.Cell(sheet.ColumnByName("Id")).Value.ToInt32();
                var name = row.Cell(sheet.ColumnByName("Name")).Value.ToString();
                Console.WriteLine($"{id} | {name}");
            }

            //var teste = new GeneratedClass.SeattleCompanies();
        }
    }
}