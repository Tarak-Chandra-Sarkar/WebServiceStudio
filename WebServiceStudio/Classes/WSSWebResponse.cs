using System;
using System.IO;
using System.Net;
using System.Text;

namespace WebServiceStudio
{
    public class WSSWebResponse : WebResponse
    {
        private MemoryStream stream;
        private WebResponse webResponse;

        public WSSWebResponse(WebResponse webResponse)
        {
            this.webResponse = webResponse;
            this.stream = new NoCloseMemoryStream();
            Stream responseStream = webResponse.GetResponseStream();
            byte[] buffer = new byte[0x400];
            while (true)
            {
                int count = responseStream.Read(buffer, 0, buffer.Length);
                if (count <= 0)
                {
                    break;
                }
                this.stream.Write(buffer, 0, count);
            }
            this.stream.Position = 0L;
        }

        public override void Close()
        {
            this.webResponse.Close();
        }

        public string DumpResponse()
        {
            long position = this.stream.Position;
            this.stream.Position = 0L;
            string str = DumpResponse(this);
            this.stream.Position = position;
            return str;
        }

        public static string DumpResponse(WebResponse response)
        {
            Stream responseStream = response.GetResponseStream();
            StringBuilder builder = new StringBuilder();
            if (response is HttpWebResponse)
            {
                HttpWebResponse response2 = (HttpWebResponse) response;
                builder.Append(string.Concat(new object[] { "ResponseCode: ", (int) response2.StatusCode, " (", response2.StatusDescription, ")\n" }));
            }
            else if (response is WSSWebResponse)
            {
                WSSWebResponse response3 = (WSSWebResponse) response;
                builder.Append(string.Concat(new object[] { "ResponseCode: ", (int) response3.StatusCode, " (", response3.StatusDescription, ")\n" }));
            }
            foreach (string str in response.Headers.Keys)
            {
                builder.Append(str + ":" + response.Headers[str].ToString() + "\n");
            }
            builder.Append("\n");
            builder.Append(MessageTracer.ReadMessage(responseStream, (int) response.ContentLength, response.ContentType));
            return builder.ToString();
        }

        public override Stream GetResponseStream()
        {
            return this.stream;
        }

        public override long ContentLength
        {
            get
            {
                return this.webResponse.ContentLength;
            }
            set
            {
                this.webResponse.ContentLength = value;
            }
        }

        public override string ContentType
        {
            get
            {
                return this.webResponse.ContentType;
            }
            set
            {
                this.webResponse.ContentType = value;
            }
        }

        public override WebHeaderCollection Headers
        {
            get
            {
                return this.webResponse.Headers;
            }
        }

        public override Uri ResponseUri
        {
            get
            {
                return this.webResponse.ResponseUri;
            }
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                if (this.webResponse is HttpWebResponse)
                {
                    return ((HttpWebResponse) this.webResponse).StatusCode;
                }
                return HttpStatusCode.NotImplemented;
            }
        }

        public string StatusDescription
        {
            get
            {
                if (this.webResponse is HttpWebResponse)
                {
                    return ((HttpWebResponse) this.webResponse).StatusDescription;
                }
                return "";
            }
        }
    }
}

