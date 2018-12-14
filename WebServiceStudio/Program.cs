using System;
using System.Windows.Forms;

namespace WebServiceStudio
{
    static class Program
    {
        public static bool isV1 { get; private set; }
        public static MainForm mainForm { get; private set; }

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            Version version = typeof(string).Assembly.GetName().Version;
            isV1 = ((version.Major == 1) && (version.Minor == 0)) && (version.Build == 0xce4);
            mainForm = new MainForm();
            WSSWebRequestCreate.RegisterPrefixes();
            try
            {
                mainForm.SetupAssemblyResolver();
            }
            catch (Exception exception)
            {
                MessageBox.Show(null, exception.ToString(), "Error Setting up Assembly Resolver");
            }
            Application.Run(mainForm);
        }
    }
}
