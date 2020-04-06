using CsvHelper;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;

namespace Expenses.FileImporter
{
    public class CSVImporter : IFileImporter
    {
        /// <summary>
        /// Parses the file into a collection of string lists.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <param name="delimiter">CSV delimiter (optional)</param>
        /// <returns></returns>
        public List<List<string>> ToStringList(string path, string delimiter = "")
        {
            var result = new List<List<string>>();
            using (var streamReader = new StreamReader(path))
            {
                using (var reader = new CsvReader(streamReader))
                {
                    //Set delimiter according to the current decimal separator.
                    if (delimiter == "" && CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ",")
                    {
                        delimiter = ";";
                    }
                    if (delimiter == "" && CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator == ".")
                    {
                        delimiter = ",";
                    }
                    reader.Configuration.Delimiter = delimiter;
                    reader.Configuration.BadDataFound = null;

                    //Read the file.
                    while (true)
                    {
                        reader.Read();
                        var temp = ToList(reader);
                        if (temp.Count > 0)
                        {
                            result.Add(temp);
                        }
                        else
                        {
                            return result;
                        }
                    }
                }
            }
        }

        private List<string> ToList(CsvReader reader)
        {
            var result = new List<string>();
            for (int i = 0; true; i++)
            {
                try
                {
                    result.Add(reader[i]);
                }
                catch (Exception)
                {
                    return result;
                }
            }
        }
    }
}
