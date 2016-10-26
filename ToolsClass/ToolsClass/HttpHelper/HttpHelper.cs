using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ToolsClass.HttpHelper
{
    /// <summary>
    ///  Http连接操作帮助类  
    /// </summary>
    public class HttpHelper
    {
        //默认的编码  
        private Encoding _enCoding = Encoding.Default;
        //Post数据编码  
        private Encoding _postEncoding = Encoding.Default;
        //HttpWebRequest对象用来发起请求  
        private HttpWebRequest _request = null;
        //获取影响流的数据对象  
        private HttpWebResponse _response = null;

        public HttpResult GetHtml(HttpItem item)
        {
            HttpResult result = new HttpResult();
            try
            {
                SetRequest(item);
            }
            catch (Exception e)
            {
                result.Cookie = string.Empty;
                result.Header = null;
                result.Html = e.Message;
                result.StatusDescription = "Configure Error,Error Message:" + e.Message;
                return result;
            }
            try
            {
                using (_response = (HttpWebResponse) _request.GetResponse())
                {
                    GetData(item, result);
                }
            }
            catch (WebException e)
            {
                if (e.Response != null)
                {
                    //请求数据  
                    using (_response = (HttpWebResponse) e.Response)
                    {
                        GetData(item, result);
                    }
                }
                else
                {
                    result.Html = e.Message;
                }
            }
            catch (Exception e)
            {
                result.Html = e.Message;
            }

            if (item.IsToLower)
                result.Html = result.Html.ToLower();
            return result;
        }

        /// <summary>
        ///  获取数据的并解析的方法 
        /// </summary>
        /// <param name="item"></param>
        /// <param name="result"></param>
        private void GetData(HttpItem item, HttpResult result)
        {
            result.StatusCode = _response.StatusCode;
            result.StatusDescription = _response.StatusDescription;
            result.Header = _response.Headers;
            if (_response.Cookies != null)
                result.CookieCollection = _response.Cookies;
            if (_response.Headers["set-cookie"] != null)
                result.Cookie = _response.Headers["set-cookie"];

            byte[] ResponseByte = GetBype();

            if (ResponseByte != null && ResponseByte.Length > 0)
            {
                SetEncoding(item, result, ResponseByte);
                result.Html = _enCoding.GetString(ResponseByte);
            }
            else
            {
                result.Html = string.Empty;
            }
        }

        /// <summary>
        /// 设置编码  
        /// </summary>
        /// <param name="item"></param>
        /// <param name="result"></param>
        /// <param name="responseByte"></param>
        private void SetEncoding(HttpItem item, HttpResult result, byte[] responseByte)
        {
            if (item.ResultType == ResultType.Byte)
                result.ResultBytes = responseByte;

            if (_enCoding == null)
            {
                Match meta = Regex.Match(Encoding.Default.GetString(responseByte),"<meta[^<]*charset=([^<]*)[\"']",RegexOptions.IgnoreCase);
                string sr = string.Empty;
                if (meta != null && meta.Groups.Count > 0)
                {
                    sr = meta.Groups[1].Value.ToLower().Trim();
                }
                if (sr.Length > 2)
                {
                    try
                    {
                        _enCoding =
                            Encoding.GetEncoding(
                                sr.Replace("\"", string.Empty)
                                    .Replace("'", "")
                                    .Replace(";", "")
                                    .Replace("iso-8859-1", "gbk")
                                    .Trim());
                    }
                    catch
                    {
                        if (string.IsNullOrEmpty(_response.CharacterSet))
                        {
                            _enCoding = Encoding.UTF8;
                        }
                        else
                        {
                            _enCoding = Encoding.GetEncoding(_response.CharacterSet);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// 提取网页Byte  
        /// </summary>
        /// <returns></returns>
        private byte[] GetBype()
        {
            byte[] ResponseByte = null;
            MemoryStream _stream = new MemoryStream();

            if (_response.ContentEncoding != null &&
                _response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
            {
                _stream = GetMemoryStream(new GZipStream(_response.GetResponseStream(), CompressionMode.Decompress));
            }
            else
            {
                _stream = GetMemoryStream(_response.GetResponseStream());
            }

            ResponseByte = _stream.ToArray();
            _stream.Close();
            return ResponseByte;
        }

        /// <summary>
        /// 4.0以下.net版本取数据使用  
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private MemoryStream GetMemoryStream(Stream stream)
        {
            MemoryStream _stream = new MemoryStream();
            int Length = 256;
            Byte[] buffer = new Byte[Length];
            int bytesRead = stream.Read(buffer, 0, Length);
            while (bytesRead > 0)
            {
                _stream.Write(buffer,0,bytesRead);
                bytesRead = stream.Read(buffer, 0, Length);
            }
            return _stream;
        }

        private void SetRequest(HttpItem item)
        {
            throw new NotImplementedException();
        }
    }
}
