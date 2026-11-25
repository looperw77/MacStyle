# MacStyle

A dock, a top island, a control center and a launcher for Windows 10/11, in one small exe. The interface is in Arabic.

- **Island**: hidden over apps. Come close and a white line shows, hold the mouse at the top edge for the clock, click it for the media player (seek, ±10 s, per-app volume, shuffle/repeat, source switching).
- **Notifications** in the island, with a switch per app, duration, size and color.
- **Dock** that appears when the mouse is held at the bottom edge, with magnification and the genie minimize animation.
- **Control center** (Win + A): Wi-Fi, Bluetooth, brightness, volume per app.
- **Launcher** and search on the Windows key.
- **Paste a screenshot** into a folder or the desktop with Ctrl + V.

## Download

Releases are on the Releases page. `MacStyle-Update-v2.exe` updates an existing installation and keeps its settings.

## What it changes on your system

- Hides the Windows taskbar while the dock is on, and restores it on exit.
- Starts with Windows (a scheduled task, or the `Run` registry key).
- Handles the Windows key, Win + A and the volume keys while the matching options are on.
- Turns off the Windows minimize animation while the genie animation is on.
- Reads Windows notifications while island notifications are on.
- If Seelen UI is installed, turns off its dock. If Files is installed, applies the accent color to it.
- Stores settings in `%APPDATA%\MacStyle`.

Every part has a switch in the settings. **Uninstall** from Windows Settings > Apps, or from the bottom of the MacStyle settings window.

## Privacy

This program will not transfer any information to other networked systems unless specifically requested by the user or the person installing or operating it. Notifications are shown on screen only and never stored. Choosing a web search in the launcher opens your browser.

## Code signing policy

Free code signing provided by [SignPath.io](https://about.signpath.io/), certificate by [SignPath Foundation](https://signpath.org/). *(Applied for, not active yet: current releases are unsigned.)*

- Committers and reviewers: Ahmed Kamel
- Approvers: Ahmed Kamel

## Build

Needs Visual Studio Build Tools (C# compiler) and .NET Framework 4.8.

```powershell
.\build.ps1
.\tests\run.ps1
```

## License

[MIT](LICENSE)
