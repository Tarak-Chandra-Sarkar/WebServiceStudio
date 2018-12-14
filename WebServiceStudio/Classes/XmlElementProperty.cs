using System;
using System.Collections;
using System.Xml;

namespace WebServiceStudio
{
    internal class XmlElementProperty : ClassProperty
    {
        private static Type[] attrArrayType = new Type[] { typeof(XmlAttribute[]) };
        private static Type[] elemArrayType = new Type[] { typeof(XmlElement[]) };
        private static Type[] stringType = new Type[] { typeof(string) };

        public XmlElementProperty(Type[] possibleTypes, string name, object val) : base(possibleTypes, name, val)
        {
        }

        protected override void CreateChildren()
        {
            base.TreeNode.Nodes.Clear();
            if (base.InternalValue != null)
            {
                TreeNodeProperty.CreateTreeNodeProperty(stringType, "Name", this.xmlElement.Name).RecreateSubtree(base.TreeNode);
                TreeNodeProperty.CreateTreeNodeProperty(stringType, "NamespaceURI", this.xmlElement.NamespaceURI).RecreateSubtree(base.TreeNode);
                TreeNodeProperty.CreateTreeNodeProperty(stringType, "TextValue", this.xmlElement.InnerText).RecreateSubtree(base.TreeNode);
                ArrayList list = new ArrayList();
                ArrayList list2 = new ArrayList();
                if (this.xmlElement != null)
                {
                    for (XmlNode node = this.xmlElement.FirstChild; node != null; node = node.NextSibling)
                    {
                        if (node.NodeType == XmlNodeType.Element)
                        {
                            list2.Add(node);
                        }
                    }
                    foreach (XmlAttribute attribute in this.xmlElement.Attributes)
                    {
                        if ((attribute.Name != "xmlns") && !attribute.Name.StartsWith("xmlns:"))
                        {
                            list.Add(attribute);
                        }
                    }
                }
                XmlAttribute[] val = ((list.Count == 0) && !this.IsInput()) ? null : (list.ToArray(typeof(XmlAttribute)) as XmlAttribute[]);
                XmlElement[] elementArray = ((list2.Count == 0) && !this.IsInput()) ? null : (list2.ToArray(typeof(XmlElement)) as XmlElement[]);
                TreeNodeProperty.CreateTreeNodeProperty(attrArrayType, "Attributes", val).RecreateSubtree(base.TreeNode);
                TreeNodeProperty.CreateTreeNodeProperty(elemArrayType, "SubElements", elementArray).RecreateSubtree(base.TreeNode);
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
                return this.xmlElement.OwnerDocument;
            }
            return property2.GetXmlDocument();
        }

        public override object ReadChildren()
        {
            XmlElement element3;
            if (base.InternalValue == null)
            {
                return null;
            }
            string qualifiedName = ((TreeNodeProperty) base.TreeNode.Nodes[0].Tag).ReadChildren().ToString();
            string namespaceURI = ((TreeNodeProperty) base.TreeNode.Nodes[1].Tag).ReadChildren().ToString();
            string str3 = ((TreeNodeProperty) base.TreeNode.Nodes[2].Tag).ReadChildren().ToString();
            XmlAttribute[] attributeArray = (XmlAttribute[]) ((TreeNodeProperty) base.TreeNode.Nodes[3].Tag).ReadChildren();
            XmlElement[] elementArray = (XmlElement[]) ((TreeNodeProperty) base.TreeNode.Nodes[4].Tag).ReadChildren();
            XmlElement element = this.GetXmlDocument().CreateElement(qualifiedName, namespaceURI);
            if (attributeArray != null)
            {
                foreach (XmlAttribute attribute in attributeArray)
                {
                    element.SetAttributeNode(attribute);
                }
            }
            element.InnerText = str3;
            if (elementArray != null)
            {
                foreach (XmlElement element2 in elementArray)
                {
                    element.AppendChild(element2);
                }
            }
            this.xmlElement = element3 = element;
            return element3;
        }

        private XmlElement xmlElement
        {
            get
            {
                return (base.InternalValue as XmlElement);
            }
            set
            {
                base.InternalValue = value;
            }
        }
    }
}

