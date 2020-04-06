using NUnit.Framework;

namespace Expenses.FileImporter.Tests
{
    public class CsvImporterTests
    {
        [Test]
        public void ReturnsEmptyListGivenEmptyCSV()
        {
            var filePath = GetEmptyFilePath();

            var importer = new CSVImporter();
            var result = importer.ToStringList(filePath);

            Assert.AreEqual(0, result.Count);
        }

        [Test]
        public void Returns2ListElementGiven41TestData()
        {
            var filePath = GetDefaultTestFilePath();

            var importer = new CSVImporter();
            var result = importer.ToStringList(filePath);

            Assert.AreEqual(2, result.Count);
        }

        [Test]
        public void Returns9ElementsInOneRowGivenTestDataAutoDelimiter()
        {
            var filePath = GetDefaultTestFilePath();

            var importer = new CSVImporter();
            var result = importer.ToStringList(filePath);

            Assert.AreEqual(9, result[0].Count);
        }

        [Test]
        public void ReturnsCorrectValueFromGivenCellGivenTestDataAutoDelimiter()
        {
            var filePath = GetDefaultTestFilePath();

            var importer = new CSVImporter();
            var result = importer.ToStringList(filePath);

            Assert.AreEqual("-2,50", result[1][5]);
        }

        [Test]
        public void ReturnsCorrectValueFromGivenCellGivenTestDataSemicolonDelimiter()
        {
            var filePath = GetDefaultTestFilePath();

            var importer = new CSVImporter();
            var result = importer.ToStringList(filePath, ";");

            Assert.AreEqual("-2,50", result[1][5]);
        }

        [Test]
        public void ReturnsFalseNumberOfElementsInOneRowGivenTestDataCommaDelimiter()
        {
            var filePath = GetDefaultTestFilePath();

            var importer = new CSVImporter();
            var result = importer.ToStringList(filePath, ",");

            Assert.AreNotEqual(9, result[0].Count);
        }

        private string GetEmptyFilePath()
        {
            return @"TestFiles/empty.csv";
            //return @"empty.csv";
        }
        private string GetDefaultTestFilePath()
        {
            return @"TestFiles/historia_2018-11-22_3924.csv";
            //return @"./historia_2018-11-22_3924.csv";
        }
    }
}