using System;
using System.IO;
using System.Net;

namespace WebServiceStudio
{
    public class WSSWebRequest : WebRequest
    {
        private static RequestProperties requestProperties;
        private MemoryStream stream;
        private WebRequest webRequest;

        public WSSWebRequest(WebRequest webRequest)
        {
            this.webRequest = webRequest;
            this.stream = new NoCloseMemoryStream();
        }

        public override IAsyncResult BeginGetRequestStream(AsyncCallback callback, object asyncState)
        {
            throw new NotSupportedException();
        }

        public override IAsyncResult BeginGetResponse(AsyncCallback callback, object asyncState)
        {
            throw new NotSupportedException();
        }

        public override Stream EndGetRequestStream(IAsyncResult result)
        {
            throw new NotSupportedException();
        }

        public override WebResponse EndGetResponse(IAsyncResult result)
        {
            throw new NotSupportedException();
        }

        public override Stream GetRequestStream()
        {
            return this.stream;
        }

        public override WebResponse GetResponse()
        {
            WebResponse response4;
            requestProperties.contentType = this.webRequest.ContentType;
            requestProperties.soapAction = this.webRequest.Headers["SOAPAction"];
            requestProperties.url = this.webRequest.RequestUri.ToString();
            if (this.webRequest.Method.ToUpper() == "POST")
            {
                requestProperties.Method = RequestProperties.HttpMethod.POST;
                Stream requestStream = this.webRequest.GetRequestStream();
                requestStream.Write(this.stream.GetBuffer(), 0, (int) this.stream.Length);
                requestStream.Close();
                this.stream.Position = 0L;
                requestProperties.requestPayLoad = MessageTracer.ReadMessage(this.stream, requestProperties.contentType);
            }
            else if (this.webRequest.Method.ToUpper() == "GET")
            {
                requestProperties.Method = RequestProperties.HttpMethod.GET;
            }
            try
            {
                WSSWebResponse response2 = new WSSWebResponse(this.webRequest.GetResponse());
                requestProperties.responsePayLoad = response2.DumpResponse();
                response4 = response2;
            }
            catch (WebException exception)
            {
                if (exception.Response != null)
                {
                    WSSWebResponse response = new WSSWebResponse(exception.Response);
                    requestProperties.responsePayLoad = response.DumpResponse();
                    throw new WebException(exception.Message, exception, exception.Status, response);
                }
                requestProperties.responsePayLoad = exception.ToString();
                throw;
            }
            catch (Exception exception2)
            {
                requestProperties.responsePayLoad = exception2.ToString();
                throw;
            }
            return response4;
        }

        public override string ConnectionGroupName
        {
            get
            {
                return this.webRequest.ConnectionGroupName;
            }
            set
            {
                this.webRequest.ConnectionGroupName = value;
            }
        }

        public override long ContentLength
        {
            get
            {
                return this.webRequest.ContentLength;
            }
            set
            {
                this.webRequest.ContentLength = value;
            }
        }

        public override string ContentType
        {
            get
            {
                return this.webRequest.ContentType;
            }
            set
            {
                this.webRequest.ContentType = value;
            }
        }

        public override ICredentials Credentials
        {
            get
            {
                return this.webRequest.Credentials;
            }
            set
            {
                this.webRequest.Credentials = value;
            }
        }

        public override WebHeaderCollection Headers
        {
            get
            {
                return this.webRequest.Headers;
            }
            set
            {
                this.webRequest.Headers = value;
            }
        }

        public override string Method
        {
            get
            {
                return this.webRequest.Method;
            }
            set
            {
                this.webRequest.Method = value;
            }
        }

        public override bool PreAuthenticate
        {
            get
            {
                return this.webRequest.PreAuthenticate;
            }
            set
            {
                this.webRequest.PreAuthenticate = value;
            }
        }

        public override IWebProxy Proxy
        {
            get
            {
                return this.webRequest.Proxy;
            }
            set
            {
                this.webRequest.Proxy = value;
            }
        }

        internal static RequestProperties RequestTrace
        {
            get
            {
                return requestProperties;
            }
            set
            {
                requestProperties = value;
            }
        }

        public override Uri RequestUri
        {
            get
            {
                return this.webRequest.RequestUri;
            }
        }

        public override int Timeout
        {
            get
            {
                return this.webRequest.Timeout;
            }
            set
            {
                this.webRequest.Timeout = value;
            }
        }
    }
}

