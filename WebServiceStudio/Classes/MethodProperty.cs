using System.Collections;
using System.Reflection;
using System.Web.Services.Protocols;
using System.Windows.Forms;

namespace WebServiceStudio
{
    internal class MethodProperty : TreeNodeProperty
    {
        private bool isIn;
        private MethodInfo method;
        private object[] paramValues;
        private ProxyProperty proxyProperty;
        private object result;

        public MethodProperty(ProxyProperty proxyProperty, MethodInfo method) : base(new System.Type[] { method.ReturnType }, method.Name)
        {
            this.proxyProperty = proxyProperty;
            this.method = method;
            this.isIn = true;
        }

        public MethodProperty(ProxyProperty proxyProperty, MethodInfo method, object result, object[] paramValues) : base(new System.Type[] { method.ReturnType }, method.Name)
        {
            this.proxyProperty = proxyProperty;
            this.method = method;
            this.isIn = false;
            this.result = result;
            this.paramValues = paramValues;
        }

        private void AddBody()
        {
            TreeNode parentNode = base.TreeNode.Nodes.Add("Body");
            if (!this.isIn && (this.method.ReturnType != typeof(void)))
            {
                System.Type type = (this.result != null) ? this.result.GetType() : this.method.ReturnType;
                TreeNodeProperty.CreateTreeNodeProperty(new System.Type[] { type }, "result", this.result).RecreateSubtree(parentNode);
            }
            ParameterInfo[] parameters = this.method.GetParameters();
            for (int i = 0; i < parameters.Length; i++)
            {
                if ((!this.isIn && (parameters[i].IsOut || parameters[i].ParameterType.IsByRef)) || (this.isIn && !parameters[i].IsOut))
                {
                    System.Type parameterType = parameters[i].ParameterType;
                    if (parameterType.IsByRef)
                    {
                        parameterType = parameterType.GetElementType();
                    }
                    object val = (this.paramValues != null) ? this.paramValues[i] : (this.isIn ? TreeNodeProperty.CreateNewInstance(parameterType) : null);
                    TreeNodeProperty.CreateTreeNodeProperty(base.GetIncludedTypes(parameterType), parameters[i].Name, val).RecreateSubtree(parentNode);
                }
            }
            parentNode.ExpandAll();
        }

        private void AddHeaders()
        {
            TreeNode parentNode = base.TreeNode.Nodes.Add("Headers");
            FieldInfo[] soapHeaders = GetSoapHeaders(this.method, this.isIn);
            HttpWebClientProtocol proxy = this.proxyProperty.GetProxy();
            foreach (FieldInfo info in soapHeaders)
            {
                object val = (proxy != null) ? info.GetValue(proxy) : null;
                TreeNodeProperty.CreateTreeNodeProperty(base.GetIncludedTypes(info.FieldType), info.Name, val).RecreateSubtree(parentNode);
            }
            parentNode.ExpandAll();
        }

        protected override void CreateChildren()
        {
            this.AddHeaders();
            this.AddBody();
        }

        protected override MethodInfo GetCurrentMethod()
        {
            return this.method;
        }

        protected override object GetCurrentProxy()
        {
            return this.proxyProperty.GetProxy();
        }

        public MethodInfo GetMethod()
        {
            return this.method;
        }

        public ProxyProperty GetProxyProperty()
        {
            return this.proxyProperty;
        }

        public static FieldInfo[] GetSoapHeaders(MethodInfo method, bool isIn)
        {
            System.Type declaringType = method.DeclaringType;
            SoapHeaderAttribute[] customAttributes = (SoapHeaderAttribute[]) method.GetCustomAttributes(typeof(SoapHeaderAttribute), true);
            ArrayList list = new ArrayList();
            for (int i = 0; i < customAttributes.Length; i++)
            {
                SoapHeaderAttribute attribute = customAttributes[i];
                if (((attribute.Direction == SoapHeaderDirection.InOut) || (isIn && (attribute.Direction == SoapHeaderDirection.In))) || (!isIn && (attribute.Direction == SoapHeaderDirection.Out)))
                {
                    FieldInfo field = declaringType.GetField(attribute.MemberName);
                    list.Add(field);
                }
            }
            return (FieldInfo[]) list.ToArray(typeof(FieldInfo));
        }

        protected override bool IsInput()
        {
            return this.isIn;
        }

        private void ReadBody()
        {
            TreeNode node = base.TreeNode.Nodes[1];
            ParameterInfo[] parameters = this.method.GetParameters();
            this.paramValues = new object[parameters.Length];
            int index = 0;
            int num2 = 0;
            while (index < this.paramValues.Length)
            {
                ParameterInfo info = parameters[index];
                if (!info.IsOut)
                {
                    TreeNode node2 = node.Nodes[num2++];
                    TreeNodeProperty tag = node2.Tag as TreeNodeProperty;
                    if (tag != null)
                    {
                        this.paramValues[index] = tag.ReadChildren();
                    }
                }
                index++;
            }
        }

        public override object ReadChildren()
        {
            this.ReadHeaders();
            this.ReadBody();
            return this.paramValues;
        }

        private void ReadHeaders()
        {
            TreeNode node = base.TreeNode.Nodes[0];
            System.Type declaringType = this.method.DeclaringType;
            HttpWebClientProtocol proxy = this.proxyProperty.GetProxy();
            foreach (TreeNode node2 in node.Nodes)
            {
                ClassProperty tag = node2.Tag as ClassProperty;
                if (tag != null)
                {
                    declaringType.GetField(tag.Name).SetValue(proxy, tag.ReadChildren());
                }
            }
        }

        public override string ToString()
        {
            return base.Name;
        }
    }
}

