using System;
using System.ComponentModel;
using System.Drawing.Design;

namespace WebServiceStudio
{
    internal class PrimitiveProperty : TreeNodeProperty
    {
        private object val;

        public PrimitiveProperty(string name, object val) : base(new Type[] { val.GetType() }, name)
        {
            this.val = val;
        }

        public override object ReadChildren()
        {
            return this.Value;
        }

        public override string ToString()
        {
            return string.Concat(new object[] { this.Type.Name, " ", base.Name, " = ", this.Value });
        }

        [Editor(typeof(DynamicEditor), typeof(UITypeEditor)), TypeConverter(typeof(DynamicConverter))]
        public object Value
        {
            get
            {
                return this.val;
            }
            set
            {
                this.val = value;
                base.TreeNode.Text = this.ToString();
            }
        }
    }
}

