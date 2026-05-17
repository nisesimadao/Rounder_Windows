# Rounder for Windows

Rounder for Windows is a tray utility that makes selected screens look like they have rounded corners. It recreates the macOS version with Windows-native APIs: a notification area icon, click-through topmost overlay windows, JSON settings, multi-monitor selection, presets, and the animated Super Duper Gaming Mode.

## Features

- Runs quietly from the Windows notification area.
- Adds click-through overlays to selected screen corners.
- Adjustable radius from 0 to 40 pixels.
- Custom overlay color.
- Enable or disable each corner independently.
- Select which monitors receive the effect.
- Save, edit, delete, import, export, and apply presets.
- Super Duper Gaming Mode with animated rainbow color and glow.
- Regenerates overlays when display settings change.
- Per-monitor DPI awareness for 100%, 125%, and 150% scale configurations.

## Requirements

- Windows 10 or Windows 11.
- .NET 8 SDK for building from source.
- .NET Desktop Runtime 8 for running a published build.

No administrator permission is required for normal use.

## Build

From the repository root:

```powershell
dotnet build .\Rounder_Windows\Rounder_Windows.csproj -c Release
```

The executable is generated under:

```text
Rounder_Windows\bin\Release\net8.0-windows\
```

## Run

For development:

```powershell
dotnet run --project .\Rounder_Windows\Rounder_Windows.csproj -c Release
```

Or run the built executable directly:

```powershell
.\Rounder_Windows\bin\Release\net8.0-windows\Rounder_Windows.exe
```

The app opens a tray icon. Double-click the icon, or right-click it and choose **Settings...**.

## Settings Storage

Settings and presets are stored in:

```text
%AppData%\Rounder
```

Files:

- `settings.json`
- `presets.json`

## macOS Porting Differences

- macOS `NSStatusItem` is replaced by WinForms `NotifyIcon`.
- macOS `NSWindow.Level.screenSaver` is replaced by topmost click-through WinForms overlay windows using Win32 extended window styles.
- macOS `NSScreen` / `CGDirectDisplayID` is replaced by `Screen.AllScreens` and Windows device names.
- macOS `UserDefaults` is replaced by JSON files in `%AppData%\Rounder`.
- macOS display-change restart behavior is replaced by in-process overlay regeneration.
- macOS permission setup is simplified because the Windows implementation does not need Accessibility, Screen Recording, or Automation permissions.
- The Windows app is configured for PerMonitorV2 DPI awareness, so the settings window and overlay placement adapt across monitors with different scale factors.

## Known Limitations

- The overlay may not appear above the lock screen, UAC secure desktop, or exclusive fullscreen applications.
- Windows virtual desktop behavior depends on OS window-management rules.
- The Windows UI is currently English-only, while the macOS project contains localized strings.
