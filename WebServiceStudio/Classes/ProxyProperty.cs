using System.Reflection;
using System.Web.Services.Protocols;
using System.Windows.Forms;

namespace WebServiceStudio
{
    internal class ProxyProperty : TreeNodeProperty
    {
        private HttpWebClientProtocol proxy;
        private ProxyProperties proxyProperties;

        public ProxyProperty(HttpWebClientProtocol proxy) : base(new System.Type[] { typeof(ProxyProperties) }, "Proxy")
        {
            this.proxy = proxy;
            this.proxyProperties = new ProxyProperties(proxy);
        }

        protected override void CreateChildren()
        {
            base.TreeNode.Nodes.Clear();
            foreach (PropertyInfo info in this.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                object val = info.GetValue(this.proxyProperties, null);
                TreeNodeProperty.CreateTreeNodeProperty(base.GetIncludedTypes(info.PropertyType), info.Name, val).RecreateSubtree(base.TreeNode);
            }
        }

        public HttpWebClientProtocol GetProxy()
        {
            ((ProxyProperties) this.ReadChildren()).UpdateProxy(this.proxy);
            return this.proxy;
        }

        public override object ReadChildren()
        {
            object proxyProperties = this.proxyProperties;
            if (proxyProperties == null)
            {
                return null;
            }
            int num = 0;
            foreach (PropertyInfo info in this.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                TreeNode node = base.TreeNode.Nodes[num++];
                TreeNodeProperty tag = node.Tag as TreeNodeProperty;
                if (tag != null)
                {
                    info.SetValue(proxyProperties, tag.ReadChildren(), null);
                }
            }
            return proxyProperties;
        }
    }
}

