using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OfficeOpenXml;

namespace TestFramework.TestHelpers.Attachements
{
    public class Xlsx
    {
        private readonly ExcelPackage _excelPackage;
        public Xlsx(byte[] data)
        {
            _excelPackage = new ExcelPackage(new MemoryStream(data));
        }

        public IEnumerator<IEnumerable<string>> GetColumnFromAllWorksheets(string column, int offset)
        {
            return _excelPackage.Workbook.Worksheets
                .Select(ws => ws.Cells[$"{column}{offset}:{column}"])
                .Select(columnimport => columnimport.Select(c => c.GetValue<string>()))
                .GetEnumerator();
        }
    }
}
