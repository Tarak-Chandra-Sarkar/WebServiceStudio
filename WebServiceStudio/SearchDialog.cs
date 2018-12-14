using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace WebServiceStudio
{
    public class SearchDialog : Form
    {
        private Button buttonCancel;
        private Button buttonOk;
        private CheckBox checkMatchCase;
        private CheckBox checkWholeWord;
        private Container components = null;
        private Label label1;
        public bool MatchCase;
        public string SearchStr;
        private TextBox textSearch;
        public bool WholeWord;

        public SearchDialog()
        {
            this.InitializeComponent();
        }

        private void buttonCancel_Click(object sender, EventArgs e)
        {
            base.DialogResult = DialogResult.Cancel;
            base.Close();
        }

        private void buttonOk_Click(object sender, EventArgs e)
        {
            this.SearchStr = this.textSearch.Text;
            this.MatchCase = this.checkMatchCase.Checked;
            this.WholeWord = this.checkWholeWord.Checked;
            base.DialogResult = DialogResult.OK;
            base.Close();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.label1 = new Label();
            this.textSearch = new TextBox();
            this.checkMatchCase = new CheckBox();
            this.checkWholeWord = new CheckBox();
            this.buttonOk = new Button();
            this.buttonCancel = new Button();
            base.SuspendLayout();
            this.label1.Location = new Point(0x18, 0x10);
            this.label1.Name = "label1";
            this.label1.Size = new Size(40, 0x10);
            this.label1.TabIndex = 0;
            this.label1.Text = "Search";
            this.textSearch.Location = new Point(0x40, 0x10);
            this.textSearch.Name = "textSearch";
            this.textSearch.Size = new Size(0xd0, 20);
            this.textSearch.TabIndex = 1;
            this.textSearch.Text = "";
            this.checkMatchCase.Location = new Point(0x18, 0x30);
            this.checkMatchCase.Name = "checkMatchCase";
            this.checkMatchCase.Size = new Size(0x58, 0x18);
            this.checkMatchCase.TabIndex = 2;
            this.checkMatchCase.Text = "Match Case";
            this.checkWholeWord.Location = new Point(0x18, 0x48);
            this.checkWholeWord.Name = "checkWholeWord";
            this.checkWholeWord.Size = new Size(0x58, 0x18);
            this.checkWholeWord.TabIndex = 2;
            this.checkWholeWord.Text = "Whole Word";
            this.buttonOk.Location = new Point(0x80, 0x38);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new Size(0x40, 0x18);
            this.buttonOk.TabIndex = 3;
            this.buttonOk.Text = "OK";
            this.buttonOk.Click += new EventHandler(this.buttonOk_Click);
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.Location = new Point(0xd0, 0x38);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new Size(0x40, 0x18);
            this.buttonCancel.TabIndex = 3;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new EventHandler(this.buttonCancel_Click);
            base.AcceptButton = this.buttonOk;
            base.CancelButton = this.buttonCancel;
            this.AutoScaleBaseSize = new Size(5, 13);
            base.ClientSize = new Size(0x124, 0x6d);
            base.ControlBox = false;
            base.Controls.AddRange(new Control[] { this.buttonOk, this.checkMatchCase, this.textSearch, this.label1, this.checkWholeWord, this.buttonCancel });
            base.FormBorderStyle = FormBorderStyle.FixedDialog;
            base.Name = "SearchDialog";
            this.Text = "SearchDialog";
            base.ResumeLayout(false);
        }
    }
}

