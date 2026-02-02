using NUnit.Framework;

// Top level statement support
[ExcelProvider(TemplatePath = @"./TestFile.xlsx")]
public partial class MyExcelDataGlobalNamespace
{
}

namespace Maestria.TypeProviders.Test.ExcelProvider
{
    // Default is used sheet 1
    [ExcelProvider(TemplatePath = @"./TestFile.xlsx")]
    public partial class MyExcelData
    {
    }
    
    // // Its possible set "SheetName" or "SheePosition" to load data struct of another page
    [ExcelProvider(TemplatePath = @"./TestFile.xlsx", SheetName = "Plan2")]
    public partial class MyExcelDataSheet2
    {
    }

    public class ExcelProviderTest
    {
        [TestCase("../../../../generated/tests/Maestria.TypeProviders/Maestria.TypeProviders.Excel.ExcelGenerator/ExcelExtensions.g.cs")]
        [TestCase("../../../../generated/tests/Maestria.TypeProviders/Maestria.TypeProviders.Excel.ExcelGenerator/ExcelGeneratorException.g.cs")]
        [TestCase("../../../../generated/tests/Maestria.TypeProviders/Maestria.TypeProviders.Excel.ExcelGenerator/ExcelProviderAttribute.g.cs")]
        public void GeneratedFile_AreExists_AndNotEmpty(string path)
        {
            Assert.That(File.Exists(path), "Expected source generator not create expected file: " + path);
            
            var fileContent = File.ReadAllText(path);
            Assert.That(fileContent, Is.Not.Empty, "Expected source generator create a empty file: " + path);
        }

        [Test]
        public void LoadExcelFile_MustBe_Success()
        {
            var filePath = @"../../../ExcelProvider/TestFile.xlsx";
            var data = MyExcelDataFactory.Load(filePath).ToArray();
            Assert.That(data, Is.Not.Null);
            Assert.That(data.Length, Is.EqualTo(6));

            for (var arrayIndex = 0; arrayIndex < data.Length; arrayIndex++)
            {
                var rowNumber = arrayIndex + 1;
                Assert.That(data[arrayIndex].Id, Is.EqualTo(rowNumber));
                Assert.That(data[arrayIndex].Name, Is.EqualTo($"Name {rowNumber}"));
                Assert.That(data[arrayIndex].BirthDate, Is.EqualTo(new DateTime(1987, 03, rowNumber)));
                Assert.That(data[arrayIndex].Prop, Is.EqualTo(rowNumber));
                Assert.That(data[arrayIndex].CalculatedProp, Is.EqualTo(rowNumber + 10));
                Assert.That(data[arrayIndex].PropSample, Is.EqualTo($"Teste {rowNumber + 10}"));
                Assert.That(data[arrayIndex].PropSample2, Is.EqualTo($"Teste {rowNumber + 20}"));
                Assert.That(data[arrayIndex].Prop_Sample3, Is.EqualTo($"Teste {rowNumber + 30}"));
                Assert.That(data[arrayIndex].Prop_Sample4, Is.EqualTo($"Teste {rowNumber + 40}"));
                Assert.That(data[arrayIndex].Prop_Sample5, Is.EqualTo($"Teste {rowNumber + 50}"));
                Assert.That(data[arrayIndex].Prop_Sample_AB, Is.EqualTo($"Teste {rowNumber + 60}"));
            }
            
            Assert.That(data[0].Value, Is.Null);
            Assert.That(data[1].Value, Is.EqualTo(5));
            Assert.That(data[2].Value, Is.EqualTo(5.01d));
            Assert.That(data[3].Value, Is.EqualTo(7d));
            Assert.That(data[4].Value, Is.Null);
            Assert.That(data[5].Value, Is.EqualTo(9d));
        }
    }
}