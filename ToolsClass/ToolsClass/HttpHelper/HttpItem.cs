using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace ToolsClass.HttpHelper
{
    /// <summary>
    /// 返回类型  
    /// </summary>
    public enum ResultType
    {
        /// <summary>
        ///  表示只返回字符串 只有Html有数据  
        /// </summary>
        String,
        /// <summary>
        /// 表示返回字符串和字节流 ResultByte和Html都有数据返回  
        /// </summary>
        Byte
    }

    /// <summary>
    /// Post的数据格式默认为string  
    /// </summary>
    public enum PostDataType
    {
        /// <summary>
        /// 字符串类型，这时编码Encoding可不设置  
        /// </summary>
        String,
        /// <summary>
        /// Byte类型，需要设置PostdataByte参数的值编码Encoding可设置为空  
        /// </summary>
        Byte,
        /// <summary>
        /// 传文件，Postdata必须设置为文件的绝对路径，必须设置Encoding的值  
        /// </summary>
        FilePath
    }

    /// <summary>
    /// Cookie返回类型  
    /// </summary>
    public enum ResultCookieType
    {
        /// <summary>
        ///  只返回字符串类型的Cookie  
        /// </summary>
        String,
        /// <summary>
        ///  CookieCollection格式的Cookie集合同时也返回String类型的cookie  
        /// </summary>
        CookieCollection
    }

    /// <summary>
    /// Http请求参数类 
    /// </summary>
    public class HttpItem
    {
        /// <summary>
        /// 请求URL必须填写  
        /// </summary>
        private string _URL = string.Empty;
        /// <summary>
        /// 请求方式默认为GET方式,当为POST方式时必须设置Postdata的值  
        /// </summary>
        private string _Method = "GET";
        /// <summary>
        /// 默认请求超时时间  
        /// </summary>
        private int _TimeOut = 100000;
        /// <summary>
        ///  默认写入Post数据超时间 
        /// </summary>
        private int _ReadWriteTimeOut = 30000;
        /// <summary>
        ///  获取或设置一个值，该值指示是否与 Internet 资源建立持久性连接默认为true。  
        /// </summary>
        private Boolean _KeepAlive = true;
        /// <summary>
        ///  请求标头值 默认为text/html, application/xhtml+xml, */*  
        /// </summary>
        private string _Accept = "text/html,application/xhtml+xml,*/*";
        /// <summary>
        /// 请求返回类型默认 text/html  
        /// </summary>
        private string _ContentType = "text/html";
        /// <summary>
        /// 客户端访问信息默认Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0) 
        /// </summary>
        private string _UserAgent = "Mozilla/5.0 (compatible; MSIE 9.0; Windows NT 6.1; Trident/5.0)";
        /// <summary>
        /// 返回数据编码默认为NUll,可以自动识别,一般为utf-8,gbk,gb2312  
        /// </summary>
        private Encoding _enCoding = null;
        /// <summary>
        /// Post的数据类型  
        /// </summary>
        private PostDataType _postDataType = PostDataType.String;
        /// <summary>
        /// Post请求时要发送的字符串Post数据  
        /// </summary>
        string _postData = string.Empty;
        /// <summary>
        ///  Post请求时要发送的Byte类型的Post数据  
        /// </summary>
        private byte[] _postDataByte = null;
        /// <summary>
        /// 设置代理对象，不想使用IE默认配置就设置为Null，而且不要设置ProxyIp  
        /// </summary>
        private WebProxy _webProxy;
        /// <summary>
        ///  Cookie对象集合  
        /// </summary>
        private CookieCollection _cookieCollection = null;
        /// <summary>
        /// 请求时的Cookie  
        /// </summary>
        private string _cookie = string.Empty;
        /// <summary>
        /// 来源地址，上次访问地址  
        /// </summary>
        private string _referer = string.Empty;
        /// <summary>
        /// 证书绝对路径  
        /// </summary>
        private string _cerPath = string.Empty;
        /// <summary>
        ///  是否设置为全文小写，默认为不转化  
        /// </summary>
        private Boolean _isToLower = false;
        /// <summary>
        ///  支持跳转页面，查询结果将是跳转后的页面，默认是不跳转  
        /// </summary>
        private Boolean _allowautoredirect = false;
        /// <summary>
        /// 最大连接数  
        /// </summary>
        private int _connectionlimit = 1024;
        /// <summary>
        /// 代理Proxy 服务器用户名  
        /// </summary>
        private string _proxyUserName = string.Empty;
        /// <summary>
        /// 代理服务器密码  
        /// </summary>
        private string _proxypwd = string.Empty;
        /// <summary>
        /// 代理服务IP ,如果要使用IE代理就设置为ieproxy  
        /// </summary>
        private string _proxyIp = string.Empty;
        /// <summary>
        ///  设置返回类型String和Byte  
        /// </summary>
        private ResultType _resultType = ResultType.String;
        /// <summary>
        ///  header对象  
        /// </summary>
        private WebHeaderCollection header = new WebHeaderCollection();
        /// <summary>
        ///  获取或设置用于请求的 HTTP 版本。返回结果:用于请求的 HTTP 版本。默认为 System.Net.HttpVersion.Version11。 
        /// </summary>
        private Version _protocolVersion;
        /// <summary>
        ///  获取或设置一个 System.Boolean 值，该值确定是否使用 100-Continue 行为。如果 POST 请求需要 100-Continue 响应，则为 true；否则为 false。默认值为 true。  
        /// </summary>
        private Boolean _expect100continue = true;
        /// <summary>
        ///  设置509证书集合  
        /// </summary>
        private X509CertificateCollection _clentCertificates;
        /// <summary>
        /// 设置或获取Post参数编码,默认的为Default编码  
        /// </summary>
        private Encoding _postEncoding;
        /// <summary>
        ///  Cookie返回类型,默认的是只返回字符串类型  
        /// </summary>
        private ResultCookieType _resultCookieType = ResultCookieType.String;
        /// <summary>
        /// 获取或设置请求的身份验证信息。
        /// </summary>
        private ICredentials _iCredentials = CredentialCache.DefaultCredentials;
        /// <summary>
        /// 设置请求将跟随的重定向的最大数目  
        /// </summary>
        private int _maximumAutomaticRedirections;
        /// <summary>
        /// 获取和设置IfModifiedSince，默认为当前日期和时间  
        /// </summary>
        private DateTime? _ifModifiedSince = null;

        public string URL
        {
            get { return _URL;}
            set { _URL = value; }
        }

        public string Method
        {
            get { return _Method;}
            set { _Method = value; }
        }

        public int TimeOut
        {
            get { return _TimeOut;}
            set { _TimeOut = value; }
        }

        public int ReadWriteTimeOut
        {
            get { return _ReadWriteTimeOut;}
            set { _ReadWriteTimeOut = value; }
        }

        public Boolean KeepAlive
        {
            get { return _KeepAlive;}
            set { _KeepAlive = value; }
        }

        public string Accept
        {
            get { return _Accept;}
            set { _Accept = value; }
        }

        public string ContentType
        {
            get { return _ContentType;}
            set { _ContentType = value; }
        }

        public string UserAgent
        {
            get { return _UserAgent; }
            set { _UserAgent = value; }
        }

        public Encoding Encoding
        {
            get { return _enCoding; }
            set { _enCoding = value; }
        }

        public PostDataType PostDataType
        {
            get { return _postDataType; }
            set { _postDataType = value; }
        }

        public string PostData
        {
            get { return _postData; }
            set { _postData = value; }
        }

        public byte[] PostDataByte
        {
            get { return _postDataByte; }
            set { _postDataByte = value; }
        }

        public WebProxy WebProxy
        {
            get { return _webProxy; }
            set { _webProxy = value; }
        }

        public CookieCollection CookieCollection
        {
            get { return _cookieCollection;}
            set { _cookieCollection = value; }
        }

        public string Cookie
        {
            get { return _cookie;}
            set { _cookie = value; }
        }

        public string Referer
        {
            get { return _referer;}
            set { _referer = value; }
        }

        public string CerPath
        {
            get { return _cerPath;}
            set { _cerPath = value; }
        }

        public Boolean IsToLower
        {
            get { return _isToLower; }
            set { _isToLower = value; }
        }

        public Boolean AllowAutoRedirect
        {
            get { return _allowautoredirect;}
            set { _allowautoredirect = value; }
        }

        public int ConnectionLimit
        {
            get { return _connectionlimit;}
            set { _connectionlimit = value; }
        }

        public string ProxyUserName
        {
            get { return _proxyUserName;}
            set { _proxyUserName = value; }
        }

        public string ProxyPwd
        {
            get { return _proxypwd; }
            set { _proxypwd = value; }
        }

        public string ProxyIp
        {
            get { return _proxyIp; }
            set { _proxyIp = value; }
        }

        public ResultType ResultType
        {
            get { return _resultType; }
            set { _resultType = value; }
        }

        public WebHeaderCollection Header
        {
            get { return header; }
            set { header = value; }
        }

        public Version ProtocolVersion
        {
            get { return _protocolVersion; }
            set { _protocolVersion = value; }
        }

        public bool Expect100Continue
        {
            get { return _expect100continue; }
            set { _expect100continue = value; }
        }

        public X509CertificateCollection ClentCertificates
        {
            get { return _clentCertificates; }
            set { _clentCertificates = value; }
        }

        public Encoding PostEncoding
        {
            get { return _postEncoding; }
            set { _postEncoding = value; }
        }

        public ResultCookieType ResultCookieType
        {
            get { return _resultCookieType; }
            set { _resultCookieType = value; }
        }

        public ICredentials Credentials
        {
            get { return _iCredentials; }
            set { _iCredentials = value; }
        }

        public int MaximumAutomaticRedirections
        {
            get { return _maximumAutomaticRedirections; }
            set { _maximumAutomaticRedirections = value; }
        }

        public DateTime? IfModifiedSince
        {
            get { return _ifModifiedSince; }
            set { _ifModifiedSince = value; }
        }
    }

    /// <summary>
    /// Http返回参数类  
    /// </summary>
    public class HttpResult
    {
        /// <summary>
        /// Http请求返回的Cookie  
        /// </summary>
        private string _cookie;
        /// <summary>
        /// Cookie对象集合
        /// </summary>
        private CookieCollection _cookieCollection;
        /// <summary>
        /// 返回的String类型数据 只有ResultType.String时才返回数据，其它情况为空  
        /// </summary>
        private string _html = string.Empty;
        /// <summary>
        /// 返回的Byte数组 只有ResultType.Byte时才返回数据，其它情况为空  
        /// </summary>
        private byte[] _resultBytes;
        /// <summary>
        /// header对象  
        /// </summary>
        private WebHeaderCollection _header;
        /// <summary>
        ///  返回状态说明  
        /// </summary>
        private string _statusDescription;
        /// <summary>
        /// 返回状态码,默认为OK
        /// </summary>
        private HttpStatusCode _statusCode;

        public string Cookie
        {
            get { return _cookie; }
            set { _cookie = value; }
        }

        public CookieCollection CookieCollection
        {
            get { return _cookieCollection; }
            set { _cookieCollection = value; }
        }

        public string Html
        {
            get { return _html; }
            set { _html = value; }
        }

        public byte[] ResultBytes
        {
            get { return _resultBytes; }
            set { _resultBytes = value; }
        }

        public WebHeaderCollection Header
        {
            get { return _header; }
            set { _header = value; }
        }

        public string StatusDescription
        {
            get { return _statusDescription; }
            set { _statusDescription = value; }
        }

        public HttpStatusCode StatusCode
        {
            get { return _statusCode; }
            set { _statusCode = value; }
        }
    }
}
