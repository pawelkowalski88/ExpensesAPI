using System;
using System.Collections.Generic;
using System.Text;

namespace Expenses.FileImporter
{
    public interface IFileImporter
    {
        List<List<string>> ToStringList(string path, string delimiter = "");
    }
}
