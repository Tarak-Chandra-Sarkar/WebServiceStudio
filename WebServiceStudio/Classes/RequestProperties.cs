using System.Net;
using System.Web.Services.Protocols;

namespace WebServiceStudio
{
    internal class RequestProperties
    {
        public bool allowAutoRedirect = true;
        public bool allowWriteStreamBuffering = true;
        public string basicAuthPassword;
        public string basicAuthUserName;
        public string contentType;
        public bool keepAlive;
        public HttpMethod method;
        public bool pipelined;
        public bool preAuthenticate;
        public string proxy;
        public string requestPayLoad;
        public string responsePayLoad;
        public bool sendChunked;
        public string soapAction;
        public int timeout = 0x2710;
        public string url;
        public bool useCookieContainer;
        public bool useDefaultCredential;

        public RequestProperties(HttpWebClientProtocol proxy)
        {
            if (proxy != null)
            {
                this.Method = HttpMethod.POST;
                this.preAuthenticate = proxy.PreAuthenticate;
                this.timeout = proxy.Timeout;
                this.useCookieContainer = proxy.CookieContainer != null;
                SoapHttpClientProtocol protocol = proxy as SoapHttpClientProtocol;
                if (protocol != null)
                {
                    this.allowAutoRedirect = protocol.AllowAutoRedirect;
                    this.allowWriteStreamBuffering = protocol.AllowAutoRedirect;
                    WebProxy proxy2 = protocol.Proxy as WebProxy;
                    this.HttpProxy = ((proxy2 != null) && (proxy2.Address != null)) ? proxy2.Address.ToString() : null;
                }
            }
        }

        public string __RequestProperties__
        {
            get
            {
                return "";
            }
        }

        public bool AllowAutoRedirect
        {
            get
            {
                return this.allowAutoRedirect;
            }
            set
            {
                this.allowAutoRedirect = value;
            }
        }

        public bool AllowWriteStreamBuffering
        {
            get
            {
                return this.allowWriteStreamBuffering;
            }
            set
            {
                this.allowWriteStreamBuffering = value;
            }
        }

        public string BasicAuthPassword
        {
            get
            {
                return this.basicAuthPassword;
            }
            set
            {
                this.basicAuthPassword = value;
            }
        }

        public string BasicAuthUserName
        {
            get
            {
                return this.basicAuthUserName;
            }
            set
            {
                this.basicAuthUserName = value;
            }
        }

        public string ContentType
        {
            get
            {
                return this.contentType;
            }
            set
            {
                this.contentType = value;
            }
        }

        public string HttpProxy
        {
            get
            {
                return this.proxy;
            }
            set
            {
                this.proxy = ((value == null) || (value.Length == 0)) ? null : new WebProxy(value).Address.ToString();
            }
        }

        public bool KeepAlive
        {
            get
            {
                return this.keepAlive;
            }
            set
            {
                this.keepAlive = value;
            }
        }

        public HttpMethod Method
        {
            get
            {
                return this.method;
            }
            set
            {
                this.method = value;
            }
        }

        public bool Pipelined
        {
            get
            {
                return this.pipelined;
            }
            set
            {
                this.pipelined = value;
            }
        }

        public bool PreAuthenticate
        {
            get
            {
                return this.preAuthenticate;
            }
            set
            {
                this.preAuthenticate = value;
            }
        }

        public bool SendChunked
        {
            get
            {
                return this.sendChunked;
            }
            set
            {
                this.sendChunked = value;
            }
        }

        public string SOAPAction
        {
            get
            {
                return this.soapAction;
            }
            set
            {
                this.soapAction = value;
            }
        }

        public int Timeout
        {
            get
            {
                return this.timeout;
            }
            set
            {
                this.timeout = value;
            }
        }

        public string Url
        {
            get
            {
                return this.url;
            }
            set
            {
                this.url = value;
            }
        }

        public bool UseCookieContainer
        {
            get
            {
                return this.useCookieContainer;
            }
            set
            {
                this.useCookieContainer = value;
            }
        }

        public bool UseDefaultCredential
        {
            get
            {
                return this.useDefaultCredential;
            }
            set
            {
                this.useDefaultCredential = value;
            }
        }

        public enum HttpMethod
        {
            GET,
            POST
        }
    }
}

