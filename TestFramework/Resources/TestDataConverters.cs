using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;

namespace TestFramework.Resources
{
    public static class TestDataConverters
    {
        public static DateTime ToDateTime(this string date)
        {
            if (date.ToLower() == "idag") return new DateTime(DateTime.Today.Ticks); // CompareSerialized-friendly datetime

            var format = date.Length > 10 ? "dd/MM/yyyy HH:mm:ss.ff" : "dd/MM/yyyy";
            try
            {
                return DateTime.ParseExact(date, format, CultureInfo.InvariantCulture);
            }
            catch (Exception)
            {
                throw new FormatException($"Expected format '{format}' got '{date}'");
            }
        }

        public static bool ToBool(this string str)
        {
            var map = new Dictionary<string, bool>
            {
                { "true", true },
                { "ja", true },
                { "1", true },
                { "false", false },
                { "nei", false },
                { "0", false },
            };

            str = str.Trim().ToLower();
            if (map.ContainsKey(str))
            {
                return map[str];
            }
            throw new ArgumentException($"Expected a value of either {string.Join(", ", map.Keys)}, got {str}");
        }

        public static int ToInt(this string str)
        {
            try
            {
                return int.Parse(str);
            }
            catch (Exception)
            {
                throw new FormatException($"Expected int got '{str}'");
            }
        }

        public static Dictionary<T1, T2> ToDictionary<T1,T2>(this string jsonString)
        {
            return JsonConvert.DeserializeObject<Dictionary<T1, T2>>(jsonString) ?? new Dictionary<T1, T2>();
        }

        public static T Convert<T>(this string value, Dictionary<string, T> converter)
        {
            if (!converter.ContainsKey(value)) throw new ArgumentException($"Could not get converted value. Was '{value}' expected one of [{string.Join(", ", converter.Keys)}]");
            return converter[value];
        }
    }
}
