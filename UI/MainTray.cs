using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using PixelRigid.Config;
using PixelRigid.Utils;
using SharpCorners;

namespace PixelRigid.UI
{
    internal sealed class MainTray : ApplicationContext
    {
        private readonly NotifyIcon _tray;
        private readonly HookEngine _engine;
        private readonly ToolStripMenuItem _toggleItem;
        private readonly ToolStripMenuItem _autostartItem;
        private readonly ToolStripMenuItem _elevateItem;
        private readonly ToolStripMenuItem _removeAdminItem;
        private readonly ToolStripMenuItem _advancedItem;
        private bool _active = true;
        private bool _isAdmin = false;

        public MainTray()
        {
            _isAdmin = Elevation.ElevationManager.IsElevated();
            _engine = new HookEngine();
            _engine.StatusChanged += s => UpdateTrayText();

            _toggleItem = new ToolStripMenuItem("Active (click to pause)", null, OnToggle)
            {
                Font = new Font("Segoe UI", 9f, FontStyle.Bold)
            };

            _autostartItem = new ToolStripMenuItem("Run at startup", null, OnAutostart);
            _autostartItem.Checked = ProcessHelper.IsScheduledTaskRegistered();

            _elevateItem = new ToolStripMenuItem("Request Administrator Access", null, OnElevate);
            _removeAdminItem = new ToolStripMenuItem("Remove Administrator Access", null, OnRemoveAdmin);

            var uninstallItem = new ToolStripMenuItem("Uninstall", null, OnUninstall);

            _advancedItem = new ToolStripMenuItem("Advanced");
            UpdateElevateItem(uninstallItem);
            _advancedItem.DropDownOpening += (s, e) => UpdateElevateItem(uninstallItem);

            var aboutItem = new ToolStripMenuItem("About", null, (s, e) =>
                MessageBox.Show(
                    "PixelRigid v2.0\n\n" +
                    "Forces square/sharp window corners on Windows 11.\n" +
                    "Event-driven — zero CPU usage when idle.\n\n" +
                    "github.com/uswuth/PixelRigid",
                    "PixelRigid", MessageBoxButtons.OK, MessageBoxIcon.Information));

            var exitItem = new ToolStripMenuItem("Exit", null, OnExit);

            var menu = new ContextMenuStrip();
            menu.Items.Add(_toggleItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(_autostartItem);
            menu.Items.Add(_advancedItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(aboutItem);
            menu.Items.Add(exitItem);

            _tray = new NotifyIcon
            {
                ContextMenuStrip = menu,
                Visible = true
            };

            _tray.Icon = ThemeDetector.GetTrayIcon();

            _tray.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left) OnToggle(s, e);
            };

            UpdateTrayText();
            _engine.Start();
        }

        private void UpdateElevateItem(ToolStripMenuItem uninstallItem)
        {
            _advancedItem.DropDownItems.Clear();
            if (_isAdmin)
            {
                _elevateItem.Text = "Administrator Access Active";
                _elevateItem.Enabled = false;
                _advancedItem.DropDownItems.Add(_elevateItem);
                _advancedItem.DropDownItems.Add(_removeAdminItem);
                _advancedItem.DropDownItems.Add(new ToolStripSeparator());
                _advancedItem.DropDownItems.Add(uninstallItem);
            }
            else
            {
                _elevateItem.Text = "Request Administrator Access";
                _elevateItem.Enabled = true;
                _advancedItem.DropDownItems.Add(_elevateItem);
                _advancedItem.DropDownItems.Add(new ToolStripSeparator());
                _advancedItem.DropDownItems.Add(uninstallItem);
            }
        }

        private void OnToggle(object s, EventArgs e)
        {
            _active = !_active;
            if (_active) _engine.Start();
            else _engine.Stop();
            UpdateTrayText();
        }

        private void UpdateTrayText()
        {
            if (_tray == null) return;
            string mode = _isAdmin ? "Admin" : "User";
            _tray.Text = _active ? $"PixelRigid — Active ({mode})" : $"PixelRigid — Paused ({mode})";
            _toggleItem.Text = $"{(char)(_active ? '●' : '○')} {(_active ? "Active" : "Paused")} (click to {(_active ? "pause" : "resume")})";
        }

        private void OnAutostart(object s, EventArgs e)
        {
            _autostartItem.Checked = !_autostartItem.Checked;

            if (_autostartItem.Checked)
                ProcessHelper.RegisterScheduledTask(Application.ExecutablePath);
            else
                ProcessHelper.RemoveScheduledTask();
        }

        private void OnElevate(object s, EventArgs e)
        {
            Elevation.ElevationManager.RequestElevationIfNeeded();
        }

        private void OnRemoveAdmin(object s, EventArgs e)
        {
            _isAdmin = false;
            var uninstallItem = _advancedItem.DropDownItems[_advancedItem.DropDownItems.Count - 1] as ToolStripMenuItem;
            UpdateElevateItem(uninstallItem);
            UpdateTrayText();
            _tray.BalloonTipTitle = "PixelRigid";
            _tray.BalloonTipText = "Administrator access removed.";
            _tray.BalloonTipIcon = ToolTipIcon.Info;
            _tray.ShowBalloonTip(3000);
        }

        private void OnUninstall(object s, EventArgs e)
        {
            var result = MessageBox.Show(
                "Remove PixelRigid and all its files?\n\n" +
                "This cannot be undone.",
                "Uninstall PixelRigid",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (result == DialogResult.Yes)
            {
                try
                {
                    ProcessHelper.RemoveScheduledTask();

                    string configDir = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "PixelRigid"
                    );
                    if (Directory.Exists(configDir))
                        Directory.Delete(configDir, true);

                    _engine.Dispose();
                    _tray.Visible = false;

                    MessageBox.Show(
                        "PixelRigid has been removed.\n\n" +
                        "You can now delete the installation folder.",
                        "Uninstall Complete",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);

                    Application.Exit();
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error: {ex.Message}", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
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