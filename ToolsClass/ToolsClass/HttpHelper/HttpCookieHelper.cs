﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ToolsClass.HttpHelper
{
    public class HttpCookieHelper
    {
        /// <summary>
        ///  根据字符生成Cookie列表  
        /// </summary>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static List<CookieItem> GetCookieList(string cookie)
        {
            List<CookieItem> cookielist = new List<CookieItem>();
            foreach (string item in cookie.Split(new string[] {";",","},StringSplitOptions.RemoveEmptyEntries))
            {
                if (Regex.IsMatch(item, @"([\s\S]*?)=([\s\S]*?)$"))
                {
                    Match m = Regex.Match(item, @"([\s\S]*?)=([\s\S]*?)$");
                    cookielist.Add(new CookieItem()
                    {
                        Key = m.Groups[1].Value,Value = m.Groups[2].Value
                    });
                }
            }
            return cookielist;
        }

        /// <summary>
        /// 根据Key值得到Cookie值,Key不区分大小写  
        /// </summary>
        /// <param name="key"></param>
        /// <param name="cookie"></param>
        /// <returns></returns>
        public static string GetCookieValue(string key, string cookie)
        {
            foreach (CookieItem item in GetCookieList(cookie))
            {
                if (item.Key == key)
                    return item.Value;
            }

            return "";
        }

        /// <summary>
        /// 格式化Cookie为标准格式  
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static string CookieFormat(string key, string value)
        {
            return string.Format("{0}={1};", key, value);
        }
    }
}