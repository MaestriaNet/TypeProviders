using NUnit.Framework;
using System.IO;

// Top level statement support
[ExcelProvider(TemplatePath = @"../../resources/Excel.xlsx")]
public partial class MyExcelDataGlobalNamespace
{
}

namespace Maestria.TypeProviders.Test.ExcelProvider
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

    public class ExcelProviderTest
    {
        [Test]
        public void Validated_Generated_ExcelExtensions()
        {
            var generated = File.ReadAllLines("../../../../generated/tests/Maestria.TypeProviders/Maestria.TypeProviders.Excel.ExcelGenerator/ExcelExtensions.cs");
            var expected = File.ReadAllLines("../../../ExcelProvider/ExpectedFiles/ExcelExtensions.expected");
            Assert.AreEqual(expected, generated);
        }

        [Test]
        public void Validated_Generated_ExcelProviderAttribute()
        {
            var generated = File.ReadAllLines("../../../../generated/tests/Maestria.TypeProviders/Maestria.TypeProviders.Excel.ExcelGenerator/ExcelProviderAttribute.cs");
            var expected = File.ReadAllLines("../../../ExcelProvider/ExpectedFiles/ExcelProviderAttribute.expected");
            Assert.AreEqual(expected, generated);
        }

        [Test]
        public void Validated_Generated_MyExcelData()
        {
            var generated = File.ReadAllLines("../../../../generated/tests/Maestria.TypeProviders/Maestria.TypeProviders.Excel.ExcelGenerator/Maestria.TypeProviders.Test.ExcelProvider.MyExcelData.ExcelTypeProvider.cs");
            var expected = File.ReadAllLines("../../../ExcelProvider/ExpectedFiles/Maestria.TypeProviders.Test.ExcelProvider.MyExcelData.ExcelTypeProvider.expected");
            Assert.AreEqual(expected, generated);
        }

        [Test]
        public void Validated_Generated_MyExcelDataSheet2()
        {
            var generated = File.ReadAllLines("../../../../generated/tests/Maestria.TypeProviders/Maestria.TypeProviders.Excel.ExcelGenerator/Maestria.TypeProviders.Test.ExcelProvider.MyExcelDataSheet2.ExcelTypeProvider.cs");
            var expected = File.ReadAllLines("../../../ExcelProvider/ExpectedFiles/Maestria.TypeProviders.Test.ExcelProvider.MyExcelDataSheet2.ExcelTypeProvider.expected");
            Assert.AreEqual(expected, generated);
        }

        [Test]
        public void Validated_Generated_MyExcelDataGlobalNamespace()
        {
            var generated = File.ReadAllLines("../../../../generated/tests/Maestria.TypeProviders/Maestria.TypeProviders.Excel.ExcelGenerator/MyExcelDataGlobalNamespace.ExcelTypeProvider.cs");
            var expected = File.ReadAllLines("../../../ExcelProvider/ExpectedFiles/MyExcelDataGlobalNamespace.ExcelTypeProvider.expected");
            Assert.AreEqual(expected, generated);
        }
    }
}