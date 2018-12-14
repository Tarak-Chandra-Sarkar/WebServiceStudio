using System;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Windows.Forms;

namespace WebServiceStudio
{
    public class DataSetEditor : UITypeEditor
    {
        private EditForm editForm = null;

        public override object EditValue(ITypeDescriptorContext context, IServiceProvider provider, object value)
        {
            if (value != null)
            {
                if (this.editForm == null)
                {
                    this.editForm = new EditForm();
                }
                this.editForm.DataSource = ((DataSet) value).Copy();
                if (this.editForm.ShowDialog() == DialogResult.OK)
                {
                    value = this.editForm.DataSource;
                }
            }
            return value;
        }

        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            return UITypeEditorEditStyle.Modal;
        }

        internal class EditForm : Form
        {
            private Button Cancel;
            private DataGrid dataGrid1;
            private static string[] DataSetFileExtenstions = new string[] { "xsd", "xml" };
            private Label label1;
            private Button LoadXml;
            private Button OK;
            private Panel panelBottomMain;
            private Panel panelTopMain;

            internal EditForm()
            {
                this.InitializeComponent();
            }

            private void Cancel_Click(object sender, EventArgs e)
            {
                base.Close();
            }

            private void dataGrid1_Navigate(object sender, NavigateEventArgs ne)
            {
            }

            protected override void Dispose(bool dispose)
            {
                base.Dispose(dispose);
            }

            private void EditForm_Load(object sender, EventArgs e)
            {
                if (this.DataSource.Tables.Count == 1)
                {
                    this.dataGrid1.DataMember = this.DataSource.Tables[0].TableName;
                }
                else
                {
                    this.dataGrid1.DataMember = "";
                }
                this.label1.Text = this.DataSource.DataSetName;
            }

            private void InitializeComponent()
            {
                this.label1 = new Label();
                this.Cancel = new Button();
                this.OK = new Button();
                this.LoadXml = new Button();
                this.dataGrid1 = new DataGrid();
                this.panelTopMain = new Panel();
                this.panelBottomMain = new Panel();
                this.panelTopMain.SuspendLayout();
                this.panelBottomMain.SuspendLayout();
                base.SuspendLayout();
                this.label1.Font = new Font("Microsoft Sans Serif", 12f, FontStyle.Bold, GraphicsUnit.Point, 0);
                this.label1.Location = new Point(0x10, 8);
                this.label1.Name = "label1";
                this.label1.Size = new Size(200, 0x18);
                this.label1.TabIndex = 3;
                this.label1.Text = "Data Set";
                this.Cancel.Location = new Point(0xe0, 8);
                this.Cancel.FlatStyle = FlatStyle.Popup;
                this.Cancel.Name = "Cancel";
                this.Cancel.Size = new Size(0x60, 0x18);
                this.Cancel.TabIndex = 1;
                this.Cancel.Text = "Cancel";
                this.Cancel.Click += new EventHandler(this.Cancel_Click);
                this.OK.Location = new Point(0x148, 8);
                this.OK.FlatStyle = FlatStyle.Popup;
                this.OK.Name = "OK";
                this.OK.Size = new Size(0x60, 0x18);
                this.OK.TabIndex = 1;
                this.OK.Text = "OK";
                this.OK.Click += new EventHandler(this.OK_Click);
                this.LoadXml.Location = new Point(0x1b0, 8);
                this.LoadXml.FlatStyle = FlatStyle.Popup;
                this.LoadXml.Name = "LoadXml";
                this.LoadXml.Size = new Size(0x60, 0x18);
                this.LoadXml.TabIndex = 1;
                this.LoadXml.Text = "Load XML...";
                this.LoadXml.Click += new EventHandler(this.LoadXml_Click);
                this.dataGrid1.CaptionVisible = true;
                this.dataGrid1.DataMember = "";
                this.dataGrid1.Name = "dataGrid1";
                this.dataGrid1.Dock = DockStyle.Fill;
                this.dataGrid1.TabIndex = 4;
                this.dataGrid1.Navigate += new NavigateEventHandler(this.dataGrid1_Navigate);
                this.panelTopMain.BorderStyle = BorderStyle.None;
                this.panelTopMain.Controls.AddRange(new Control[] { this.label1, this.Cancel, this.OK, this.LoadXml });
                this.panelTopMain.Dock = DockStyle.Top;
                this.panelTopMain.Name = "panelTopMain";
                this.panelTopMain.Size = new Size(0, 50);
                this.panelTopMain.TabIndex = 0;
                this.panelBottomMain.BorderStyle = BorderStyle.None;
                this.panelBottomMain.Controls.AddRange(new Control[] { this.dataGrid1 });
                this.panelBottomMain.Dock = DockStyle.Fill;
                this.panelBottomMain.Location = new Point(0, 50);
                this.panelBottomMain.Name = "panelBottomMain";
                this.panelBottomMain.TabIndex = 1;
                this.panelBottomMain.SizeChanged += new EventHandler(this.PanelBottomMain_SizeChanged);
                base.AcceptButton = this.OK;
                base.CancelButton = this.Cancel;
                this.AutoScaleBaseSize = new Size(5, 13);
                base.ClientSize = new Size(0x2c0, 0x14d);
                base.Controls.AddRange(new Control[] { this.panelBottomMain, this.panelTopMain });
                base.Name = "EditForm";
                this.Text = "Form1";
                base.FormBorderStyle = FormBorderStyle.Fixed3D;
                base.Load += new EventHandler(this.EditForm_Load);
                this.dataGrid1.EndInit();
                base.ResumeLayout(false);
            }

            private void LoadXml_Click(object sender, EventArgs e)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.Filter = "Schema Files (*.xsd) |*.xsd|Data Files (*.xml)|*.xml|All Files (*.*)|*.*";
                if (dialog.ShowDialog() == DialogResult.OK)
                {
                    this.DataSource.ReadXml(dialog.FileName);
                    this.EditForm_Load(sender, e);
                }
            }

            private void OK_Click(object sender, EventArgs e)
            {
                base.DialogResult = DialogResult.OK;
                base.Close();
            }

            private void PanelBottomMain_SizeChanged(object sender, EventArgs e)
            {
                this.dataGrid1.SetBounds(0, 0, this.panelBottomMain.Width, this.panelBottomMain.Height, BoundsSpecified.Size);
            }

            internal DataSet DataSource
            {
                get
                {
                    return (this.dataGrid1.DataSource as DataSet);
                }
                set
                {
                    this.dataGrid1.DataSource = value;
                    this.LoadXml.Enabled = value.GetType() == typeof(DataSet);
                }
            }
        }
    }
}

