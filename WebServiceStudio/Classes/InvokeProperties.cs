using System.Collections;
using System.ComponentModel;
using System.Xml.Serialization;

namespace WebServiceStudio
{
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class InvokeProperties
    {
        private ArrayList uris = new ArrayList();

        public void AddUri(string uri)
        {
            this.uris.Remove(uri);
            this.uris.Insert(0, uri);
            Configuration.SaveMasterConfig();
        }

        public override string ToString()
        {
            return "";
        }

        [XmlArrayItem("Uri", typeof(string)), Browsable(false)]
        public string[] RecentlyUsedUris
        {
            get
            {
                return (this.uris.ToArray(typeof(string)) as string[]);
            }
            set
            {
                this.uris.Clear();
                if (value != null)
                {
                    this.uris.AddRange(value);
                }
            }
        }
    }
}

