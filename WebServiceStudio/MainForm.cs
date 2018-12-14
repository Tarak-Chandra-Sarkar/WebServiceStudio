    using System;
    using System.ComponentModel;
    using System.Data;
    using System.Drawing;
    using System.IO;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Threading;
    using System.Web.Services.Protocols;
    using System.Windows.Forms;
    using System.Xml.Serialization;

namespace WebServiceStudio
{
    public class MainForm : Form
    {
        private Button buttonBrowseFile;
        private Button buttonGet;
        private Button buttonInvoke;
        private Button buttonSend;
        private Container components = null;
        private RichTextBoxFinds findOption = RichTextBoxFinds.None;
        private static bool isV1 = false;
        private Label labelEndPointUrl;
        private Label labelInput;
        private Label labelInputValue;
        private Label labelOutput;
        private Label labelOutputValue;
        private Label labelRequest;
        private Label labelResponse;
        private MainMenu mainMenu1;
        private MenuItem menuItem1;
        private MenuItem menuItem2;
        private MenuItem menuItem3;
        private MenuItem menuItemAbout;
        private MenuItem menuItemExit;
        private MenuItem menuItemFind;
        private MenuItem menuItemFindNext;
        private MenuItem menuItemHelp;
        private MenuItem menuItemOptions;
        private MenuItem menuItemSaveAll;
        private MenuItem menuItemTreeInputCopy;
        private MenuItem menuItemTreeInputPaste;
        private MenuItem menuItemTreeOutputCopy;
        private static string MiniHelpText = "\r\n        .NET Webservice Studio is a tool to invoke webmethods interactively. The user can provide a WSDL endpoint. On clicking button Get the tool fetches the WSDL, generates .NET proxy from the WSDL and displays the list of methods available. The user can choose any method and provide the required input parameters. On clicking Invoke the SOAP request is sent to the server and the response is parsed to display the return value.\r\n        ";
        private OpenFileDialog openWsdlDialog;
        private Panel panelBottomMain;
        private Panel panelLeftInvoke;
        private Panel panelLeftRaw;
        private Panel panelLeftWsdl;
        private Panel panelRightInvoke;
        private Panel panelRightRaw;
        private Panel panelRightWsdl;
        private Panel panelTopMain;
        private PropertyGrid propInput;
        private PropertyGrid propOutput;
        private PropertyGrid propRequest;
        private RichTextBox richMessage;
        private RichTextBox richRequest;
        private RichTextBox richResponse;
        private RichTextBox richWsdl;
        private SaveFileDialog saveAllDialog;
        private string searchStr = "";
        private Splitter splitterInvoke;
        private Splitter splitterRaw;
        private Splitter splitterWsdl;
        private TabControl tabMain;
        private TabPage tabPageInvoke;
        private TabPage tabPageMessage;
        private TabPage tabPageRaw;
        private TabPage tabPageWsdl;
        private ComboBox textEndPointUri;
        private ToolBarButton toolBarButton1;
        private TreeView treeInput;
        private TreeView treeMethods;
        private TreeView treeOutput;
        private TreeView treeWsdl;
        private Wsdl wsdl = null;

        public MainForm()
        {
            this.InitializeComponent();
            this.wsdl = new WebServiceStudio.Wsdl();
						Control.CheckForIllegalCrossThreadCalls = false;
        }

        private void buttonBrowseFile_Click(object sender, EventArgs e)
        {
            if (this.openWsdlDialog.ShowDialog() == DialogResult.OK)
            {
                this.textEndPointUri.Text = this.openWsdlDialog.FileName;
            }
        }

        private void buttonGet_Click(object sender, EventArgs e)
        {
            if (this.buttonGet.Text == "Get")
            {
                this.ClearAllTabs();
                TabPage selectedTab = this.tabMain.SelectedTab;
                this.tabMain.SelectedTab = this.tabPageMessage;
                string text = this.textEndPointUri.Text;
                this.wsdl.Reset();
                this.wsdl.Paths.Add(text);
                new Thread(new ThreadStart(this.wsdl.Generate)).Start();
                this.buttonGet.Text = "Cancel";
            }
            else
            {
                this.buttonGet.Text = "Get";
                this.ShowMessageInternal(this, MessageType.Failure, "Cancelled");
                this.wsdl.Reset();
                this.wsdl = new WebServiceStudio.Wsdl();
            }
        }

        private void buttonInvoke_Click(object sender, EventArgs e)
        {
            Cursor cursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                this.propOutput.SelectedObject = null;
                this.treeOutput.Nodes.Clear();
                this.InvokeWebMethod();
            }
            finally
            {
                this.Cursor = cursor;
            }
        }

        private void buttonSend_Click(object sender, EventArgs e)
        {
            this.SendWebRequest();
        }

        private void ClearAllTabs()
        {
            this.richWsdl.Clear();
            this.richWsdl.Font = Configuration.MasterConfig.UiSettings.WsdlFont;
            this.treeWsdl.Nodes.Clear();
            this.richMessage.Clear();
            this.richMessage.Font = Configuration.MasterConfig.UiSettings.MessageFont;
            this.richRequest.Clear();
            this.richRequest.Font = Configuration.MasterConfig.UiSettings.ReqRespFont;
            this.richResponse.Clear();
            this.richResponse.Font = Configuration.MasterConfig.UiSettings.ReqRespFont;
            this.treeMethods.Nodes.Clear();
            TreeNodeProperty.ClearIncludedTypes();
            this.treeInput.Nodes.Clear();
            this.treeOutput.Nodes.Clear();
            this.propInput.SelectedObject = null;
            this.propOutput.SelectedObject = null;
        }

        private void CopyToClipboard(TreeNodeProperty tnp)
        {
            if (!this.IsValidCopyNode(tnp))
            {
                throw new Exception("Cannot copy from here");
            }
            object o = tnp.ReadChildren();
            if (o != null)
            {
                StringWriter writer = new StringWriter();
                System.Type[] extraTypes = new System.Type[] { o.GetType() };
                System.Type type = (o is DataSet) ? typeof(DataSet) : typeof(object);
                new XmlSerializer(type, extraTypes).Serialize((TextWriter) writer, o);
                Clipboard.SetDataObject(writer.ToString());
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing && (this.components != null))
            {
                this.components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void DumpResponse(HttpWebResponse response)
        {
            this.richResponse.Text = WSSWebResponse.DumpResponse(response);
        }

        private void FillInvokeTab()
        {
            Assembly proxyAssembly = this.wsdl.ProxyAssembly;
            if (proxyAssembly != null)
            {
                this.treeMethods.Nodes.Clear();
                foreach (System.Type type in proxyAssembly.GetTypes())
                {
                    if (TreeNodeProperty.IsWebService(type))
                    {
                        TreeNode node = this.treeMethods.Nodes.Add(type.Name);
                        HttpWebClientProtocol proxy = (HttpWebClientProtocol) Activator.CreateInstance(type);
                        ProxyProperty property = new ProxyProperty(proxy);
                        property.RecreateSubtree(null);
                        node.Tag = property.TreeNode;
                        proxy.Credentials = CredentialCache.DefaultCredentials;
                        SoapHttpClientProtocol protocol2 = proxy as SoapHttpClientProtocol;
                        if (protocol2 != null)
                        {
                            protocol2.CookieContainer = new CookieContainer();
                            protocol2.AllowAutoRedirect = true;
                        }
                        foreach (MethodInfo info in type.GetMethods())
                        {
                            if (TreeNodeProperty.IsWebMethod(info))
                            {
                                node.Nodes.Add(info.Name).Tag = info;
                            }
                        }
                    }
                }
                this.treeMethods.ExpandAll();
            }
        }

        private void FillWsdlTab()
        {
            if ((this.wsdl.Wsdls != null) && (this.wsdl.Wsdls.Count != 0))
            {
                int num3;
                this.richWsdl.Text = this.wsdl.Wsdls[0];
                this.treeWsdl.Nodes.Clear();
                TreeNode node = this.treeWsdl.Nodes.Add("WSDLs");
                XmlTreeWriter writer = new XmlTreeWriter();
                for (int i = 0; i < this.wsdl.Wsdls.Count; i++)
                {
                    num3 = i + 1;
                    TreeNode root = node.Nodes.Add("WSDL#" + num3.ToString());
                    root.Tag = this.wsdl.Wsdls[i];
                    writer.FillTree(this.wsdl.Wsdls[i], root);
                }
                TreeNode node3 = this.treeWsdl.Nodes.Add("Schemas");
                for (int j = 0; j < this.wsdl.Xsds.Count; j++)
                {
                    num3 = j + 1;
                    TreeNode node4 = node3.Nodes.Add("Schema#" + num3.ToString());
                    node4.Tag = this.wsdl.Xsds[j];
                    writer.FillTree(this.wsdl.Xsds[j], node4);
                }
                this.treeWsdl.Nodes.Add("Proxy").Tag = this.wsdl.ProxyCode;
                this.treeWsdl.Nodes.Add("ClientCode").Tag = "Shows client code for all methods accessed in the invoke tab";
                node.Expand();
            }
        }

        private void Find()
        {
            this.tabMain.SelectedTab = this.tabPageWsdl;
            this.richWsdl.Find(this.searchStr, this.richWsdl.SelectionStart + this.richWsdl.SelectionLength, this.findOption);
        }

        private string GenerateClientCode()
        {
            Script script = new Script(this.wsdl.ProxyNamespace, "MainClass");
            foreach (TreeNode node in this.treeMethods.Nodes)
            {
                script.Proxy = this.GetProxyPropertyFromNode(node).GetProxy();
                foreach (TreeNode node2 in node.Nodes)
                {
                    TreeNode tag = node2.Tag as TreeNode;
                    if (tag != null)
                    {
                        MethodProperty property = tag.Tag as MethodProperty;
                        if (property != null)
                        {
                            MethodInfo method = property.GetMethod();
                            object[] parameters = property.ReadChildren() as object[];
                            script.AddMethod(method, parameters);
                        }
                    }
                }
            }
            return script.Generate(this.wsdl.GetCodeGenerator());
        }

        private MethodProperty GetCurrentMethodProperty()
        {
            if ((this.treeInput.Nodes == null) || (this.treeInput.Nodes.Count == 0))
            {
                MessageBox.Show(this, "Select a web method to execute");
                return null;
            }
            TreeNode node = this.treeInput.Nodes[0];
            MethodProperty tag = node.Tag as MethodProperty;
            if (tag == null)
            {
                MessageBox.Show(this, "Select a method to execute");
                return null;
            }
            return tag;
        }

        private ProxyProperty GetProxyPropertyFromNode(TreeNode treeNode)
        {
            while (treeNode.Parent != null)
            {
                treeNode = treeNode.Parent;
            }
            TreeNode tag = treeNode.Tag as TreeNode;
            if (tag != null)
            {
                return (tag.Tag as ProxyProperty);
            }
            return null;
        }

        private void InitializeComponent()
        {
            this.textEndPointUri = new ComboBox();
            this.buttonGet = new Button();
            this.labelEndPointUrl = new Label();
            this.mainMenu1 = new MainMenu();
            this.menuItem1 = new MenuItem();
            this.menuItemSaveAll = new MenuItem();
            this.menuItemExit = new MenuItem();
            this.menuItem2 = new MenuItem();
            this.menuItemTreeOutputCopy = new MenuItem();
            this.menuItemTreeInputCopy = new MenuItem();
            this.menuItemTreeInputPaste = new MenuItem();
            this.menuItemFind = new MenuItem();
            this.menuItemFindNext = new MenuItem();
            this.menuItemOptions = new MenuItem();
            this.menuItem3 = new MenuItem();
            this.menuItemAbout = new MenuItem();
            this.menuItemHelp = new MenuItem();
            this.openWsdlDialog = new OpenFileDialog();
            this.toolBarButton1 = new ToolBarButton();
            this.buttonBrowseFile = new Button();
            this.saveAllDialog = new SaveFileDialog();
            this.tabPageInvoke = new TabPage();
            this.panelLeftInvoke = new Panel();
            this.panelRightInvoke = new Panel();
            this.splitterInvoke = new Splitter();
            this.propOutput = new PropertyGrid();
            this.propInput = new PropertyGrid();
            this.labelOutputValue = new Label();
            this.labelInputValue = new Label();
            this.treeMethods = new TreeView();
            this.labelOutput = new Label();
            this.labelInput = new Label();
            this.treeOutput = new TreeView();
            this.buttonInvoke = new Button();
            this.treeInput = new TreeView();
            this.tabPageWsdl = new TabPage();
            this.panelLeftWsdl = new Panel();
            this.panelRightWsdl = new Panel();
            this.splitterWsdl = new Splitter();
            this.treeWsdl = new TreeView();
            this.richWsdl = new RichTextBox();
            this.tabPageMessage = new TabPage();
            this.richMessage = new RichTextBox();
            this.tabPageRaw = new TabPage();
            this.panelLeftRaw = new Panel();
            this.panelRightRaw = new Panel();
            this.splitterRaw = new Splitter();
            this.buttonSend = new Button();
            this.richRequest = new RichTextBox();
            this.propRequest = new PropertyGrid();
            this.richResponse = new RichTextBox();
            this.labelRequest = new Label();
            this.labelResponse = new Label();
            this.tabMain = new TabControl();
            this.panelTopMain = new Panel();
            this.panelBottomMain = new Panel();
            this.tabPageInvoke.SuspendLayout();
            this.panelLeftInvoke.SuspendLayout();
            this.panelRightInvoke.SuspendLayout();
            this.tabPageWsdl.SuspendLayout();
            this.panelLeftWsdl.SuspendLayout();
            this.panelRightWsdl.SuspendLayout();
            this.tabPageMessage.SuspendLayout();
            this.tabPageRaw.SuspendLayout();
            this.panelLeftRaw.SuspendLayout();
            this.panelRightRaw.SuspendLayout();
            this.tabMain.SuspendLayout();
            this.panelTopMain.SuspendLayout();
            this.panelBottomMain.SuspendLayout();
            base.SuspendLayout();
            this.textEndPointUri.Location = new Point(0x58, 0x10);
            this.textEndPointUri.Name = "textEndPointUri";
            this.textEndPointUri.Size = new Size(0x1bc, 20);
            this.textEndPointUri.DropDownStyle = ComboBoxStyle.DropDown;
            this.textEndPointUri.Items.AddRange(Configuration.MasterConfig.InvokeSettings.RecentlyUsedUris);
            if (this.textEndPointUri.Items.Count > 0)
            {
                this.textEndPointUri.SelectedIndex = 0;
            }
            else
            {
                this.textEndPointUri.Text = "";
            }
            this.textEndPointUri.KeyPress += new KeyPressEventHandler(this.textEndPointUri_KeyPress);
            this.buttonGet.Location = new Point(0x298, 12);
            this.buttonGet.Name = "buttonGet";
            this.buttonGet.FlatStyle = FlatStyle.Popup;
            this.buttonGet.Size = new Size(60, 0x18);
            this.buttonGet.Text = "Get";
            this.buttonGet.Click += new EventHandler(this.buttonGet_Click);
            this.labelEndPointUrl.Location = new Point(0, 0x10);
            this.labelEndPointUrl.Name = "labelEndPointUrl";
            this.labelEndPointUrl.Size = new Size(0x58, 0x18);
            this.labelEndPointUrl.Text = "WSDL EndPoint";
            this.mainMenu1.MenuItems.AddRange(new MenuItem[] { this.menuItem1, this.menuItem2, this.menuItem3 });
            this.menuItem1.Index = 0;
            this.menuItem1.MenuItems.AddRange(new MenuItem[] { this.menuItemSaveAll, this.menuItemExit });
            this.menuItem1.Text = "File";
            this.menuItemSaveAll.Index = 0;
            this.menuItemSaveAll.Text = "Save All Files...";
            this.menuItemSaveAll.Click += new EventHandler(this.menuItemSaveAll_Click);
            this.menuItemExit.Index = 1;
            this.menuItemExit.Text = "Exit";
            this.menuItemExit.Click += new EventHandler(this.menuItemExit_Click);
            this.menuItem2.Index = 1;
            this.menuItem2.MenuItems.AddRange(new MenuItem[] { this.menuItemFind, this.menuItemFindNext, this.menuItemOptions });
            this.menuItem2.Text = "Edit";
            this.menuItemFind.Index = 0;
            this.menuItemFind.Shortcut = Shortcut.CtrlF;
            this.menuItemFind.Text = "Find...";
            this.menuItemFind.Click += new EventHandler(this.menuItemFind_Click);
            this.menuItemFindNext.Index = 1;
            this.menuItemFindNext.Shortcut = Shortcut.F3;
            this.menuItemFindNext.Text = "Find Next";
            this.menuItemFindNext.Click += new EventHandler(this.menuItemFindNext_Click);
            this.menuItemOptions.Index = 2;
            this.menuItemOptions.Text = "Options...";
            this.menuItemOptions.Click += new EventHandler(this.menuItemOptions_Click);
            this.menuItem3.Index = 2;
            this.menuItem3.MenuItems.AddRange(new MenuItem[] { this.menuItemHelp, this.menuItemAbout });
            this.menuItem3.Text = "Help";
            this.menuItemAbout.Index = 1;
            this.menuItemAbout.Text = "About...";
            this.menuItemAbout.Click += new EventHandler(this.menuItemAbout_Click);
            this.menuItemHelp.Index = 0;
            this.menuItemHelp.Text = "Help";
            this.menuItemHelp.Click += new EventHandler(this.menuItemHelp_Click);
            try
            {
                this.openWsdlDialog.DefaultExt = "wsdl";
                this.openWsdlDialog.Multiselect = true;
                this.openWsdlDialog.Title = "Open WSDL";
                this.openWsdlDialog.CheckFileExists = false;
                this.openWsdlDialog.CheckPathExists = false;
                this.saveAllDialog.FileName = "doc1";
            }
            catch (Exception exception)
            {
                Console.WriteLine(exception.ToString());
            }
            this.toolBarButton1.Text = "Open Wsdl...";
            this.toolBarButton1.ToolTipText = "Open WSDL file(s)";
            this.buttonBrowseFile.Location = new Point(540, 12);
            this.buttonBrowseFile.FlatStyle = FlatStyle.Popup;
            this.buttonBrowseFile.Name = "buttonBrowseFile";
            this.buttonBrowseFile.Size = new Size(0x74, 0x18);
            this.buttonBrowseFile.Text = "Browse Wsdl ...";
            this.buttonBrowseFile.TextAlign = ContentAlignment.TopCenter;
            this.buttonBrowseFile.Click += new EventHandler(this.buttonBrowseFile_Click);
            this.tabPageInvoke.Controls.AddRange(new Control[] { this.splitterInvoke, this.panelRightInvoke, this.panelLeftInvoke });
            this.tabPageInvoke.Name = "tabPageInvoke";
            this.tabPageInvoke.Tag = "";
            this.tabPageInvoke.Text = "Invoke";
            this.panelLeftInvoke.BorderStyle = BorderStyle.None;
            this.panelLeftInvoke.Controls.AddRange(new Control[] { this.treeMethods });
            this.panelLeftInvoke.Dock = DockStyle.Left;
            this.panelLeftInvoke.Name = "panelLeftInvoke";
            this.panelLeftInvoke.Size = new Size(0xd0, 0x1fd);
            this.panelRightInvoke.BorderStyle = BorderStyle.None;
            this.panelRightInvoke.Controls.AddRange(new Control[] { this.labelOutputValue, this.labelInputValue, this.labelOutput, this.labelInput, this.treeInput, this.treeOutput, this.propOutput, this.propInput, this.buttonInvoke });
            this.panelRightInvoke.Dock = DockStyle.Fill;
            this.panelRightInvoke.Name = "panelRightInvoke";
            this.panelRightInvoke.Location = new Point(0xd0, 0);
            this.panelRightInvoke.Size = new Size(0x228, 0x1fd);
            this.panelRightInvoke.SizeChanged += new EventHandler(this.PanelRightInvoke_SizeChanged);
            this.splitterInvoke.Location = new Point(0xd0, 0);
            this.splitterInvoke.Name = "splitterInvoke";
            this.splitterInvoke.Size = new Size(3, 0x1fd);
            this.splitterInvoke.TabStop = false;
            this.propOutput.CommandsVisibleIfAvailable = true;
            this.propOutput.HelpVisible = false;
            this.propOutput.LargeButtons = false;
            this.propOutput.LineColor = SystemColors.ScrollBar;
            this.propOutput.Location = new Point(0x110, 0x158);
            this.propOutput.Name = "propOutput";
            this.propOutput.PropertySort = PropertySort.NoSort;
            this.propOutput.Size = new Size(0xe8, 0x110);
            this.propOutput.Text = "propOutput";
            this.propOutput.ToolbarVisible = false;
            this.propOutput.ViewBackColor = SystemColors.Window;
            this.propOutput.ViewForeColor = SystemColors.WindowText;
            this.propInput.CommandsVisibleIfAvailable = true;
            this.propInput.HelpVisible = false;
            this.propInput.LargeButtons = false;
            this.propInput.LineColor = SystemColors.ScrollBar;
            this.propInput.Location = new Point(0x110, 0x18);
            this.propInput.Name = "propInput";
            this.propInput.PropertySort = PropertySort.NoSort;
            this.propInput.Size = new Size(0xe8, 0x110);
            this.propInput.Text = "propInput";
            this.propInput.ToolbarVisible = false;
            this.propInput.ViewBackColor = SystemColors.Window;
            this.propInput.ViewForeColor = SystemColors.WindowText;
            this.propInput.PropertyValueChanged += new PropertyValueChangedEventHandler(this.propInput_PropertyValueChanged);
            this.treeMethods.HideSelection = false;
            this.treeMethods.ImageIndex = -1;
            this.treeMethods.Dock = DockStyle.Fill;
            this.treeMethods.Name = "treeMethods";
            this.treeMethods.SelectedImageIndex = -1;
            this.treeMethods.AfterSelect += new TreeViewEventHandler(this.treeMethods_AfterSelect);
            this.labelInputValue.Location = new Point(0x110, 8);
            this.labelInputValue.Name = "labelInputValue";
            this.labelInputValue.Size = new Size(0x38, 0x10);
            this.labelInputValue.Text = "Value";
            this.labelOutputValue.Location = new Point(0x110, 320);
            this.labelOutputValue.Name = "labelOutputValue";
            this.labelOutputValue.Size = new Size(0x38, 0x10);
            this.labelOutputValue.Text = "Value";
            this.labelOutput.Location = new Point(8, 320);
            this.labelOutput.Name = "labelOutput";
            this.labelOutput.Size = new Size(0x40, 0x10);
            this.labelOutput.Text = "Output";
            this.labelInput.Location = new Point(8, 8);
            this.labelInput.Name = "labelInput";
            this.labelInput.Size = new Size(0x70, 0x10);
            this.labelInput.Text = "Input";
            this.treeOutput.ImageIndex = -1;
            this.treeOutput.Location = new Point(8, 0x158);
            this.treeOutput.Name = "treeOutput";
            this.treeOutput.SelectedImageIndex = -1;
            this.treeOutput.Size = new Size(0x100, 0x110);
            this.treeOutput.AfterSelect += new TreeViewEventHandler(this.treeOutput_AfterSelect);
            this.treeOutput.ContextMenu = new ContextMenu();
            this.treeOutput.ContextMenu.MenuItems.Add(this.menuItemTreeOutputCopy);
            this.menuItemTreeOutputCopy.Index = 0;
            this.menuItemTreeOutputCopy.Shortcut = Shortcut.CtrlC;
            this.menuItemTreeOutputCopy.Text = "Copy";
            this.menuItemTreeOutputCopy.Click += new EventHandler(this.treeOutputMenuCopy_Click);
            this.buttonInvoke.Location = new Point(0x1c8, 0x138);
            this.buttonInvoke.Name = "buttonInvoke";
            this.buttonInvoke.FlatStyle = FlatStyle.Popup;
            this.buttonInvoke.Size = new Size(0x38, 0x18);
            this.buttonInvoke.Text = "Invoke";
            this.buttonInvoke.Click += new EventHandler(this.buttonInvoke_Click);
            this.treeInput.HideSelection = false;
            this.treeInput.ImageIndex = -1;
            this.treeInput.Location = new Point(8, 0x18);
            this.treeInput.Name = "treeInput";
            this.treeInput.SelectedImageIndex = -1;
            this.treeInput.Size = new Size(0x100, 0x110);
            this.treeInput.AfterSelect += new TreeViewEventHandler(this.treeInput_AfterSelect);
            this.treeInput.ContextMenu = new ContextMenu();
            this.treeInput.ContextMenu.MenuItems.Add(this.menuItemTreeInputCopy);
            this.treeInput.ContextMenu.MenuItems.Add(this.menuItemTreeInputPaste);
            this.menuItemTreeInputCopy.Index = 0;
            this.menuItemTreeInputCopy.Shortcut = Shortcut.CtrlC;
            this.menuItemTreeInputCopy.Text = "Copy";
            this.menuItemTreeInputCopy.Click += new EventHandler(this.treeInputMenuCopy_Click);
            this.menuItemTreeInputPaste.Index = 1;
            this.menuItemTreeInputPaste.Shortcut = Shortcut.CtrlV;
            this.menuItemTreeInputPaste.Text = "Paste";
            this.menuItemTreeInputPaste.Click += new EventHandler(this.treeInputMenuPaste_Click);
            this.tabPageWsdl.Controls.AddRange(new Control[] { this.splitterWsdl, this.panelRightWsdl, this.panelLeftWsdl });
            this.tabPageWsdl.Name = "tabPageWsdl";
            this.tabPageWsdl.Tag = "";
            this.tabPageWsdl.Text = "WSDLs & Proxy";
            this.panelLeftWsdl.BorderStyle = BorderStyle.None;
            this.panelLeftWsdl.Controls.AddRange(new Control[] { this.treeWsdl });
            this.panelLeftWsdl.Dock = DockStyle.Left;
            this.panelLeftWsdl.Name = "panelLeftWsdl";
            this.panelLeftWsdl.Size = new Size(0xd0, 0x1fd);
            this.panelRightWsdl.BorderStyle = BorderStyle.None;
            this.panelRightWsdl.Controls.AddRange(new Control[] { this.richWsdl });
            this.panelRightWsdl.Dock = DockStyle.Fill;
            this.panelRightWsdl.Name = "panelRightWsdl";
            this.panelRightWsdl.Location = new Point(0xd0, 0);
            this.panelRightWsdl.Size = new Size(0x228, 0x1fd);
            this.splitterWsdl.Location = new Point(0xd0, 0);
            this.splitterWsdl.Name = "splitterWsdl";
            this.splitterWsdl.Size = new Size(3, 0x1fd);
            this.splitterWsdl.TabStop = false;
            this.treeWsdl.ImageIndex = -1;
            this.treeWsdl.Dock = DockStyle.Fill;
            this.treeWsdl.Name = "treeWsdl";
            this.treeWsdl.SelectedImageIndex = -1;
            this.treeWsdl.AfterSelect += new TreeViewEventHandler(this.treeWsdl_AfterSelect);
            this.richWsdl.Font = Configuration.MasterConfig.UiSettings.WsdlFont;
            this.richWsdl.Dock = DockStyle.Fill;
            this.richWsdl.Name = "richWsdl";
            this.richWsdl.ReadOnly = true;
            this.richWsdl.Text = "";
            this.richWsdl.WordWrap = false;
            this.richWsdl.HideSelection = false;
            this.tabPageMessage.Controls.AddRange(new Control[] { this.richMessage });
            this.tabPageMessage.Name = "tabPageMessage";
            this.tabPageMessage.Tag = "";
            this.tabPageMessage.Text = "Messages";
            this.richMessage.Font = Configuration.MasterConfig.UiSettings.MessageFont;
            this.richMessage.Dock = DockStyle.Fill;
            this.richMessage.Name = "richMessage";
            this.richMessage.ReadOnly = true;
            this.richMessage.Text = "";
            this.tabMain.Controls.AddRange(new Control[] { this.tabPageInvoke, this.tabPageRaw, this.tabPageWsdl, this.tabPageMessage });
            this.tabMain.Dock = DockStyle.Fill;
            this.tabMain.Font = new Font("Microsoft Sans Serif", 8.25f, FontStyle.Regular, GraphicsUnit.Point, 0);
            this.tabMain.ItemSize = new Size(0x2a, 0x12);
            this.tabMain.Name = "tabMain";
            this.tabMain.SelectedIndex = 0;
            this.tabMain.Appearance = TabAppearance.FlatButtons;
            this.tabMain.SelectedIndexChanged += new EventHandler(this.tabMain_SelectedIndexChanged);
            this.tabPageRaw.Controls.AddRange(new Control[] { this.splitterRaw, this.panelRightRaw, this.panelLeftRaw });
            this.tabPageRaw.Name = "tabPageRaw";
            this.tabPageRaw.Text = "Request/Response";
            this.panelLeftRaw.BorderStyle = BorderStyle.None;
            this.panelLeftRaw.Controls.AddRange(new Control[] { this.propRequest });
            this.panelLeftRaw.Dock = DockStyle.Left;
            this.panelLeftRaw.Name = "panelLeftRaw";
            this.panelLeftRaw.Size = new Size(0xd0, 0x1fd);
            this.panelLeftRaw.SizeChanged += new EventHandler(this.PanelLeftRaw_SizeChanged);
            this.panelRightRaw.BorderStyle = BorderStyle.None;
            this.panelRightRaw.Controls.AddRange(new Control[] { this.buttonSend, this.richRequest, this.richResponse, this.labelRequest, this.labelResponse });
            this.panelRightRaw.Dock = DockStyle.Fill;
            this.panelRightRaw.Name = "panelRightRaw";
            this.panelRightRaw.Location = new Point(0xd0, 0);
            this.panelRightRaw.Size = new Size(0x228, 0x1fd);
            this.panelRightRaw.SizeChanged += new EventHandler(this.PanelRightRaw_SizeChanged);
            this.splitterRaw.Location = new Point(0xd0, 0);
            this.splitterRaw.Name = "splitterRaw";
            this.splitterRaw.Size = new Size(3, 0x1fd);
            this.splitterRaw.TabStop = false;
            this.buttonSend.Location = new Point(0x2b8, 0x138);
            this.buttonSend.FlatStyle = FlatStyle.Popup;
            this.buttonSend.Name = "buttonSend";
            this.buttonSend.Size = new Size(0x38, 0x18);
            this.buttonSend.Text = "Send";
            this.buttonSend.Click += new EventHandler(this.buttonSend_Click);
            this.richRequest.Font = Configuration.MasterConfig.UiSettings.ReqRespFont;
            this.richRequest.Location = new Point(240, 0x18);
            this.richRequest.Name = "richRequest";
            this.richRequest.Size = new Size(0x200, 0x110);
            this.richRequest.Text = "";
            this.richRequest.WordWrap = false;
            this.propRequest.Dock = DockStyle.Fill;
            this.propRequest.CommandsVisibleIfAvailable = true;
            this.propRequest.HelpVisible = false;
            this.propRequest.LargeButtons = false;
            this.propRequest.LineColor = SystemColors.ScrollBar;
            this.propRequest.Name = "propRequest";
            this.propRequest.PropertySort = PropertySort.Alphabetical;
            this.propRequest.Text = "propRequest";
            this.propRequest.ToolbarVisible = false;
            this.propRequest.ViewBackColor = SystemColors.Window;
            this.propRequest.ViewForeColor = SystemColors.WindowText;
            this.richResponse.Font = Configuration.MasterConfig.UiSettings.ReqRespFont;
            this.richResponse.Location = new Point(240, 0x158);
            this.richResponse.Name = "richResponse";
            this.richResponse.ReadOnly = true;
            this.richResponse.Size = new Size(0x200, 0x110);
            this.richResponse.Text = "";
            this.richResponse.WordWrap = false;
            this.labelRequest.Location = new Point(240, 8);
            this.labelRequest.Name = "labelRequest";
            this.labelRequest.Size = new Size(0x90, 0x10);
            this.labelRequest.Text = "Request";
            this.labelResponse.Location = new Point(240, 0x148);
            this.labelResponse.Name = "labelResponse";
            this.labelResponse.Size = new Size(0x70, 0x10);
            this.labelResponse.Text = "Response";
            this.panelTopMain.BorderStyle = BorderStyle.None;
            this.panelTopMain.Controls.AddRange(new Control[] { this.labelEndPointUrl, this.textEndPointUri, this.buttonBrowseFile, this.buttonGet });
            this.panelTopMain.Dock = DockStyle.Top;
            this.panelTopMain.Name = "panelTopMain";
            this.panelTopMain.Size = new Size(0, 50);
            this.panelTopMain.TabIndex = 0;
            this.panelBottomMain.BorderStyle = BorderStyle.None;
            this.panelBottomMain.Controls.AddRange(new Control[] { this.tabMain });
            this.panelBottomMain.Dock = DockStyle.Fill;
            this.panelBottomMain.Location = new Point(0, 50);
            this.panelBottomMain.Name = "panelBottomMain";
            this.panelBottomMain.TabIndex = 1;
            this.AutoScaleBaseSize = new Size(5, 13);
            base.ClientSize = new Size(0x2fe, 0x2bf);
            base.Controls.AddRange(new Control[] { this.panelBottomMain, this.panelTopMain });
            base.Icon = new Icon(typeof(MainForm), "WebServiceStudio.ico");
            base.Menu = this.mainMenu1;
            base.Name = "MainForm";
            this.Text = ".NET WebService Studio";
            this.tabPageInvoke.ResumeLayout(false);
            this.panelLeftInvoke.ResumeLayout(false);
            this.panelRightInvoke.ResumeLayout(false);
            this.tabPageWsdl.ResumeLayout(false);
            this.panelLeftWsdl.ResumeLayout(false);
            this.panelRightWsdl.ResumeLayout(false);
            this.tabPageRaw.ResumeLayout(false);
            this.tabPageMessage.ResumeLayout(false);
            this.tabMain.ResumeLayout(false);
            this.panelTopMain.ResumeLayout(false);
            this.panelBottomMain.ResumeLayout(false);
            base.ResumeLayout(false);
        }

        private void InvokeWebMethod()
        {
            MethodProperty currentMethodProperty = this.GetCurrentMethodProperty();
            if (currentMethodProperty != null)
            {
                HttpWebClientProtocol proxy = currentMethodProperty.GetProxyProperty().GetProxy();
                RequestProperties properties = new RequestProperties(proxy);
                try
                {
                    MethodInfo method = currentMethodProperty.GetMethod();
                    System.Type declaringType = method.DeclaringType;
                    WSSWebRequest.RequestTrace = properties;
                    object[] parameters = currentMethodProperty.ReadChildren() as object[];
                    object result = method.Invoke(proxy, BindingFlags.Public, null, parameters, null);
                    this.treeOutput.Nodes.Clear();
                    MethodProperty property2 = new MethodProperty(currentMethodProperty.GetProxyProperty(), method, result, parameters);
                    property2.RecreateSubtree(null);
                    this.treeOutput.Nodes.Add(property2.TreeNode);
                    this.treeOutput.ExpandAll();
                }
                finally
                {
                    WSSWebRequest.RequestTrace = null;
                    this.propRequest.SelectedObject = properties;
                    this.richRequest.Text = properties.requestPayLoad;
                    this.richResponse.Text = properties.responsePayLoad;
                }
            }
        }

        private bool IsValidCopyNode(TreeNodeProperty tnp)
        {
            return (((tnp != null) && (tnp.TreeNode.Parent != null)) && (tnp.GetType() != typeof(TreeNodeProperty)));
        }

        private bool IsValidPasteNode(TreeNodeProperty tnp)
        {
            IDataObject dataObject = Clipboard.GetDataObject();
            if ((dataObject == null) || (dataObject.GetData(DataFormats.Text) == null))
            {
                return false;
            }
            return this.IsValidCopyNode(tnp);
        }
        
        private void MainForm_SizeChanged(object sender, EventArgs e)
        {
            this.tabMain.Width = (base.Location.X + base.Width) - this.tabMain.Location.X;
            this.tabMain.Height = (base.Location.Y + base.Height) - this.tabMain.Location.Y;
        }

        private void menuItemAbout_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, ".NET Web Service Studio 2.0 \nIdeas and suggestions - Please mailto:sowmys@microsoft.com");
        }

        private void menuItemExit_Click(object sender, EventArgs e)
        {
            base.Close();
        }

        private void menuItemFind_Click(object sender, EventArgs e)
        {
            SearchDialog dialog = new SearchDialog();
            dialog.ShowDialog();
            if (dialog.DialogResult == DialogResult.OK)
            {
                this.tabMain.SelectedTab = this.tabPageWsdl;
                this.findOption = RichTextBoxFinds.None;
                if (dialog.MatchCase)
                {
                    this.findOption |= RichTextBoxFinds.MatchCase;
                }
                if (dialog.WholeWord)
                {
                    this.findOption |= RichTextBoxFinds.WholeWord;
                }
                this.searchStr = dialog.SearchStr;
                this.Find();
            }
        }

        private void menuItemFindNext_Click(object sender, EventArgs e)
        {
            if (this.tabMain.SelectedTab == this.tabPageInvoke)
            {
                MessageBox.Show(this, "'Find' cannot be used in the 'Invoke' tab");
            }
            else
            {
                this.Find();
            }
        }

        private void menuItemHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show(this, MiniHelpText);
        }

        private void menuItemOpen_Click(object sender, EventArgs e)
        {
            this.openWsdlDialog.ShowDialog();
            string fileName = this.openWsdlDialog.FileName;
            Cursor cursor = this.Cursor;
            this.Cursor = Cursors.WaitCursor;
            try
            {
                this.wsdl.Reset();
                this.wsdl.Paths.Add(fileName);
                this.wsdl.Generate();
                this.FillWsdlTab();
                this.FillInvokeTab();
            }
            finally
            {
                this.Cursor = cursor;
            }
        }

        private void menuItemOptions_Click(object sender, EventArgs e)
        {
            new OptionDialog().ShowDialog();
        }

        private void menuItemSaveAll_Click(object sender, EventArgs e)
        {
            if ((this.saveAllDialog.ShowDialog() == DialogResult.OK) && ((((this.wsdl.Wsdls != null) && (this.wsdl.Wsdls.Count != 0)) || ((this.wsdl.Xsds != null) && (this.wsdl.Xsds.Count != 0))) || (this.wsdl.ProxyCode != null)))
            {
                int length = this.saveAllDialog.FileName.LastIndexOf('.');
                string str = (length >= 0) ? this.saveAllDialog.FileName.Substring(0, length) : this.saveAllDialog.FileName;
                if (this.wsdl.Wsdls.Count == 1)
                {
                    this.SaveFile(str + ".wsdl", this.wsdl.Wsdls[0]);
                }
                else
                {
                    for (int i = 0; i < this.wsdl.Wsdls.Count; i++)
                    {
                        this.SaveFile(str + i.ToString() + ".wsdl", this.wsdl.Wsdls[i]);
                    }
                }
                if (this.wsdl.Xsds.Count == 1)
                {
                    this.SaveFile(str + ".xsd", this.wsdl.Xsds[0]);
                }
                else
                {
                    for (int j = 0; j < this.wsdl.Xsds.Count; j++)
                    {
                        this.SaveFile(str + j.ToString() + ".xsd", this.wsdl.Xsds[j]);
                    }
                }
                this.SaveFile(str + "." + this.wsdl.ProxyFileExtension, this.wsdl.ProxyCode);
                this.SaveFile(str + "Client." + this.wsdl.ProxyFileExtension, Script.GetUsingCode(this.wsdl.WsdlProperties.Language) + "\n" + this.GenerateClientCode() + "\n" + Script.GetDumpCode(this.wsdl.WsdlProperties.Language));
            }
        }

        public Assembly OnAssemblyResolve(object sender, ResolveEventArgs args)
        {
            Assembly proxyAssembly = this.wsdl.ProxyAssembly;
            if ((proxyAssembly != null) && (proxyAssembly.GetName().ToString() == args.Name))
            {
                return proxyAssembly;
            }
            return null;
        }

        private void PanelLeftRaw_SizeChanged(object sender, EventArgs e)
        {
            this.propRequest.SetBounds(0, 0, this.panelLeftRaw.Width, this.panelLeftRaw.Height, BoundsSpecified.Size);
        }

        private void PanelRightInvoke_SizeChanged(object sender, EventArgs e)
        {
            int width = (this.panelRightInvoke.Width - 0x18) / 2;
            int x = 8;
            int num3 = (8 + width) + 8;
            int height = (((this.panelRightInvoke.Height - 0x10) - 20) - 40) / 2;
            int y = 8;
            int num6 = (0x1c + height) + 20;
            this.labelInput.SetBounds(x, y, 0, 0, BoundsSpecified.Location);
            this.labelInputValue.SetBounds(num3, y, 0, 0, BoundsSpecified.Location);
            this.labelOutput.SetBounds(x, num6, 0, 0, BoundsSpecified.Location);
            this.labelOutputValue.SetBounds(num3, num6, 0, 0, BoundsSpecified.Location);
            y += 20;
            num6 += 20;
            this.treeInput.SetBounds(x, y, width, height, BoundsSpecified.All);
            this.treeOutput.SetBounds(x, num6, width, height, BoundsSpecified.All);
            this.propInput.SetBounds(num3, y, width, height, BoundsSpecified.All);
            this.propOutput.SetBounds(num3, num6, width, height, BoundsSpecified.All);
            this.buttonInvoke.SetBounds((num3 + width) - this.buttonInvoke.Width, ((this.panelRightInvoke.Height + 20) - this.buttonInvoke.Height) / 2, 0, 0, BoundsSpecified.Location);
        }

        private void PanelRightRaw_SizeChanged(object sender, EventArgs e)
        {
            int width = this.panelRightRaw.Width - 0x10;
            int x = 8;
            int height = (((this.panelRightRaw.Height - 0x10) - 20) - 40) / 2;
            int y = 8;
            int num5 = (0x1c + height) + 20;
            this.labelRequest.SetBounds(x, y, 0, 0, BoundsSpecified.Location);
            this.labelResponse.SetBounds(x, num5, 0, 0, BoundsSpecified.Location);
            y += 20;
            num5 += 20;
            this.richRequest.SetBounds(x, y, width, height, BoundsSpecified.All);
            this.richResponse.SetBounds(x, num5, width, height, BoundsSpecified.All);
            this.buttonSend.SetBounds((x + width) - this.buttonSend.Width, ((this.panelRightRaw.Height + 20) - this.buttonSend.Height) / 2, 0, 0, BoundsSpecified.Location);
        }

        private void propInput_PropertyValueChanged(object s, PropertyValueChangedEventArgs e)
        {
            TreeNodeProperty selectedObject = this.propInput.SelectedObject as TreeNodeProperty;
            if ((selectedObject != null) && ((e.ChangedItem.Label == "Type") && (e.OldValue.GetType() != selectedObject.Type)))
            {
                TreeNodeProperty property2 = TreeNodeProperty.CreateTreeNodeProperty(selectedObject);
                property2.TreeNode = selectedObject.TreeNode;
                property2.RecreateSubtree(null);
                this.treeInput.SelectedNode = property2.TreeNode;
            }
        }

        private bool SaveFile(string fileName, string contents)
        {
            if (System.IO.File.Exists(fileName) && (MessageBox.Show(this, "File " + fileName + " already exists. Overwrite?", "Warning", MessageBoxButtons.YesNo) != DialogResult.Yes))
            {
                return false;
            }
            FileStream stream = System.IO.File.OpenWrite(fileName);
            StreamWriter writer = new StreamWriter(stream);
            writer.Write(contents);
            writer.Flush();
            stream.SetLength(stream.Position);
            stream.Close();
            return true;
        }

        private void SendWebRequest()
        {
            Encoding encoding = new UTF8Encoding(true);
            RequestProperties selectedObject = this.propRequest.SelectedObject as RequestProperties;
            HttpWebRequest request = (HttpWebRequest) WebRequest.CreateDefault(new Uri(selectedObject.Url));
            if ((selectedObject.HttpProxy != null) && (selectedObject.HttpProxy.Length != 0))
            {
                request.Proxy = new WebProxy(selectedObject.HttpProxy);
            }
            request.Method = selectedObject.Method.ToString();
            request.ContentType = selectedObject.ContentType;
            request.Headers["SOAPAction"] = selectedObject.SOAPAction;
            request.SendChunked = selectedObject.SendChunked;
            request.AllowAutoRedirect = selectedObject.AllowAutoRedirect;
            request.AllowWriteStreamBuffering = selectedObject.AllowWriteStreamBuffering;
            request.KeepAlive = selectedObject.KeepAlive;
            request.Pipelined = selectedObject.Pipelined;
            request.PreAuthenticate = selectedObject.PreAuthenticate;
            request.Timeout = selectedObject.Timeout;
            HttpWebClientProtocol proxy = this.GetCurrentMethodProperty().GetProxyProperty().GetProxy();
            if (selectedObject.UseCookieContainer)
            {
                if (proxy.CookieContainer != null)
                {
                    request.CookieContainer = proxy.CookieContainer;
                }
                else
                {
                    request.CookieContainer = new CookieContainer();
                }
            }
            CredentialCache cache = new CredentialCache();
            bool flag = false;
            if ((selectedObject.BasicAuthUserName != null) && (selectedObject.BasicAuthUserName.Length != 0))
            {
                cache.Add(new Uri(selectedObject.Url), "Basic", new NetworkCredential(selectedObject.BasicAuthUserName, selectedObject.BasicAuthPassword));
                flag = true;
            }
            if (selectedObject.UseDefaultCredential)
            {
                cache.Add(new Uri(selectedObject.Url), "NTLM", (NetworkCredential) CredentialCache.DefaultCredentials);
                flag = true;
            }
            if (flag)
            {
                request.Credentials = cache;
            }
            if (selectedObject.Method == RequestProperties.HttpMethod.POST)
            {
                request.ContentLength = this.richRequest.Text.Length + encoding.GetPreamble().Length;
                StreamWriter writer = new StreamWriter(request.GetRequestStream(), encoding);
                writer.Write(this.richRequest.Text);
                writer.Close();
            }
            try
            {
                HttpWebResponse response = (HttpWebResponse) request.GetResponse();
                this.DumpResponse(response);
                response.Close();
            }
            catch (WebException exception)
            {
                if (exception.Response != null)
                {
                    this.DumpResponse((HttpWebResponse) exception.Response);
                }
                else
                {
                    this.richResponse.Text = exception.ToString();
                }
            }
            catch (Exception exception2)
            {
                this.richResponse.Text = exception2.ToString();
            }
        }

        internal void SetupAssemblyResolver()
        {
            ResolveEventHandler handler = new ResolveEventHandler(this.OnAssemblyResolve);
            AppDomain.CurrentDomain.AssemblyResolve += handler;
        }

        public static void ShowMessage(object sender, MessageType status, string message)
        {
            if (Program.mainForm != null)
            {
                Program.mainForm.ShowMessageInternal(sender, status, message);
            }
        }

        private void ShowMessageInternal(object sender, MessageType status, string message)
        {
            if (message == null)
            {
                message = status.ToString();
            }
            switch (status)
            {
                case MessageType.Begin:
                    this.richMessage.SelectionColor = Color.Blue;
                    this.richMessage.AppendText(message + "\n");
                    this.richMessage.Update();
                    break;

                case MessageType.Success:
                    this.richMessage.SelectionColor = Color.Green;
                    this.richMessage.AppendText(message + "\n");
                    this.richMessage.Update();
                    if (sender == this.wsdl)
                    {
                        base.BeginInvoke(new WsdlGenerationDoneCallback(this.WsdlGenerationDone), new object[] { true });
                    }
                    break;

                case MessageType.Failure:
                    this.richMessage.SelectionColor = Color.Red;
                    this.richMessage.AppendText(message + "\n");
                    this.richMessage.Update();
                    if (sender == this.wsdl)
                    {
                        base.BeginInvoke(new WsdlGenerationDoneCallback(this.WsdlGenerationDone), new object[] { false });
                    }
                    break;

                case MessageType.Warning:
                    this.richMessage.SelectionColor = Color.DarkRed;
                    this.richMessage.AppendText(message + "\n");
                    this.richMessage.Update();
                    break;

                case MessageType.Error:
                    this.richMessage.SelectionColor = Color.Red;
                    this.richMessage.AppendText(message + "\n");
                    this.richMessage.Update();
                    break;
            }
        }

        private void tabMain_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.tabMain.SelectedTab == this.tabPageRaw)
            {
                if (this.propRequest.SelectedObject == null)
                {
                    this.propRequest.SelectedObject = new RequestProperties(null);
                }
            }
            else if (((this.tabMain.SelectedTab == this.tabPageWsdl) && (this.treeWsdl.Nodes != null)) && (this.treeWsdl.Nodes.Count != 0))
            {
                TreeNode node = this.treeWsdl.Nodes[3];
                node.Tag = this.GenerateClientCode();
                if (this.treeWsdl.SelectedNode == node)
                {
                    this.richWsdl.Text = node.Tag.ToString();
                }
            }
        }

        private void textEndPointUri_KeyPress(object sender, KeyPressEventArgs e)
        {
            if ((e.KeyChar == '\r') || (e.KeyChar == '\n'))
            {
                this.buttonGet_Click(sender, null);
                e.Handled = true;
            }
            else if (!char.IsControl(e.KeyChar))
            {
                if (!isV1)
                {
                    this.textEndPointUri.SelectedText = e.KeyChar.ToString();
                }
                e.Handled = true;
                string text = this.textEndPointUri.Text;
                if ((text != null) && (text.Length != 0))
                {
                    for (int i = 0; i < this.textEndPointUri.Items.Count; i++)
                    {
                        if (((string) this.textEndPointUri.Items[i]).StartsWith(text))
                        {
                            this.textEndPointUri.SelectedIndex = i;
                            this.textEndPointUri.Select(text.Length, this.textEndPointUri.Text.Length);
                            break;
                        }
                    }
                }
            }
        }

        private void treeInput_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.propInput.SelectedObject = e.Node.Tag;
            this.menuItemTreeInputCopy.Enabled = this.IsValidCopyNode(e.Node.Tag as TreeNodeProperty);
            this.menuItemTreeInputPaste.Enabled = this.IsValidPasteNode(e.Node.Tag as TreeNodeProperty);
        }

        private void treeInputMenuCopy_Click(object sender, EventArgs e)
        {
            this.CopyToClipboard(this.treeInput.SelectedNode.Tag as TreeNodeProperty);
        }

        private void treeInputMenuPaste_Click(object sender, EventArgs e)
        {
            TreeNodeProperty tag = this.treeInput.SelectedNode.Tag as TreeNodeProperty;
            if (tag is MethodProperty)
            {
                throw new Exception("Paste not valid on method");
            }
            System.Type[] typeList = tag.GetTypeList();
            System.Type type = typeof(DataSet).IsAssignableFrom(typeList[0]) ? typeof(DataSet) : typeof(object);
            XmlSerializer serializer = new XmlSerializer(type, typeList);
            StringReader textReader = new StringReader((string) Clipboard.GetDataObject().GetData(DataFormats.Text));
            object val = serializer.Deserialize(textReader);
            if ((val == null) || !typeList[0].IsAssignableFrom(val.GetType()))
            {
                throw new Exception("Invalid Type pasted");
            }
            TreeNodeProperty property2 = TreeNodeProperty.CreateTreeNodeProperty(tag, val);
            property2.TreeNode = tag.TreeNode;
            property2.RecreateSubtree(null);
            this.treeInput.SelectedNode = property2.TreeNode;
        }

        private void treeMethods_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is MethodInfo)
            {
                MethodInfo tag = e.Node.Tag as MethodInfo;
                this.treeInput.Nodes.Clear();
                MethodProperty property = new MethodProperty(this.GetProxyPropertyFromNode(e.Node), tag);
                property.RecreateSubtree(null);
                this.treeInput.Nodes.Add(property.TreeNode);
                e.Node.Tag = property.TreeNode;
            }
            else if (e.Node.Tag is TreeNode)
            {
                this.treeInput.Nodes.Clear();
                this.treeInput.Nodes.Add((TreeNode) e.Node.Tag);
            }
            this.treeInput.ExpandAll();
            this.treeInput.SelectedNode = this.treeInput.Nodes[0];
        }

        private void treeOutput_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.propOutput.SelectedObject = e.Node.Tag;
            this.menuItemTreeOutputCopy.Enabled = this.IsValidCopyNode(e.Node.Tag as TreeNodeProperty);
        }

        private void treeOutputMenuCopy_Click(object sender, EventArgs e)
        {
            this.CopyToClipboard(this.treeOutput.SelectedNode.Tag as TreeNodeProperty);
        }

        private void treeWsdl_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if ((e.Node.Tag != null) && (this.richWsdl.Tag != e.Node.Tag))
            {
                this.richWsdl.Text = e.Node.Tag.ToString();
                this.richWsdl.Tag = e.Node.Tag;
            }
            XmlTreeNode node = e.Node as XmlTreeNode;
            if (node != null)
            {
                this.richWsdl.Select(node.StartPosition, node.EndPosition - node.StartPosition);
            }
        }

        private void WsdlGenerationDone(bool genDone)
        {
            this.buttonGet.Text = "Get";
            this.FillWsdlTab();
            if (genDone)
            {
                this.ShowMessageInternal(this, MessageType.Begin, "Reflecting Proxy Assembly");
                this.FillInvokeTab();
                this.tabMain.SelectedTab = this.tabPageInvoke;
                this.ShowMessageInternal(this, MessageType.Success, "Ready To Invoke");
                Configuration.MasterConfig.InvokeSettings.AddUri(this.textEndPointUri.Text);
                this.textEndPointUri.Items.Clear();
                this.textEndPointUri.Items.AddRange(Configuration.MasterConfig.InvokeSettings.RecentlyUsedUris);
            }
        }

        private delegate void WsdlGenerationDoneCallback(bool genDone);
    }
}

