using System;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;

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
        private const string RegRunKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Run";

        public TrayApp()
        {
            _engine = new HookEngine();
            _engine.StatusChanged += s => UpdateTrayText();

            // ── Context menu ─────────────────────────────────────────────────
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
                    "github.com/yourusername/PixelRigid",
                    "PixelRigid", MessageBoxButtons.OK, MessageBoxIcon.Information));

            var exitItem = new ToolStripMenuItem("Exit", null, OnExit);

            var menu = new ContextMenuStrip();
            menu.Items.Add(_toggleItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(_autostartItem);
            menu.Items.Add(aboutItem);
            menu.Items.Add(new ToolStripSeparator());
            menu.Items.Add(exitItem);

            // ── Tray icon ────────────────────────────────────────────────────
            _tray = new NotifyIcon
            {
                Text             = "PixelRigid — Active",
                ContextMenuStrip = menu,
                Visible          = true
            };

            // Use the exe's own icon (set via <ApplicationIcon> in .csproj)
            _tray.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);

            _tray.MouseClick += (s, e) =>
            {
                if (e.Button == MouseButtons.Left) OnToggle(s, e);
            };

            _engine.Start();
        }

        // ── Toggle on/off ─────────────────────────────────────────────────────
        private void OnToggle(object s, EventArgs e)
        {
            _active = !_active;
            if (_active) _engine.Start();
            else         _engine.Stop();
            UpdateTrayText();
        }

        // ── Update tray tooltip + menu label ──────────────────────────────────
        private void UpdateTrayText()
        {
            if (_tray == null) return;
            _tray.Text         = _active ? "PixelRigid — Active" : "PixelRigid — Paused";
            _toggleItem.Text   = _active ? "● Active (click to pause)" : "○ Paused (click to resume)";
        }

        // ── Autostart via registry ────────────────────────────────────────────
        private void OnAutostart(object s, EventArgs e)
        {
            _autostartItem.Checked = !_autostartItem.Checked;
            using (var key = Registry.CurrentUser.OpenSubKey(RegRunKey, true))
            {
                if (_autostartItem.Checked)
                    key.SetValue(AppName, $"\"{Application.ExecutablePath}\"");
                else
                    key.DeleteValue(AppName, false);
            }
        }

        private bool IsAutostartEnabled()
        {
            using (var key = Registry.CurrentUser.OpenSubKey(RegRunKey, false))
                return key?.GetValue(AppName) != null;
        }

        // ── Exit ──────────────────────────────────────────────────────────────
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