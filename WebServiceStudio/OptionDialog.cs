
    using System;
    using System.ComponentModel;
    using System.Drawing;
    using System.Windows.Forms;

namespace WebServiceStudio
{
    internal class OptionDialog : Form
    {
        private Button buttonCancel;
        private Button buttonOk;
        private Container components = null;
        private Panel panelBottomMain;
        private Panel panelTopMain;
        private PropertyGrid propertyOptions;

        public OptionDialog()
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
            base.DialogResult = DialogResult.OK;
            Configuration selectedObject = this.propertyOptions.SelectedObject as Configuration;
            if (selectedObject != null)
            {
                Configuration.MasterConfig = selectedObject;
            }
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
            this.propertyOptions = new PropertyGrid();
            this.buttonOk = new Button();
            this.buttonCancel = new Button();
            this.panelTopMain = new Panel();
            this.panelBottomMain = new Panel();
            this.panelTopMain.SuspendLayout();
            this.panelBottomMain.SuspendLayout();
            base.SuspendLayout();
            this.propertyOptions.CommandsVisibleIfAvailable = true;
            this.propertyOptions.HelpVisible = false;
            this.propertyOptions.LargeButtons = false;
            this.propertyOptions.LineColor = SystemColors.ScrollBar;
            this.propertyOptions.Location = new Point(8, 8);
            this.propertyOptions.Name = "propertyOptions";
            this.propertyOptions.PropertySort = PropertySort.Alphabetical;
            this.propertyOptions.Dock = DockStyle.Fill;
            this.propertyOptions.TabIndex = 0;
            this.propertyOptions.Text = "PropertyGrid";
            this.propertyOptions.ToolbarVisible = false;
            this.propertyOptions.ViewBackColor = SystemColors.Window;
            this.propertyOptions.ViewForeColor = SystemColors.WindowText;
            this.propertyOptions.SelectedObject = Configuration.MasterConfig.Copy();
            this.buttonOk.DialogResult = DialogResult.Cancel;
            this.buttonOk.FlatStyle = FlatStyle.Popup;
            this.buttonOk.Location = new Point(8, 5);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new Size(50, 20);
            this.buttonOk.TabIndex = 1;
            this.buttonOk.Text = "OK";
            this.buttonOk.Click += new EventHandler(this.buttonOk_Click);
            this.buttonCancel.DialogResult = DialogResult.Cancel;
            this.buttonCancel.FlatStyle = FlatStyle.Popup;
            this.buttonCancel.Location = new Point(70, 5);
            this.buttonCancel.Name = "buttonCancel";
            this.buttonCancel.Size = new Size(50, 20);
            this.buttonCancel.TabIndex = 1;
            this.buttonCancel.Text = "Cancel";
            this.buttonCancel.Click += new EventHandler(this.buttonCancel_Click);
            this.panelTopMain.BorderStyle = BorderStyle.None;
            this.panelTopMain.Controls.AddRange(new Control[] { this.propertyOptions });
            this.panelTopMain.Dock = DockStyle.Fill;
            this.panelTopMain.Name = "panelTopMain";
            this.panelTopMain.Size = new Size(0, 250);
            this.panelTopMain.TabIndex = 0;
            this.panelBottomMain.BorderStyle = BorderStyle.None;
            this.panelBottomMain.Controls.AddRange(new Control[] { this.buttonOk, this.buttonCancel });
            this.panelBottomMain.Dock = DockStyle.Bottom;
            this.panelBottomMain.Size = new Size(0, 30);
            this.panelBottomMain.Name = "panelBottomMain";
            this.panelBottomMain.TabIndex = 1;
            base.AcceptButton = this.buttonOk;
            base.CancelButton = this.buttonCancel;
            this.AutoScaleBaseSize = new Size(5, 13);
            base.ClientSize = new Size(0x110, 0x12b);
            base.Controls.AddRange(new Control[] { this.panelTopMain, this.panelBottomMain });
            base.Name = "OptionDialog";
            this.Text = "Options ";
            this.panelTopMain.ResumeLayout(false);
            this.panelBottomMain.ResumeLayout(false);
            base.ResumeLayout(false);
            this.propertyOptions.ExpandAllGridItems();
        }
    }
}

