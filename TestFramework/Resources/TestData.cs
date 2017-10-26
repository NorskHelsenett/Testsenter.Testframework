using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;

namespace TestFramework.Resources
{
    public class TestData
    {
        public Dictionary<string, string> Data { get; set; }
        public Dictionary<string, byte[]> Attachments { get; set; }

        public TestData()
        {
            Data = new Dictionary<string, string>();
        }

        public TestData(DataColumnCollection dataColumns, object[] p)
        {
            Data = new Dictionary<string, string>();
            Attachments = new Dictionary<string, byte[]>();

            for (int i = 0; i < p.Length; i++)
            {
                try
                {
                    Data.Add(dataColumns[i].ColumnName, p[i] as string ?? "");
                }
                catch (Exception e)
                {
                    throw new ArgumentException("Error inn reading in parameters from TFS for column: " + dataColumns[i].ColumnName, e);
                }
            }
        }

        public void Insert(string key, string value)
        {
            if (Data.ContainsKey(key))
                Data[key] = value;
            else
                Data.Add(key, value);
        }

        public string GetValue(string key)
        {
            if (!Data.ContainsKey(key))
                return "";

            string result;
            Data.TryGetValue(key, out result);
            
            return result;
        }

        public async Task<JToken> GetValueAsJsonAsync(string key)
        {
            JToken jToken = null;
            await Task.Run(() =>
            {
                var value = GetValue(key);
                try
                {
                    jToken = JToken.Parse(value);
                }
                catch (Exception)
                {
                    throw;
                }
            });
            return jToken;
        }

        public async Task<JToken> ParseValueToJsonAsync(string value)
        {
            JToken jToken = null;
            await Task.Run(() =>
            {
                try
                {
                    jToken = JToken.Parse(value);
                }
                catch (Exception)
                {
                    throw;
                }
            });
            return jToken;
        }


        public string GetNullableValue(string key)
        {
            var keyValue = GetValue(key);
            return keyValue == "null" ? null : keyValue;
        }
        
        public bool GetValueBool(string key)
        {
            return GetValue(key).ToBool();
        }

        public DateTime GetValueDate(string key)
        {
            return GetValue(key).ToDateTime();
        }

        public Guid GetValueGuid(string key)
        {
            var value = GetValue(key);
            try
            {
                return new Guid(GetValue(key));
            }
            catch (Exception)
            {
                throw new FormatException($"Expected guid got '{value}'");
            }
        }

        public Guid? GetValueNullableGuid(string key)
        {
            return GetValue(key) == "null" ? (Guid?)null : GetValueGuid(key);
        }

        public Guid? GetValueOrDefaultGuid(string key, Guid? defaultGuid = null)
        {
            return IsEmpty(key) ? defaultGuid : GetValueGuid(key);
        }


        public decimal GetValueDecimal(string key)
        {
            return decimal.Parse(GetValue(key), CultureInfo.InvariantCulture);
        }

        public int GetValueInt(string key)
        {
            return GetValue(key).ToInt();
        }

        public Dictionary<string, string> GetValueDictionary(string key)
        {
            return GetValueDictionary<string, string>(key);
        }

        public Dictionary<T1, T2> GetValueDictionary<T1,T2>(string key)
        {
            return GetValue(key).ToDictionary<T1,T2>();
        }

        public string GetValueOrDefault(string key, string defaultValue="")
        {
            return IsEmpty(key) ? defaultValue : GetValue(key);
        }

        public int GetValueOrDefaultInt(string key, int defaultValue)
        {
            return IsEmpty(key) ? defaultValue : GetValueInt(key);
        }

        public int? GetValueNullableInt(string key)
        {
            return GetValue(key) == "null" ? (int?)null : GetValueInt(key);
        }

        public int? GetValueNullableIntOrDefault(string key, int? defaultValue = null)
        {
            return IsEmpty(key) ? defaultValue : GetValueNullableInt(key);
        }
        public DateTime? GetValueOrDefaultDate(string key, DateTime? defaultValue = null, int? index = null)
        {
            return IsEmpty(key) ? defaultValue : GetValueDate(key);
        }

        public DateTime GetValueOrDefaultDate(string key, DateTime defaultValue)
        {
            return IsEmpty(key) ? defaultValue : GetValueDate(key);
        }

        public bool GetValueOrDefaultBool(string key, bool defaultValue)
        {
            return IsEmpty(key) ? defaultValue : GetValueBool(key);
        }

        public string[] GetValues(string key)
        {
            return GetValuesOrDefault(key, new string[] { });
        }

        public string[] GetValuesOrDefaultNullable(string key, params string[] defaults)
        {
            return GetValue(key) == "null" ? null : GetValuesOrDefault(key, defaults);
        }

        public string[] GetValuesOrDefault(string key, params string[] defaults)
        {
            if (IsEmpty(key))
            {
                return defaults;
            }
            return GetValue(key)
                .Split(',')
                .Select(word => word.Trim())
                .ToArray();
        }

        public int[] GetValuesInt(string key)
        {
            return GetValues(key)
                .Select(int.Parse)
                .ToArray();
        }

        public bool IsEmpty(string key)
        {
            return string.IsNullOrEmpty(GetValue(key));
        }
        
        public bool Contains(params string[] keys)
        {
            return !keys.Any(IsEmpty);
        }
    }
}
