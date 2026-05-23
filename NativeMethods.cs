using System;
using System.Runtime.InteropServices;

namespace SharpCorners
{
    internal static class NativeMethods
    {
        // ── DWM ──────────────────────────────────────────────────────────────
        public const int  DWMWA_WINDOW_CORNER_PREFERENCE = 33;
        public const int  DWMWCP_DEFAULT    = 0;
        public const int  DWMWCP_DONOTROUND = 1;

        [DllImport("dwmapi.dll")]
        public static extern int DwmSetWindowAttribute(
            IntPtr hwnd, int attr, ref int value, int size);

        // ── Window state ──────────────────────────────────────────────────────
        [DllImport("user32.dll")]
        public static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        public static extern int GetWindowTextLength(IntPtr hWnd);

        // ── WinEvent hook ─────────────────────────────────────────────────────
        public const uint WINEVENT_OUTOFCONTEXT   = 0x0000;
        public const uint EVENT_SYSTEM_FOREGROUND = 0x0003;
        public const uint EVENT_OBJECT_SHOW       = 0x8002;

        public delegate void WinEventProc(
            IntPtr hook, uint evt, IntPtr hwnd,
            int obj, int child, uint thread, uint time);

        [DllImport("user32.dll")]
        public static extern IntPtr SetWinEventHook(
            uint evMin, uint evMax, IntPtr hMod,
            WinEventProc proc, uint pid, uint tid, uint flags);

        [DllImport("user32.dll")]
        public static extern bool UnhookWinEvent(IntPtr hook);

        // ── Message pump ──────────────────────────────────────────────────────
        [StructLayout(LayoutKind.Sequential)]
        public struct MSG
        {
            public IntPtr  hwnd;
            public uint    message;
            public UIntPtr wParam;
            public IntPtr  lParam;
            public uint    time;
            public int     ptX, ptY;
        }

        [DllImport("user32.dll")]
        public static extern bool GetMessage(
            out MSG msg, IntPtr hWnd, uint min, uint max);

        [DllImport("user32.dll")]
        public static extern bool TranslateMessage(ref MSG msg);

        [DllImport("user32.dll")]
        public static extern IntPtr DispatchMessage(ref MSG msg);

        // ── SeDebugPrivilege (needed for system windows like Task Manager) ────
        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool OpenProcessToken(
            IntPtr process, uint access, out IntPtr token);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool LookupPrivilegeValue(
            string host, string name, out LUID luid);

        [DllImport("advapi32.dll", SetLastError = true)]
        private static extern bool AdjustTokenPrivileges(
            IntPtr token, bool disable, ref TOKEN_PRIVILEGES priv,
            uint len, IntPtr prev, IntPtr prevLen);

        [DllImport("kernel32.dll")]
        private static extern IntPtr GetCurrentProcess();

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr h);

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID { public uint Low; public int High; }

        [StructLayout(LayoutKind.Sequential)]
        private struct LUID_AND_ATTRIBUTES { public LUID Luid; public uint Attr; }

        [StructLayout(LayoutKind.Sequential)]
        private struct TOKEN_PRIVILEGES
        {
            public uint Count;
            public LUID_AND_ATTRIBUTES Privilege;
        }

        public static void EnableDebugPrivilege()
        {
            IntPtr token;
            if (!OpenProcessToken(GetCurrentProcess(), 0x0028, out token)) return;
            LUID luid;
            if (!LookupPrivilegeValue(null, "SeDebugPrivilege", out luid))
            { CloseHandle(token); return; }

            var tp = new TOKEN_PRIVILEGES
            {
                Count = 1,
                Privilege = new LUID_AND_ATTRIBUTES { Luid = luid, Attr = 0x0002 }
            };
            AdjustTokenPrivileges(token, false, ref tp, 0, IntPtr.Zero, IntPtr.Zero);
            CloseHandle(token);
        }
    }
}
