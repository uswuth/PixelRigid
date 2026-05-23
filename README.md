![PixelRigid](./PixelRigid.png)

> Force square/sharp window corners on Windows 11 — system-wide, silent, zero CPU.

Windows 11 rounds every window corner by default. Pixel Rigid removes that globally, instantly, with no polling loop.

---

## Features

- **Event-driven** — uses `WinEventHook`, not a polling loop. CPU usage is literally 0% when idle
- **System windows too** — Task Manager, Settings, and other elevated windows are covered (runs as admin)
- **System tray** — lives quietly in your tray; left-click to pause/resume, right-click for menu
- **Run at startup** — one checkbox in the tray menu writes to your registry run key
- **No runtime needed** — targets .NET 4.8 which ships with every Windows 10/11 install
- **Tiny** — single `.exe`, ~50 KB

---

## Download

Grab the latest `Pixel Rigid.exe` from [Releases](../../releases).

---

## Build from source

Requirements: [.NET SDK 6+](https://dotnet.microsoft.com/download) (only needed to build; the output runs on .NET 4.8)

```bash
git clone https://github.com/uswuth/PixelRigid
cd PixelRigid
dotnet publish -c Release
```

Output: `bin\Release\net48\PixelRigid.exe`

---

## Usage

1. Run `Pixel Rigid.exe` — UAC will prompt once for admin rights (needed for system windows)
2. The app hides to your system tray immediately
3. **Left-click** the tray icon to pause/resume
4. **Right-click** for options including _Run at startup_ and _Exit_

---

## How it works

Windows 11 exposes a DWM (Desktop Window Manager) API — `DwmSetWindowAttribute` with `DWMWA_WINDOW_CORNER_PREFERENCE` — that controls per-window corner rounding. Pixel Rigid installs two `WinEventHook` listeners:

| Event                     | When                   |
| ------------------------- | ---------------------- |
| `EVENT_OBJECT_SHOW`       | A window first appears |
| `EVENT_SYSTEM_FOREGROUND` | A window gains focus   |

On each event it calls `DwmSetWindowAttribute(hwnd, 33, DWMWCP_DONOTROUND, 4)`.
The hook thread blocks in a Win32 `GetMessage` loop — zero CPU when nothing is happening.

---

## Why admin?

Task Manager, UAC dialogs, and other system processes run at a higher integrity level. Without admin rights, `DwmSetWindowAttribute` silently fails on them. Pixel Rigid requests `requireAdministrator` in its manifest and enables `SeDebugPrivilege` at startup to reach all windows.

---

## Credits

- Built for Windows 11 customization
- Uses standard Windows APIs provided by Microsoft
