using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ToolsClass.StringHelper
{
    public class JsonHelper
    {
        /// <summary>
        /// Objects to json.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static string ObjectToJson<T>(T obj)
        {
            DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                json.WriteObject(ms, obj);
                string jsonStr = Encoding.UTF8.GetString(ms.ToArray());
                return jsonStr;
            }
        }

        /// <summary>
        /// Jsons to object.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonStr">The json string.</param>
        /// <returns></returns>
        public static T JsonToObject<T>(string jsonStr)
        {
            T obj = Activator.CreateInstance<T>();
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonStr)))
            {
                DataContractJsonSerializer temp = new DataContractJsonSerializer(typeof(T));
                return (T)temp.ReadObject(ms);
            }
        }

        /// <summary>
        /// Objects the time to json time.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="obj">The object.</param>
        /// <returns></returns>
        public static string ObjectTimeToJsonTime<T>(T obj)
        {
            DataContractJsonSerializer json = new DataContractJsonSerializer(typeof(T));
            using (MemoryStream ms = new MemoryStream())
            {
                json.WriteObject(ms, obj);
                string jsonString = Encoding.UTF8.GetString(ms.ToArray());
                string pattern = @"\\/Date\((\d+)\+\d+\)\\/)";
                MatchEvaluator match = new MatchEvaluator(ConvertJsonDateToDateString);
                Regex reg = new Regex(pattern);
                jsonString = reg.Replace(jsonString, match);
                return jsonString;
            }
        }

        /// <summary>
        /// Jsons the time to object time.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="jsonStr">The json string.</param>
        /// <returns></returns>
        public static T JsonTimeToObjectTime<T>(string jsonStr)
        {
            T obj = Activator.CreateInstance<T>();
            string pattern = @"\d{4}-\d{2}-\d{2}\s\d{2}:\d{2}:\d{2}";
            MatchEvaluator match = new MatchEvaluator(ConvertDateStringToJsonDate);
            Regex reg = new Regex(pattern);
            jsonStr = reg.Replace(jsonStr, match);
            using (MemoryStream ms = new MemoryStream(Encoding.UTF8.GetBytes(jsonStr)))
            {
                DataContractJsonSerializer temp = new DataContractJsonSerializer(typeof(T));
                return (T)temp.ReadObject(ms);
            }
        }

        /// <summary>
        /// Converts the json date to date string.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        private static string ConvertJsonDateToDateString(Match m)
        {
            string result = string.Empty;
            DateTime dt = new DateTime(1970, 1, 1);
            dt = dt.AddMilliseconds(long.Parse(m.Groups[1].Value));
            dt = dt.ToLocalTime();
            result = dt.ToString("yyyy-MM-dd HH:mm:ss");
            return result;
        }

        /// <summary>
        /// Converts the date string to json date.
        /// </summary>
        /// <param name="m">The m.</param>
        /// <returns></returns>
        private static string ConvertDateStringToJsonDate(Match m)
        {
            string result = string.Empty;
            DateTime dt = DateTime.Parse(m.Groups[0].Value);
            dt = dt.ToUniversalTime();
            TimeSpan ts = dt - DateTime.Parse("1970-01-01");
            result = string.Format("\\/Date({0}+0800)\\/", ts.TotalMilliseconds);
            return result;
        }
    }
}
