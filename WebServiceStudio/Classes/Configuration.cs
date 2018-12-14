using System.ComponentModel;
using System.IO;
using System.Reflection;
using System.Xml.Serialization;

namespace WebServiceStudio
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Configuration
    {
        private CustomHandler[] dataEditors;
        private InvokeProperties invokeSettings = new InvokeProperties();
        private static Configuration masterConfig = null;
        private CustomHandler[] proxyProperties;
        private CustomHandler[] typeConverters;
        private UiProperties uiSettings;
        private WsdlProperties wsdlSettings;

        public Configuration Copy()
        {
            MemoryStream stream = new MemoryStream();
            XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
            serializer.Serialize((Stream) stream, masterConfig);
            stream.Position = 0L;
            return (serializer.Deserialize(stream) as Configuration);
        }

        private static string GetConfigFileName()
        {
            return (Assembly.GetExecutingAssembly().Location + ".options");
        }

        internal string[] GetProxyBaseTypes()
        {
            CustomHandler[] proxyProperties = this.ProxyProperties;
            string[] strArray = new string[(proxyProperties != null) ? (proxyProperties.Length + 1) : 1];
            strArray[0] = "";
            for (int i = 1; i < strArray.Length; i++)
            {
                strArray[i] = proxyProperties[i - 1].TypeName;
            }
            return strArray;
        }

        public static void LoadMasterConfig()
        {
            try
            {
                FileStream stream = File.OpenRead(GetConfigFileName());
                StreamReader textReader = new StreamReader(stream);
                XmlSerializer serializer = new XmlSerializer(typeof(Configuration));
                masterConfig = serializer.Deserialize(textReader) as Configuration;
                stream.Flush();
                stream.Close();
            }
            catch
            {
            }
            if (masterConfig == null)
            {
                masterConfig = new Configuration();
            }
            if (masterConfig.DataEditors == null)
            {
                masterConfig.DataEditors = new CustomHandler[0];
            }
            if (masterConfig.TypeConverters == null)
            {
                masterConfig.TypeConverters = new CustomHandler[0];
            }
            if (masterConfig.WsdlSettings == null)
            {
                masterConfig.WsdlSettings = new WsdlProperties();
            }
            if (masterConfig.UiSettings == null)
            {
                masterConfig.UiSettings = new UiProperties();
            }
        }

        public static void SaveMasterConfig()
        {
            FileStream stream = File.OpenWrite(GetConfigFileName());
            StreamWriter writer = new StreamWriter(stream);
            new XmlSerializer(typeof(Configuration)).Serialize((TextWriter) writer, masterConfig);
            stream.SetLength(stream.Position);
            stream.Flush();
            stream.Close();
        }

        [Browsable(false)]
        public CustomHandler[] DataEditors
        {
            get
            {
                return this.dataEditors;
            }
            set
            {
                this.dataEditors = value;
            }
        }

        [Browsable(false)]
        public InvokeProperties InvokeSettings
        {
            get
            {
                return this.invokeSettings;
            }
            set
            {
                this.invokeSettings = value;
            }
        }

        internal static Configuration MasterConfig
        {
            get
            {
                if (masterConfig == null)
                {
                    LoadMasterConfig();
                }
                return masterConfig;
            }
            set
            {
                masterConfig = value;
                SaveMasterConfig();
            }
        }

        [Browsable(false)]
        public CustomHandler[] ProxyProperties
        {
            get
            {
                return this.proxyProperties;
            }
            set
            {
                this.proxyProperties = value;
            }
        }

        [Browsable(false)]
        public CustomHandler[] TypeConverters
        {
            get
            {
                return this.typeConverters;
            }
            set
            {
                this.typeConverters = value;
            }
        }

        public UiProperties UiSettings
        {
            get
            {
                return this.uiSettings;
            }
            set
            {
                this.uiSettings = value;
            }
        }

        public WsdlProperties WsdlSettings
        {
            get
            {
                return this.wsdlSettings;
            }
            set
            {
                this.wsdlSettings = value;
            }
        }
    }
}

