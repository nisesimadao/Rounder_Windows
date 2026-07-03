# Rounder for Windows

Rounder for Windows is a tray utility that draws modern rounded-corner cutouts on selected displays.

This Windows port tracks the macOS Rounder v2.1.4 feature set where the platform allows it: immediate overlay updates, selectable monitors, presets, rounded/squircle/polygon cutouts, login startup, and the Super Duper Gaming Mode with animated rainbow edge glow.

![Rounder icon](Assets/rounder.png)

[Japanese README](./README_jp.md)

## Features

- Runs quietly from the Windows notification area.
- Left-click or double-click the tray icon to open settings.
- Toggle the rounded-corner effect from the tray menu.
- Apply changes immediately without restarting the app.
- Select target monitors. Newly connected monitors are covered automatically.
- Choose rounded, squircle, or polygon cutout shapes.
- Adjust radius, color, visible corners, gaming speed, glow intensity, and bloom width.
- Save, edit, import, and export presets.
- Launch at login using the current user's Windows Run registry key.
- Keeps overlay windows above the taskbar by reasserting topmost z-order.
- Per-monitor DPI aware for mixed-scale environments.

## Requirements

- Windows 10 or Windows 11
- .NET 9.0 Desktop Runtime
- .NET 9.0 SDK if building from source

## Build

```powershell
dotnet build .\Rounder_Windows.csproj -c Release
```

## Single-file Release Build

```powershell
dotnet publish .\Rounder_Windows.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:DebugType=None -p:DebugSymbols=false -o .\artifacts\release\Rounder_Windows-win-x64-singlefile
```

The release executable is:

```text
artifacts\release\Rounder_Windows-win-x64-singlefile\Rounder_Windows.exe
```

## Installer Build

Install [Inno Setup 6](https://jrsoftware.org/isinfo.php), then run:

```powershell
& "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe" `
  "/DMyAppVersion=2.1.4" `
  "/DPublishDir=$PWD\artifacts\release\Rounder_Windows-win-x64-singlefile" `
  ".\installer\Rounder_Windows.iss"
```

The installer is:

```text
artifacts\installer\Rounder_Windows_Setup.exe
```

## GitHub Actions Release

The `Build and Release` workflow builds the pushed commit on Windows. Pushes to `main` or `master` upload workflow artifacts. Pushing a `v*` tag also creates or updates the matching GitHub Release and uploads:

- `Rounder_Windows.exe`
- `Rounder_Windows-win-x64-singlefile.zip`
- `Rounder_Windows_Setup.exe`

```powershell
git tag v2.1.4
git push origin v2.1.4
```

## Implementation Notes

- Target framework: .NET 9, `net9.0-windows`
- Tray/app lifetime: Windows Forms `ApplicationContext` and `NotifyIcon`
- Settings UI: WPF with the official .NET 9 Fluent theme, Desktop Acrylic backdrop, and a macOS-style sidebar with continuous settings scroll
- Overlay drawing: click-through topmost layered WinForms windows with per-pixel alpha
- Gaming glow: separate transparent edge-band layered windows with animated rainbow gradients
- Startup: `HKCU\Software\Microsoft\Windows\CurrentVersion\Run`
- Settings storage: JSON under `%AppData%\Rounder`

## Troubleshooting

**The corners are not visible.**  
Check that Rounder is enabled and that the target display is selected.

**The overlay is hidden behind system UI.**  
Rounder reasserts topmost z-order, but secure desktops, lock screen, and exclusive fullscreen apps can still appear above normal app windows.

**The build fails because `Rounder_Windows.exe` is locked.**  
Exit the running app from the tray menu, or stop the `Rounder_Windows` process, then build again.
