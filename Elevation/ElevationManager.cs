using System;
using System.Diagnostics;
using System.Security.Principal;
using System.Windows.Forms;

namespace PixelRigid.Elevation
{
    internal static class ElevationManager
    {
        public static bool IsElevated()
        {
            using (var identity = WindowsIdentity.GetCurrent())
            {
                var principal = new WindowsPrincipal(identity);
                return principal.IsInRole(WindowsBuiltInRole.Administrator);
            }
        }

        public static bool RequestElevationIfNeeded()
        {
            if (IsElevated())
                return true;

            var result = MessageBox.Show(
                "This operation requires administrator access.\n\n" +
                "Click OK to grant access (UAC prompt will appear).\n" +
                "The application will restart with elevated privileges.",
                "Administrator Access Required",
                MessageBoxButtons.OKCancel,
                MessageBoxIcon.Question);

            if (result == DialogResult.OK)
            {
                return RequestElevation();
            }

            return false;
        }

        private static bool RequestElevation()
        {
            var psi = new ProcessStartInfo
            {
                FileName = Application.ExecutablePath,
                UseShellExecute = true,
                Verb = "runas"
            };

            try
            {
                Process.Start(psi);
                Application.Exit();
                return true;
            }
            catch
            {
                MessageBox.Show(
                    "Could not request administrator access.\n" +
                    "Operation cancelled.",
                    "Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
                return false;
            }
        }
    }
}