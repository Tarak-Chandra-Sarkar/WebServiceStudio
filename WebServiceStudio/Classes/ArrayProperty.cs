using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace WebServiceStudio
{
    internal class ArrayProperty : ClassProperty
    {
        public ArrayProperty(System.Type[] possibleTypes, string name, Array val) : base(possibleTypes, name, val)
        {
        }

        protected override void CreateChildren()
        {
            base.TreeNode.Nodes.Clear();
            if (this.OkayToCreateChildren())
            {
                System.Type elementType = this.Type.GetElementType();
                int length = this.Length;
                for (int i = 0; i < length; i++)
                {
                    object val = this.ArrayValue.GetValue(i);
                    if ((val == null) && this.IsInput())
                    {
                        val = TreeNodeProperty.CreateNewInstance(elementType);
                    }
                    TreeNodeProperty.CreateTreeNodeProperty(base.GetIncludedTypes(elementType), base.Name + "_" + i.ToString(), val).RecreateSubtree(base.TreeNode);
                }
            }
        }

        public override object ReadChildren()
        {
            Array arrayValue = this.ArrayValue;
            if (arrayValue == null)
            {
                return null;
            }
            int num = 0;
            for (int i = 0; i < arrayValue.Length; i++)
            {
                TreeNode node = base.TreeNode.Nodes[num++];
                TreeNodeProperty tag = node.Tag as TreeNodeProperty;
                if (tag != null)
                {
                    arrayValue.SetValue(tag.ReadChildren(), i);
                }
            }
            return arrayValue;
        }

        private Array ArrayValue
        {
            get
            {
                return (base.InternalValue as Array);
            }
            set
            {
                base.InternalValue = value;
            }
        }

        [RefreshProperties(RefreshProperties.All)]
        public virtual int Length
        {
            get
            {
                return ((this.ArrayValue != null) ? this.ArrayValue.Length : 0);
            }
            set
            {
                int length = this.Length;
                int num2 = value;
                Array destinationArray = Array.CreateInstance(this.Type.GetElementType(), num2);
                if (this.ArrayValue != null)
                {
                    Array.Copy(this.ArrayValue, destinationArray, Math.Min(num2, length));
                }
                this.ArrayValue = destinationArray;
                base.TreeNode.Text = this.ToString();
                this.CreateChildren();
            }
        }
    }
}

