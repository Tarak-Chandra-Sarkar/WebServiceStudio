using System.Windows.Forms;

namespace WebServiceStudio
{
    public class XmlTreeNode : TreeNode
    {
        private int endPos;
        private int startPos;

        public XmlTreeNode(string text, int startPos) : base(text)
        {
            this.startPos = startPos;
        }

        public int EndPosition
        {
            get
            {
                return this.endPos;
            }
            set
            {
                this.endPos = value;
            }
        }

        public int StartPosition
        {
            get
            {
                return this.startPos;
            }
            set
            {
                this.startPos = value;
            }
        }
    }
}

