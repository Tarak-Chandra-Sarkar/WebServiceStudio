using System.ComponentModel;
using System.Drawing;
using System.Xml.Serialization;

namespace WebServiceStudio
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class UiProperties
    {
        private static FontConverter fontConverter = new FontConverter();
        private Font messageFont;
        private Font reqRespFont;
        private Font wsdlFont;

        public override string ToString()
        {
            return "";
        }

        [XmlIgnore, TypeConverter(typeof(FontConverter))]
        public Font MessageFont
        {
            get
            {
                if (this.messageFont == null)
                {
                    this.messageFont = new Font("Lucida Sans Unicode", 9.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
                }
                return this.messageFont;
            }
            set
            {
                this.messageFont = value;
            }
        }

        [Browsable(false), XmlElement("MessageFont")]
        public string MessageFontX
        {
            get
            {
                return (string) fontConverter.ConvertTo(null, null, this.messageFont, typeof(string));
            }
            set
            {
                this.messageFont = (Font) fontConverter.ConvertFrom(null, null, value);
            }
        }

        [TypeConverter(typeof(FontConverter)), XmlIgnore]
        public Font ReqRespFont
        {
            get
            {
                if (this.reqRespFont == null)
                {
                    this.reqRespFont = new Font("Lucida Sans Unicode", 9.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
                }
                return this.reqRespFont;
            }
            set
            {
                this.reqRespFont = value;
            }
        }

        [Browsable(false), XmlElement("ReqRespFont")]
        public string ReqRespFontX
        {
            get
            {
                return (string) fontConverter.ConvertTo(null, null, this.reqRespFont, typeof(string));
            }
            set
            {
                this.reqRespFont = (Font) fontConverter.ConvertFrom(null, null, value);
            }
        }

        [XmlIgnore, TypeConverter(typeof(FontConverter))]
        public Font WsdlFont
        {
            get
            {
                if (this.wsdlFont == null)
                {
                    this.wsdlFont = new Font("Lucida Sans Unicode", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
                }
                return this.wsdlFont;
            }
            set
            {
                this.wsdlFont = value;
            }
        }

        [XmlElement("WsdlFont"), Browsable(false)]
        public string WsdlFontX
        {
            get
            {
                return (string) fontConverter.ConvertTo(null, null, this.wsdlFont, typeof(string));
            }
            set
            {
                this.wsdlFont = (Font) fontConverter.ConvertFrom(null, null, value);
            }
        }
    }
}

