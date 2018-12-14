using System.ComponentModel;
using System.Reflection;
using System.Windows.Forms;

namespace WebServiceStudio
{
    internal class ClassProperty : TreeNodeProperty
    {
        private bool isNull;
        private object val;

        public ClassProperty(System.Type[] possibleTypes, string name, object val) : base(possibleTypes, name)
        {
            this.isNull = false;
            this.val = val;
            this.isNull = this.val == null;
        }

        protected override void CreateChildren()
        {
            base.TreeNode.Nodes.Clear();
            if (this.OkayToCreateChildren())
            {
                foreach (PropertyInfo info in this.Type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
                {
                    object val = info.GetValue(this.val, null);
                    if ((val == null) && this.IsInput())
                    {
                        val = TreeNodeProperty.CreateNewInstance(info.PropertyType);
                    }
                    TreeNodeProperty.CreateTreeNodeProperty(base.GetIncludedTypes(info.PropertyType), info.Name, val).RecreateSubtree(base.TreeNode);
                }
                foreach (FieldInfo info2 in this.Type.GetFields(BindingFlags.Public | BindingFlags.Instance))
                {
                    object obj3 = info2.GetValue(this.val);

                    if ((obj3 == null) && this.IsInput())
                    {
                        obj3 = TreeNodeProperty.CreateNewInstance(info2.FieldType);
                    }
                    TreeNodeProperty.CreateTreeNodeProperty(base.GetIncludedTypes(info2.FieldType), info2.Name, obj3).RecreateSubtree(base.TreeNode);
                }
            }
        }

        protected virtual bool OkayToCreateChildren()
        {
            if (TreeNodeProperty.IsInternalType(this.Type))
            {
                return false;
            }
            if (TreeNodeProperty.IsDeepNesting(this))
            {
                this.InternalValue = null;
            }
            if (this.InternalValue == null)
            {
                return false;
            }
            return true;
        }

        public override object ReadChildren()
        {
            object internalValue = this.InternalValue;
            if (internalValue == null)
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
                    info.SetValue(internalValue, tag.ReadChildren(), null);
                }
            }
            foreach (FieldInfo info2 in this.Type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                TreeNode node2 = base.TreeNode.Nodes[num++];
                TreeNodeProperty property2 = node2.Tag as TreeNodeProperty;
                if (property2 != null)
                {
                    info2.SetValue(internalValue, property2.ReadChildren(), BindingFlags.Public, null, null);
                }
            }
            return internalValue;
        }

        public virtual object ToObject()
        {
            return this.InternalValue;
        }

        public override string ToString()
        {
            return (base.GetTypeList()[0].Name + " " + base.Name + (this.IsNull ? " = null" : ""));
        }

        internal object InternalValue
        {
            get
            {
                return (this.isNull ? null : this.val);
            }
            set
            {
                this.val = value;
                this.isNull = value == null;
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        public bool IsNull
        {
            get
            {
                return this.isNull;
            }
            set
            {
                if (this.isNull != value)
                {
                    if (!value)
                    {
                        if (this.val == null)
                        {
                            this.val = TreeNodeProperty.CreateNewInstance(this.Type);
                        }
                        if (this.val == null)
                        {
                            MessageBox.Show("Not able to create an instance of " + this.Type.FullName);
                            value = true;
                        }
                    }
                    else
                    {
                        this.ReadChildren();
                    }
                    this.isNull = value;
                    this.CreateChildren();
                    base.TreeNode.Text = this.ToString();
                }
            }
        }

        public override System.Type Type
        {
            get
            {
                return ((this.InternalValue != null) ? this.InternalValue.GetType() : base.Type);
            }
            set
            {
                try
                {
                    if (this.Type != value)
                    {
                        this.InternalValue = TreeNodeProperty.CreateNewInstance(value);
                    }
                }
                catch
                {
                    this.InternalValue = null;
                }
            }
        }
    }
}

