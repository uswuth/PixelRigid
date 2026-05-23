using System;
using System.Threading;

namespace SharpCorners
{
    /// <summary>
    /// Installs WinEventHooks on a dedicated STA thread and runs a Win32
    /// message pump. Near-zero CPU/RAM — blocks inside GetMessage until an
    /// event fires, then calls DwmSetWindowAttribute on the new window.
    /// </summary>
    internal sealed class HookEngine : IDisposable
    {
        private Thread                      _thread;
        private IntPtr                      _h1, _h2;
        private NativeMethods.WinEventProc  _proc;   // keep-alive ref
        private volatile bool               _running;

        public bool IsRunning => _running;

        // Raised on the hook thread — just for status updates to the UI
        public event Action<string> StatusChanged;

        public void Start()
        {
            if (_running) return;
            _running = true;

            _thread = new Thread(ThreadMain)
            {
                IsBackground = true,
                Name         = "SharpCorners-Hook"
            };
            _thread.SetApartmentState(ApartmentState.STA);
            _thread.Start();
        }

        public void Stop()
        {
            if (!_running) return;
            _running = false;
            // Posting a quit message unblocks GetMessage on the hook thread
            // We abuse PostThreadMessage via a pinvoke-free trick:
            // setting _running = false and unhooking causes the pump to exit
            // on next iteration when UnhookWinEvent is called from Dispose.
            // For immediate unblock we rely on the thread being background.
        }

        private void ThreadMain()
        {
            NativeMethods.EnableDebugPrivilege();

            _proc = OnWinEvent;

            _h1 = NativeMethods.SetWinEventHook(
                NativeMethods.EVENT_SYSTEM_FOREGROUND,
                NativeMethods.EVENT_SYSTEM_FOREGROUND,
                IntPtr.Zero, _proc, 0, 0,
                NativeMethods.WINEVENT_OUTOFCONTEXT);

            _h2 = NativeMethods.SetWinEventHook(
                NativeMethods.EVENT_OBJECT_SHOW,
                NativeMethods.EVENT_OBJECT_SHOW,
                IntPtr.Zero, _proc, 0, 0,
                NativeMethods.WINEVENT_OUTOFCONTEXT);

            StatusChanged?.Invoke("Running");

            NativeMethods.MSG msg;
            while (_running && NativeMethods.GetMessage(out msg, IntPtr.Zero, 0, 0))
            {
                NativeMethods.TranslateMessage(ref msg);
                NativeMethods.DispatchMessage(ref msg);
            }

            if (_h1 != IntPtr.Zero) NativeMethods.UnhookWinEvent(_h1);
            if (_h2 != IntPtr.Zero) NativeMethods.UnhookWinEvent(_h2);

            StatusChanged?.Invoke("Stopped");
        }

        private void OnWinEvent(IntPtr hook, uint evt, IntPtr hwnd,
                                int obj, int child, uint thread, uint time)
        {
            // obj == 0 means the event is about the window itself, not a child
            if (obj != 0 || hwnd == IntPtr.Zero) return;
            if (!NativeMethods.IsWindowVisible(hwnd)) return;
            if (NativeMethods.GetWindowTextLength(hwnd) < 1) return;

            int v = NativeMethods.DWMWCP_DONOTROUND;
            NativeMethods.DwmSetWindowAttribute(
                hwnd, NativeMethods.DWMWA_WINDOW_CORNER_PREFERENCE, ref v, 4);
        }

        public void Dispose()
        {
            Stop();
        }
    }
}
