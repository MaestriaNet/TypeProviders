using System;
using System.IO;

// Console.WriteLine("aaa");

// Top level statement support
[ExcelProvider(TemplatePath = @"../../resources/Excel.xlsx")]
public partial class MyExcelDataTopLevelStatement;

namespace ExcelSample
{
    // Default is used sheet 1
    [ExcelProvider(TemplatePath = @"../../resources/Excel.xlsx")]
    public partial class MyExcelData;

    // Its possible set "SheetName" or "SheePosition" to load data struct of another page
    [ExcelProvider(TemplatePath = @"../../resources/Excel.xlsx", SheetName = "Plan2")]
    public partial class MyExcelDataSheet2;

    static class Program
    {
        public static void Main(string[] args)
        {
            Console.WriteLine("Maestria Type Provider Sample");
            Console.WriteLine();

            var currentDir = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            var filePath = Path.Combine(currentDir, @"../../../../../resources/Excel.xlsx");

            LoadExcelWithMaestria(filePath);
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
    }
}
