# Rounder for Windows

Rounder for Windows is a lightweight notification-area app that draws rounded-corner overlays on selected monitors.

It is a Windows port of the macOS Rounder app. The Windows version keeps the same core behavior where practical, while replacing macOS APIs with Windows-native equivalents: WinForms tray hosting, click-through topmost overlay windows, WPF settings UI, JSON settings, presets, multi-monitor selection, and the animated Super Duper Gaming Mode.

![Rounder icon](Assets/rounder.png)

[Japanese README](./README_jp.md)

## Features

- Runs quietly from the Windows notification area.
- Left-click or double-click the tray icon to open settings.
- Toggle rounded-corner overlays on or off from the tray menu.
- Choose which monitors receive overlays.
- Adjust radius, color, visible corners, and gaming-mode animation.
- Save, edit, import, and export presets as JSON.
- Uses a Fluent-style WPF settings window with light/dark theme support.
- Per-monitor DPI aware for mixed scaling environments.

## Requirements

- Windows 10 or Windows 11
- .NET 8.0 Desktop Runtime
- .NET 8.0 SDK if building from source

## Build

From this directory:

```powershell
dotnet build .\Rounder_Windows.csproj -c Release
```

The executable is written to:

```text
bin\Release\net8.0-windows\Rounder_Windows.exe
```

If a previous copy of Rounder is running, close it from the tray menu before rebuilding. Windows will lock the executable while the app is running.

## Run

```powershell
.\bin\Release\net8.0-windows\Rounder_Windows.exe
```

After launch, Rounder appears in the notification area. Click the tray icon to open settings, or right-click it for the menu.

## Settings

### General

- Enable or disable rounded corners.
- Refresh and select monitors.
- Set corner radius from 0 to 40 pixels.
- Pick corner color with quick presets or a custom color picker.
- Toggle each corner independently.

### Presets

- Apply saved presets.
- Save the current settings as a new preset.
- Edit or delete existing presets.
- Import and export preset files.

### Super Duper Gaming Mode

- Enables animated rainbow/glow overlays.
- Provides speed and glow intensity controls.

## Implementation Notes

- Target framework: .NET 8, `net8.0-windows`
- Tray/app lifetime: Windows Forms `ApplicationContext` and `NotifyIcon`
- Settings UI: WPF with `iNKORE.UI.WPF.Modern`
- Overlay drawing: WinForms topmost layered windows and GDI+
- Click-through behavior: Win32 extended window styles such as `WS_EX_TRANSPARENT`
- Settings storage: JSON under `%AppData%\Rounder`
- DPI behavior: `HighDpiMode.PerMonitorV2`

## Project Structure

```text
Rounder_Windows/
|-- Assets/                      App icon and image assets
|-- AppAssets.cs                  Embedded icon/image loading
|-- AppSettings.cs                Settings model
|-- AppTheme.cs                   Windows theme detection helpers
|-- CornerOverlayForm.cs          Click-through overlay window
|-- CornerPreset.cs               Preset model
|-- JsonStore.cs                  JSON settings and preset storage
|-- OverlayManager.cs             Overlay lifecycle management
|-- PresetEditorForm.cs           Preset editing dialog
|-- Program.cs                    STA entry point and DPI setup
|-- RounderApplicationContext.cs  Tray icon and app lifetime
|-- WpfSettingsWindow.xaml        Fluent-style settings window
|-- WpfThemeBootstrap.cs          WPF/iNKORE theme bootstrap
|-- Rounder_Windows.csproj        Project file
|-- README.md                     English README
`-- README_jp.md                  Japanese README
```

## Troubleshooting

**The corners are not visible.**  
Check that Rounder is enabled and that the target monitor is selected.

**The overlay is hidden behind some apps.**  
Exclusive fullscreen apps, secure desktops such as UAC, and the lock screen can appear above normal topmost windows.

**The build fails because `Rounder_Windows.exe` is locked.**  
Exit the running app from the tray menu, or stop the `Rounder_Windows` process, then build again.
