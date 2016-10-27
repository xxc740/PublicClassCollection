using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;
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

        /// <summary>
        /// 根据传入的数据，得到相应页面数据  
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
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
        ///  获取数据并解析的方法 
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
            //是否返回Byte类型数据  
            if (item.ResultType == ResultType.Byte)
                result.ResultBytes = responseByte;

            //从这里开始我们要无视编码了  
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

            //GZIIP处理  
            if (_response.ContentEncoding != null &&
                _response.ContentEncoding.Equals("gzip", StringComparison.InvariantCultureIgnoreCase))
            {
                //开始读取流并设置编码方式  
                _stream = GetMemoryStream(new GZipStream(_response.GetResponseStream(), CompressionMode.Decompress));
            }
            else
            {
                //开始读取流并设置编码方式  
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

        /// <summary>
        ///  为请求准备参数  
        /// </summary>
        /// <param name="item"></param>
        private void SetRequest(HttpItem item)
        {
            // 验证证书  
            SetCer(item);
            //设置Header参数  
            if (item.Header != null && item.Header.Count > 0)
            {
                foreach (string key in item.Header.AllKeys)
                {
                    _request.Headers.Add(key, item.Header[key]);
                }
            }

            //设置代理
            SetProxy(item);
            if (item.ProtocolVersion != null)
                _request.ProtocolVersion = item.ProtocolVersion;
            _request.ServicePoint.Expect100Continue = item.Expect100Continue;

            //请求方式Get或者Post  
            _request.Method = item.Method;
            _request.Timeout = item.TimeOut;
            _request.KeepAlive = item.KeepAlive;
            _request.ReadWriteTimeout = item.ReadWriteTimeOut;
            if (item.IfModifiedSince != null)
                _request.IfModifiedSince = Convert.ToDateTime(item.IfModifiedSince);
            _request.Accept = item.Accept;
            //ContentType返回类型  
            _request.ContentType = item.ContentType;
            //UserAgent客户端的访问类型，包括浏览器版本和操作系统信息  
            _request.UserAgent = item.UserAgent;
            //编码  
            _enCoding = item.Encoding;
            //设置安全凭证  
            _request.Credentials = item.Credentials;
            //设置Cookie  
            SetCookie(item);
            //来源地址  
            _request.Referer = item.Referer;
            //是否执行跳转功能  
            _request.AllowAutoRedirect = item.AllowAutoRedirect;
            if (item.MaximumAutomaticRedirections > 0)
            {
                _request.MaximumAutomaticRedirections = item.MaximumAutomaticRedirections;
            }

            //设置Post数据
            SetPostData(item);
            //设置最大连接  
            if (item.ConnectionLimit > 0)
                _request.ServicePoint.ConnectionLimit = item.ConnectionLimit;
        }

        /// <summary>
        /// 设置Post数据  
        /// </summary>
        /// <param name="item"></param>
        private void SetPostData(HttpItem item)
        {
            //验证在得到结果时是否有传入数据  
            if (!_request.Method.Trim().ToLower().Contains("get"))
            {
                if (item.PostEncoding != null)
                {
                    _postEncoding = item.PostEncoding;
                }

                byte[] buffer = null;
                //写入Byte类型  
                if (item.PostDataType == PostDataType.Byte && item.PostDataByte != null && item.PostDataByte.Length > 0)
                {
                    //验证在得到结果时是否有传入数据  
                    buffer = item.PostDataByte;
                }
                //写入文件  
                else if (item.PostDataType == PostDataType.FilePath && !string.IsNullOrEmpty(item.PostData))
                {
                    StreamReader sr = new StreamReader(item.PostData,_postEncoding);
                    buffer = _postEncoding.GetBytes(sr.ReadToEnd());
                    sr.Close();
                }
                //写入字符串  
                else if (!string.IsNullOrEmpty(item.PostData))
                {
                    buffer = _postEncoding.GetBytes(item.PostData);
                }
                if (buffer != null)
                {
                    _request.ContentLength = buffer.Length;
                    _request.GetRequestStream().Write(buffer,0,buffer.Length);
                }
            }
        }

        /// <summary>
        ///  设置Cookie  
        /// </summary>
        /// <param name="item"></param>
        private void SetCookie(HttpItem item)
        {
            if (!string.IsNullOrEmpty(item.Cookie))
                _request.Headers[HttpRequestHeader.Cookie] = item.Cookie;
            //设置CookieCollection  
            if (item.ResultCookieType == ResultCookieType.CookieCollection)
            {
                _request.CookieContainer = new CookieContainer();
                if(item.CookieCollection != null && item.CookieCollection.Count > 0)
                    _request.CookieContainer.Add(item.CookieCollection);
            }
        }

        /// <summary>
        /// 设置代理  
        /// </summary>
        /// <param name="item"></param>
        private void SetProxy(HttpItem item)
        {
            bool isIEProxy = false;
            if (!string.IsNullOrEmpty(item.ProxyIp))
            {
                isIEProxy = item.ProxyIp.ToLower().Contains("ieproxy");
            }
            if (!string.IsNullOrEmpty(item.ProxyIp) && !isIEProxy)
            {
                //设置代理服务器  
                if (item.ProxyIp.Contains(":"))
                {
                    string[] plist = item.ProxyIp.Split(':');
                    WebProxy myProxy = new WebProxy(plist[0].Trim(),Convert.ToInt32(plist[1].Trim()));
                    //建议连接  
                    myProxy.Credentials = new NetworkCredential(item.ProxyUserName,item.ProxyPwd);
                    //给当前请求对象  
                    _request.Proxy = myProxy;
                }
                else
                {
                    WebProxy myProxy = new WebProxy(item.ProxyIp,false);
                    //建议连接  
                    myProxy.Credentials = new NetworkCredential(item.ProxyUserName,item.ProxyPwd);
                    //给当前请求对象  
                    _request.Proxy = myProxy;
                }
            }else if(isIEProxy)
            {
                //设置为IE代理  
            }
            else
            {
                _request.Proxy = item.WebProxy;
            }
        }

        //设置证书  
        private void SetCer(HttpItem item)
        {
            if (!string.IsNullOrEmpty(item.CerPath))
            {
                //这一句一定要写在创建连接的前面。
                //使用回调的方法进行证书验证。  
                ServicePointManager.ServerCertificateValidationCallback = new System.Net.Security.RemoteCertificateValidationCallback(CheckValidationResult);
                //初始化对像，并设置请求的URL地址  
                _request = (HttpWebRequest) WebRequest.Create(item.URL);
                SetCerList(item);
                //将证书添加到请求里  
                _request.ClientCertificates.Add(new X509Certificate(item.CerPath));
            }
            else
            {
                //初始化对像，并设置请求的URL地址  
                _request = (HttpWebRequest) WebRequest.Create(item.URL);
                SetCerList(item);
            }
        }

        /// <summary>
        ///  设置多个证书  
        /// </summary>
        /// <param name="item"></param>
        private void SetCerList(HttpItem item)
        {
            if (item.ClentCertificates != null && item.ClentCertificates.Count > 0)
            {
                foreach (X509Certificate c in item.ClentCertificates)
                {
                    _request.ClientCertificates.Add(c);
                }
            }
        }

        /// <summary>
        /// 回调验证证书问题  
        /// </summary>
        /// <param name="sender">流对象</param>
        /// <param name="certificate">证书</param>
        /// <param name="chain">X509Chain</param>
        /// <param name="errors">SslPolicyErrors</param>
        /// <returns></returns>
        private bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain,
            SslPolicyErrors errors)
        {
            return true;
        }
    }
}
