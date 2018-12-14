using System;
using System.Collections;
using System.ComponentModel;
using System.Data;
using System.Reflection;
using System.Web.Services.Protocols;
using System.Xml;
using System.Xml.Serialization;

namespace WebServiceStudio
{
    internal class TreeNodeProperty
    {
        private static Hashtable includedTypesLookup = new Hashtable();
        public string Name;
        private static System.Type[] systemTypes = new System.Type[] { 
            typeof(bool), typeof(byte), typeof(byte[]), typeof(sbyte), typeof(short), typeof(int), typeof(long), typeof(float), typeof(double), typeof(string), typeof(string[]), typeof(DateTime), typeof(TimeSpan), typeof(XmlElement), typeof(XmlAttribute), typeof(XmlNode[]), 
            typeof(object[])
         };
        public System.Windows.Forms.TreeNode TreeNode;
        private System.Type[] types;

        public TreeNodeProperty(System.Type[] types, string name)
        {
            this.types = types;
            this.Name = name;
        }

        public void AddToTypeList(object o)
        {
            System.Type type = o as System.Type;
            System.Type[] destinationArray = new System.Type[this.types.Length + 1];
            Array.Copy(this.types, destinationArray, this.types.Length);
            destinationArray[this.types.Length] = type;
            this.types = destinationArray;
        }

        private void AddTypeToList(System.Type[] includedTypes, ArrayList list)
        {
            System.Type type = list[0] as System.Type;
            foreach (System.Type type2 in includedTypes)
            {
                if (type.IsAssignableFrom(type2) && !list.Contains(type2))
                {
                    list.Add(type2);
                }
            }
        }

        public static void ClearIncludedTypes()
        {
            includedTypesLookup.Clear();
        }

        protected virtual void CreateChildren()
        {
        }

        protected static object CreateNewInstance(System.Type type)
        {
            object obj2 = new Object ();
            try
            {
                if (type.IsArray)
                {
                    return Array.CreateInstance(type.GetElementType(), 1);
                }
                if (type == typeof(string))
                {
                    return "";
                }
                if (type == typeof(Guid))
                {
                    return Guid.NewGuid();
                }
                if (type == typeof(XmlElement))
                {
                    XmlDocument document = new XmlDocument();
                    return document.CreateElement("MyElement");
                }
                if (type == typeof(XmlAttribute))
                {
                    XmlDocument document2 = new XmlDocument();
                    return document2.CreateAttribute("MyAttribute");
                }
                obj2 = Activator.CreateInstance(type);
            }
            catch
            {
            }
            return obj2;
        }

        public static TreeNodeProperty CreateTreeNodeProperty(TreeNodeProperty tnp)
        {
            if (tnp is ClassProperty)
            {
                ClassProperty property = tnp as ClassProperty;
                return CreateTreeNodeProperty(property.types, property.Name, property.InternalValue);
            }
            if (tnp is PrimitiveProperty)
            {
                PrimitiveProperty property2 = tnp as PrimitiveProperty;
                return CreateTreeNodeProperty(property2.types, property2.Name, property2.Value);
            }
            return CreateTreeNodeProperty(tnp.types, tnp.Name, null);
        }

        public static TreeNodeProperty CreateTreeNodeProperty(TreeNodeProperty tnp, object val)
        {
            return CreateTreeNodeProperty(tnp.types, tnp.Name, val);
        }

        public static TreeNodeProperty CreateTreeNodeProperty(System.Type[] possibleTypes, string name, object val)
        {
            System.Type elementType = (val == null) ? possibleTypes[0] : val.GetType();
            if (elementType.IsByRef)
            {
                elementType = elementType.GetElementType();
            }
            if (IsPrimitiveType(possibleTypes[0]))
            {
                if (val == null)
                {
                    val = CreateNewInstance(elementType);
                }
                return new PrimitiveProperty(name, val);
            }
            if (IsNullablePrimitiveType(elementType) || IsPrimitiveType(elementType))
            {
                return new NullablePrimitiveProperty(possibleTypes, name, val);
            }
            if (typeof(XmlElement).IsAssignableFrom(elementType))
            {
                return new XmlElementProperty(possibleTypes, name, val);
            }
            if (typeof(XmlAttribute).IsAssignableFrom(elementType))
            {
                return new XmlAttributeProperty(possibleTypes, name, val);
            }
            if (elementType.IsArray)
            {
                return new ArrayProperty(possibleTypes, name, val as Array);
            }
					  if (elementType.Name.IndexOf("Nullable") >=0)
						{
							return new NullableGenericProperty(possibleTypes, name, val);
						}
            return new ClassProperty(possibleTypes, name, val);
        }

        private static System.Type[] GetAllIncludedTypes(System.Type webService)
        {
            System.Type[] typeArray = includedTypesLookup[webService] as System.Type[];
            if (typeArray == null)
            {
                ArrayList list = new ArrayList();
                SoapIncludeAttribute[] customAttributes = webService.GetCustomAttributes(typeof(SoapIncludeAttribute), true) as SoapIncludeAttribute[];
                foreach (SoapIncludeAttribute attribute in customAttributes)
                {
                    list.Add(attribute.Type);
                }
                XmlIncludeAttribute[] attributeArray2 = webService.GetCustomAttributes(typeof(XmlIncludeAttribute), true) as XmlIncludeAttribute[];
                foreach (XmlIncludeAttribute attribute2 in attributeArray2)
                {
                    list.Add(attribute2.Type);
                }
                foreach (System.Type type in systemTypes)
                {
                    list.Add(type);
                }
                typeArray = (System.Type[]) list.ToArray(typeof(System.Type));
                includedTypesLookup[webService] = typeArray;
            }
            return typeArray;
        }

        protected virtual MethodInfo GetCurrentMethod()
        {
            TreeNodeProperty parent = this.GetParent();
            if (parent == null)
            {
                return null;
            }
            return parent.GetCurrentMethod();
        }

        protected virtual object GetCurrentProxy()
        {
            TreeNodeProperty parent = this.GetParent();
            if (parent == null)
            {
                return null;
            }
            return parent.GetCurrentProxy();
        }

        protected System.Type[] GetIncludedTypes(System.Type type)
        {
            ArrayList list = new ArrayList();
            list.Add(type);
            if (type.IsByRef)
            {
                type = type.GetElementType();
            }
            MethodInfo currentMethod = this.GetCurrentMethod();
            if (currentMethod != null)
            {
                this.AddTypeToList(GetAllIncludedTypes(currentMethod.DeclaringType), list);
            }
            this.AddTypeToList(GetAllIncludedTypes(type), list);
            return (System.Type[]) list.ToArray(typeof(System.Type));
        }

        public TreeNodeProperty GetParent()
        {
            if (this.TreeNode != null)
            {
                System.Windows.Forms.TreeNode treeNode = this.TreeNode;
                while (treeNode.Parent != null)
                {
                    treeNode = treeNode.Parent;
                    TreeNodeProperty tag = treeNode.Tag as TreeNodeProperty;
                    if (tag != null)
                    {
                        return tag;
                    }
                }
            }
            return null;
        }

        public System.Type[] GetTypeList()
        {
            return this.types;
        }

        protected static bool IsDeepNesting(TreeNodeProperty tnp)
        {
            if (tnp != null)
            {
                int num = 0;
                while ((tnp = tnp.GetParent()) != null)
                {
                    num++;
                    if (num > 12)
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        protected virtual bool IsInput()
        {
            TreeNodeProperty parent = this.GetParent();
            if (parent == null)
            {
                return false;
            }
            return parent.IsInput();
        }

        protected static bool IsInternalType(System.Type type)
        {
            return typeof(System.Type).IsAssignableFrom(type);
        }

        private static bool IsNullablePrimitiveType(System.Type type)
        {
            return ((((typeof(string) == type) || (typeof(Guid) == type)) || typeof(DataSet).IsAssignableFrom(type)) || (DynamicEditor.IsEditorDefined(type) || DynamicConverter.IsConverterDefined(type)));
        }

        protected static bool IsPrimitiveType(System.Type type)
        {
            return (((type.IsEnum || type.IsPrimitive) || ((type == typeof(DateTime)) || (type == typeof(TimeSpan)))) || (type == typeof(decimal)));
        }

        public static bool IsWebMethod(MethodInfo method)
        {
            object[] customAttributes = method.GetCustomAttributes(typeof(SoapRpcMethodAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                return true;
            }
            customAttributes = method.GetCustomAttributes(typeof(SoapDocumentMethodAttribute), true);
            if ((customAttributes != null) && (customAttributes.Length > 0))
            {
                return true;
            }
            customAttributes = method.GetCustomAttributes(typeof(HttpMethodAttribute), true);
            return ((customAttributes != null) && (customAttributes.Length > 0));
        }

        public static bool IsWebService(System.Type type)
        {
            return typeof(HttpWebClientProtocol).IsAssignableFrom(type);
        }

        public virtual object ReadChildren()
        {
            return null;
        }

        public void RecreateSubtree(System.Windows.Forms.TreeNode parentNode)
        {
            int index = -1;
            if (this.TreeNode != null)
            {
                if (parentNode == null)
                {
                    parentNode = this.TreeNode.Parent;
                }
                if (this.TreeNode.Parent == parentNode)
                {
                    index = this.TreeNode.Index;
                }
                this.TreeNode.Remove();
            }
            this.TreeNode = new System.Windows.Forms.TreeNode(this.ToString());
            this.TreeNode.Tag = this;
            if (parentNode != null)
            {
                if (index < 0)
                {
                    parentNode.Nodes.Add(this.TreeNode);
                }
                else
                {
                    parentNode.Nodes.Insert(index, this.TreeNode);
                }
            }
            this.CreateChildren();
        }

        public override string ToString()
        {
            return this.Name;
        }

        [TypeConverter(typeof(ListStandardValues))]
        public virtual System.Type Type
        {
            get
            {
                return this.types[0];
            }
            set
            {
            }
        }
    }
}

