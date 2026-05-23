using System;
using System.Windows.Forms;

namespace SharpCorners
{
    static class Program
    {
        [STAThread]
        static void Main()
        {
            // Single instance guard
            bool created;
            var mutex = new System.Threading.Mutex(true, "PixelRigid_SingleInstance", out created);
            if (!created)
            {
                MessageBox.Show("PixelRigid is already running.\nCheck your system tray.",
                    "PixelRigid", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new TrayApp());

            GC.KeepAlive(mutex);
        }
    }
}
