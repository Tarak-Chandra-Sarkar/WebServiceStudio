using System;
using System.Xml;

namespace WebServiceStudio
{
    internal class XmlAttributeProperty : ClassProperty
    {
        private static Type[] stringType = new Type[] { typeof(string) };

        public XmlAttributeProperty(Type[] possibleTypes, string name, object val) : base(possibleTypes, name, val)
        {
        }

        protected override void CreateChildren()
        {
            base.TreeNode.Nodes.Clear();
            if (base.InternalValue != null)
            {
                TreeNodeProperty.CreateTreeNodeProperty(stringType, "Name", this.xmlAttribute.Name).RecreateSubtree(base.TreeNode);
                TreeNodeProperty.CreateTreeNodeProperty(stringType, "NamespaceURI", this.xmlAttribute.NamespaceURI).RecreateSubtree(base.TreeNode);
                TreeNodeProperty.CreateTreeNodeProperty(stringType, "Value", this.xmlAttribute.Value).RecreateSubtree(base.TreeNode);
            }
        }

        public XmlDocument GetXmlDocument()
        {
            ArrayProperty parent = base.GetParent() as ArrayProperty;
            XmlElementProperty property2 = null;
            if (parent != null)
            {
                property2 = parent.GetParent() as XmlElementProperty;
            }
            if (property2 == null)
            {
                return this.xmlAttribute.OwnerDocument;
            }
            return property2.GetXmlDocument();
        }

        public override object ReadChildren()
        {
            if (base.InternalValue == null)
            {
                return null;
            }
            string qualifiedName = ((TreeNodeProperty) base.TreeNode.Nodes[0].Tag).ReadChildren().ToString();
            string namespaceURI = ((TreeNodeProperty) base.TreeNode.Nodes[1].Tag).ReadChildren().ToString();
            string str3 = ((TreeNodeProperty) base.TreeNode.Nodes[2].Tag).ReadChildren().ToString();
            this.xmlAttribute = this.GetXmlDocument().CreateAttribute(qualifiedName, namespaceURI);
            this.xmlAttribute.Value = str3;
            return this.xmlAttribute;
        }

        private XmlAttribute xmlAttribute
        {
            get
            {
                return (base.InternalValue as XmlAttribute);
            }
            set
            {
                base.InternalValue = value;
            }
        }
    }
}

