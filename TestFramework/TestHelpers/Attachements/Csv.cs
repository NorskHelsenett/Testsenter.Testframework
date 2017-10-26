using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Practices.ObjectBuilder2;
using Microsoft.VisualBasic.FileIO;

namespace TestFramework.TestHelpers.Attachements
{
    public class Csv
    {
        Dictionary<string, int> headers; //Column ids
        Dictionary<string, int> codeValues; //Row ids
        string[][] data;

        public Csv(string[][] data)
        {
            headers = new Dictionary<string, int>();
            for (int i = 0; i < data[0].Length; i++)
            {
                headers[data[0][i].ToLower()] = i;
            }
            codeValues = new Dictionary<string, int>();
            for (int i = 0; i < data.Length; i++)
            {
                codeValues[data[i][0].ToLower()] = i;
            }
            this.data = data;
        }

        public string GetCellOrDefault(string codeValue, string header)
        {
            return codeValues.ContainsKey(codeValue.ToLower()) ?
                GetCell(codeValue, header) :
                null;
        }

        public string GetCell(string codeValue, string header)
        {
            return data[GetRow(codeValue)][GetColumn(header)];
        }

        public string[] GetCodeValues()
        {
            return codeValues.Keys.Skip(1).ToArray();
        }

        private int GetRow(string codeValue)
        {
            if (codeValues.ContainsKey(codeValue.ToLower()))
            {
                return codeValues[codeValue.ToLower()];
            }

            throw new KeyNotFoundException($"Could not find codeValue '{codeValue}'");
        }

        private int GetColumn(string header)
        {
            if (headers.ContainsKey(header.ToLower()))
            {
                return headers[header.ToLower()];
            }

            throw new KeyNotFoundException($"Could not find column: '{header}'");
        }

        public static Csv FromByteArray(byte[] data)
        {
            var lines = new List<string>();
            var encoding = Encoding.GetEncoding(1252);
            using (var reader = new StreamReader(new MemoryStream(data), encoding))
            {
                string line;
                while ((line = reader.ReadLine()) != null)
                {
                    lines.Add(line);
                }
            }

            var matrix = new string[lines.Count][];

            for (int i = 0; i < lines.Count; i++)
            {
                string line = lines[i];
                using (var sr = new StringReader(line))
                using (var parser = new TextFieldParser(sr))
                {
                    parser.HasFieldsEnclosedInQuotes = true;
                    parser.SetDelimiters(";");
                    parser.TrimWhiteSpace = true;
                    matrix[i] = parser.ReadFields();
                }                
            }
            return new Csv(matrix);
        }
    }
}
