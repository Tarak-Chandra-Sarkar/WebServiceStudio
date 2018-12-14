using System;
using System.Collections;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml;

namespace WebServiceStudio
{
    public class XmlTreeWriter : XmlWriter
    {
        private StringCollection attrNames;
        private StringCollection attrValues;
        private TreeNode current;
        private int[] linePositions;
        private string name;
        private XmlTextReader reader;
        private System.Xml.WriteState state;

        public XmlTreeWriter()
        {
            this.Init();
        }

        private void Ascend()
        {
            this.Update();
            this.current = this.current.Parent;
        }

        public override void Close()
        {
        }

        private void Descend()
        {
            this.Update();
            TreeNode node = new XmlTreeNode("", this.linePositions[this.reader.LineNumber - 1]);
            node.Tag = this.current.Tag;
            this.current.Nodes.Add(node);
            this.current = node;
        }

        public void FillTree(string xml, TreeNode root)
        {
            this.current = root;
            this.reader = new XmlTextReader(new StringReader(xml));
            this.initPositions(xml);
            this.WriteNode(this.reader, true);
        }

        public override void Flush()
        {
        }

        private int GetPosition(int lineNum, int linePos)
        {
            return ((this.linePositions[lineNum - 1] + linePos) - 1);
        }

        private void Init()
        {
            this.current = null;
            this.state = System.Xml.WriteState.Start;
            this.attrNames = new StringCollection();
            this.attrValues = new StringCollection();
        }

        private void initPositions(string text)
        {
            ArrayList list = new ArrayList();
            char ch = ' ';
            int num = 0;
            list.Add(0);
            for (int i = 0; i < text.Length; i++)
            {
                char ch2 = text[i];
                switch (ch)
                {
                    case '\n':
                    case '\r':
                        list.Add(i - num);
                        break;
                }
                if (((ch2 == '\r') && (ch == '\n')) || ((ch2 == '\n') && (ch == '\r')))
                {
                    ch = ' ';
                    num++;
                }
                else
                {
                    ch = ch2;
                }
            }
            list.Add(text.Length);
            this.linePositions = list.ToArray(typeof(int)) as int[];
        }

        public override string LookupPrefix(string ns)
        {
            throw new NotImplementedException();
        }

        private void Update()
        {
            XmlTreeNode current = this.current as XmlTreeNode;
            if (current != null)
            {
                current.EndPosition = this.linePositions[this.reader.LineNumber];
            }
            if (this.name != null)
            {
                StringBuilder builder = new StringBuilder();
                for (int i = 0; i < this.attrNames.Count; i++)
                {
                    builder.Append(" " + this.attrNames[i] + " = " + this.attrValues[i]);
                }
                this.current.Text = this.name + builder.ToString();
                this.attrNames.Clear();
                this.attrValues.Clear();
                this.name = null;
            }
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override void WriteBinHex(byte[] buffer, int index, int count)
        {
            throw new NotImplementedException();
        }

        public override void WriteCData(string text)
        {
        }

        public override void WriteCharEntity(char ch)
        {
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            this.WriteRaw(new string(buffer, index, count));
        }

        public override void WriteComment(string text)
        {
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
        }

        public override void WriteEndAttribute()
        {
            this.state = System.Xml.WriteState.Element;
        }

        public override void WriteEndDocument()
        {
        }

        public override void WriteEndElement()
        {
            this.Ascend();
            this.state = System.Xml.WriteState.Element;
        }

        public override void WriteEntityRef(string name)
        {
        }

        public override void WriteFullEndElement()
        {
            this.Ascend();
            this.state = System.Xml.WriteState.Element;
        }

        public override void WriteName(string name)
        {
            throw new NotImplementedException();
        }

        public override void WriteNmToken(string name)
        {
            throw new NotImplementedException();
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
        }

        public override void WriteQualifiedName(string localName, string ns)
        {
            this.WriteRaw(localName);
        }

        public override void WriteRaw(string data)
        {
            if (this.state == System.Xml.WriteState.Attribute)
            {
                this.attrValues.Add(data);
            }
            else
            {
                this.Descend();
                this.current.Text = this.current.Text + data;
            }
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            this.WriteRaw(new string(buffer, index, count));
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            this.attrNames.Add(localName);
            this.state = System.Xml.WriteState.Attribute;
        }

        public override void WriteStartDocument()
        {
        }

        public override void WriteStartDocument(bool standalone)
        {
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            this.Descend();
            this.name = ((prefix != null) && (prefix.Length > 0)) ? (prefix + ":" + localName) : localName;
            this.state = System.Xml.WriteState.Element;
        }

        public override void WriteString(string text)
        {
            this.WriteRaw(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            throw new NotImplementedException();
        }

        public override void WriteWhitespace(string ws)
        {
        }

        public override System.Xml.WriteState WriteState
        {
            get
            {
                return this.state;
            }
        }

        public override string XmlLang
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public override System.Xml.XmlSpace XmlSpace
        {
            get
            {
                throw new NotImplementedException();
            }
        }
    }
}

