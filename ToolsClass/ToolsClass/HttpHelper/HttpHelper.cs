using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ToolsClass.HttpHelper
{
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

        private void GetData(HttpItem item, HttpResult result)
        {
            throw new NotImplementedException();
        }

        private void SetRequest(HttpItem item)
        {
            throw new NotImplementedException();
        }
    }
}
