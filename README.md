<p align="center">
  <img src="./PixelRigid.png" alt="PixelRigid" width="160" />
</p>

<p align="center">
  Force sharp window corners on Windows 11.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/platform-Windows%2011-0078D6?style=flat-square" />
  <img src="https://img.shields.io/badge/.NET-4.8-512BD4?style=flat-square" />
  <img src="https://img.shields.io/badge/CPU-0%25%20Idle-black?style=flat-square" />
  <img src="https://img.shields.io/github/stars/uswuth/PixelRigid?style=flat-square" />
</p>

---

## Preview

<p align="center">
  <img src="./preview-1.png" width="32%" />
  <img src="./preview-2.png" width="32%" />
  <img src="./preview-3.png" width="32%" />
</p>

---

## Overview

Windows 11 rounds window corners by default.

PixelRigid removes them system-wide using native Windows DWM APIs and Win32 event hooks.

No polling loop.  
No background CPU usage.  
Runs silently in the tray.

---

## Features

- Sharp corners system-wide
- Event-driven architecture
- 0% CPU usage while idle
- Works on elevated/system windows
- Tray controls
- Startup toggle
- Single lightweight executable

---

## How It Works

PixelRigid listens for newly created or focused windows using `WinEventHook`.

When detected, it applies:

```cpp
DwmSetWindowAttribute(
    hwnd,
    DWMWA_WINDOW_CORNER_PREFERENCE,
    DWMWCP_DONOTROUND
);
```

This is handled entirely through native Windows APIs.

---

## Installation

Download the latest build from:

[Releases](../../releases)

---

## Build

```bash
git clone https://github.com/uswuth/PixelRigid.git
cd PixelRigid
dotnet publish -c Release
```

Output:

```txt
bin\Release\net48\PixelRigid.exe
```

---

## Usage

- Launch `PixelRigid.exe`
- Accept the UAC prompt
- App minimizes to the tray automatically

### Tray Controls

| Action         | Result         |
| -------------- | -------------- |
| Left Click     | Pause / Resume |
| Right Click    | Menu           |
| Startup Toggle | Run at startup |
| Exit           | Close app      |

---

## Third-Party Notice

PixelRigid uses standard Windows APIs provided by Microsoft Windows.

Microsoft and Windows are trademarks of Microsoft Corporation.
