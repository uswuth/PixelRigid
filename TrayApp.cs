using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Security.Principal;

namespace SharpCorners
{
    internal sealed class TrayApp : ApplicationContext
    {
        private readonly NotifyIcon        _tray;
        private readonly HookEngine        _engine;
        private readonly ToolStripMenuItem _toggleItem;
        private readonly ToolStripMenuItem _autostartItem;
        private bool _active = true;

        private const string AppName   = "PixelRigid";
        private const string TaskName  = "PixelRigid_Autostart";

        public TrayApp()
        {
            _engine = new HookEngine();
            _engine.StatusChanged += s => UpdateTrayText();

            _toggleItem = new ToolStripMenuItem("● Active (click to pause)", null, OnToggle)
            {
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            _autostartItem = new ToolStripMenuItem("Run at startup", null, OnAutostart);
            _autostartItem.Checked = IsAutostartEnabled();

            var aboutItem = new ToolStripMenuItem("About", null, (s, e) =>
                MessageBox.Show(
                    "PixelRigid v1.0\n\n" +
                    "Forces square/sharp window corners on Windows 11.\n" +
                    "Event-driven — zero CPU usage when idle.\n\n" +
                    "github.com/uswuth/PixelRigid",
                    "PixelRigid", MessageBoxButtons.OK, MessageBoxIcon.Information));

            var exitItem = new ToolStripMenuItem("Exit", null, OnExit);

            var menu = new ContextMenuStrip();
            menu.Items.Add(_toggleItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(_autostartItem);
            menu.Items.Add(aboutItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitItem);

            _tray = new NotifyIcon
            {
                Text             = "PixelRigid — Active",
                ContextMenuStrip = menu,
                Visible          = true
            };

            _tray.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _tray.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left) OnToggle(s, e);
            };

            _engine.Start();
        }

        private void OnToggle(object s, EventArgs e)
        {
            _active = !_active;
            if (_active) _engine.Start();
            else         _engine.Stop();
            UpdateTrayText();
        }

        private void UpdateTrayText()
        {
            if (_tray == null) return;
            _tray.Text       = _active ? "PixelRigid — Active" : "PixelRigid — Paused";
            _toggleItem.Text = _active ? "● Active (click to pause)" : "○ Paused (click to resume)";
        }

        // ── Autostart via Task Scheduler (works with requireAdministrator) ────
        private void OnAutostart(object s, EventArgs e)
        {
            _autostartItem.Checked = !_autostartItem.Checked;

            if (_autostartItem.Checked)
                RegisterScheduledTask();
            else
                RemoveScheduledTask();
        }

        private void RegisterScheduledTask()
        {
            // schtasks creates an elevated task that runs at login — no UAC popup
            string exe = Application.ExecutablePath;
            string xml = $@"<?xml version=""1.0"" encoding=""UTF-16""?>
<Task version=""1.2"" xmlns=""http://schemas.microsoft.com/windows/2004/02/mit/task"">
  <Triggers>
    <LogonTrigger>
      <Enabled>true</Enabled>
    </LogonTrigger>
  </Triggers>
  <Principals>
    <Principal id=""Author"">
      <LogonType>InteractiveToken</LogonType>
      <RunLevel>HighestAvailable</RunLevel>
    </Principal>
  </Principals>
  <Settings>
    <MultipleInstancesPolicy>IgnoreNew</MultipleInstancesPolicy>
    <DisallowStartIfOnBatteries>false</DisallowStartIfOnBatteries>
    <StopIfGoingOnBatteries>false</StopIfGoingOnBatteries>
    <ExecutionTimeLimit>PT0S</ExecutionTimeLimit>
    <Priority>7</Priority>
  </Settings>
  <Actions>
    <Exec>
      <Command>{exe}</Command>
    </Exec>
  </Actions>
</Task>";

            // Write XML to temp file and import via schtasks
            string tmp = System.IO.Path.GetTempFileName() + ".xml";
            System.IO.File.WriteAllText(tmp, xml, System.Text.Encoding.Unicode);

            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName        = "schtasks.exe",
                Arguments       = $"/Create /TN \"{TaskName}\" /XML \"{tmp}\" /F",
                UseShellExecute = false,
                CreateNoWindow  = true
            };

            using (var p = System.Diagnostics.Process.Start(psi))
                p.WaitForExit();

            System.IO.File.Delete(tmp);
        }

        private void RemoveScheduledTask()
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName        = "schtasks.exe",
                Arguments       = $"/Delete /TN \"{TaskName}\" /F",
                UseShellExecute = false,
                CreateNoWindow  = true
            };

            using (var p = System.Diagnostics.Process.Start(psi))
                p.WaitForExit();
        }

        private bool IsAutostartEnabled()
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName               = "schtasks.exe",
                Arguments              = $"/Query /TN \"{TaskName}\"",
                UseShellExecute        = false,
                CreateNoWindow         = true,
                RedirectStandardOutput = true
            };

            try
            {
                using (var p = System.Diagnostics.Process.Start(psi))
                {
                    p.WaitForExit();
                    return p.ExitCode == 0;
                }
            }
            catch { return false; }
        }

        private void OnExit(object s, EventArgs e)
        {
            _engine.Dispose();
            _tray.Visible = false;
            Application.Exit();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _engine?.Dispose();
                _tray?.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}