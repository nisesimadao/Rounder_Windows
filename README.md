# <img src="Assets/rounder.png" width="40" vertical-align="middle" /> Rounder for Windows

A tray utility that makes selected screens look like they have rounded corners.

It recreates the macOS version with Windows-native APIs: a notification area icon, click-through topmost overlay windows, JSON settings, multi-monitor selection, presets, and the animated Super Duper Gaming Mode. It runs quietly in the background and does not interfere with normal system behavior.

<img src="Assets/rounder.png" alt="Icon" />

[日本語版READMEはこちら](./README_jp.md)

## Project Structure

```
Rounder_Windows/
├── RounderApplicationContext.cs # Main application logic and tray icon
├── OverlayManager.cs            # Manages life cycle of overlay windows
├── CornerOverlayForm.cs         # Click-through overlay window (GDI+)
├── WpfSettingsWindow.xaml       # Modern WPF settings UI
├── AppSettings.cs               # Data model for app configuration
├── CornerPreset.cs              # Data model for corner presets
├── JsonStore.cs                 # Local JSON storage for settings/presets
├── AppTheme.cs                 # Theme detection and management
├── AppAssets.cs                 # Embedded icon/image asset management
├── Assets/                      # App icon and images
├── Rounder_Windows.csproj       # .NET Project file
└── README.md                    # This file
```

## Features

- **Background operation**: Runs from the Windows notification area (System Tray)
- **Real-time configuration**: Instantly adjust corner radius and color
- **Individual corner control**: Toggle each corner independently
- **Preset functionality**: Save and switch between favorite settings
- **Import/Export**: Share and backup presets in JSON format
- **Super Duper Gaming Mode**: Animated rainbow effects and glow
- **Multi-monitor support**: Select which monitors to apply rounded corners to
- **Monitor refresh**: Manually refresh monitor list in settings
- **Modern Interface**: Fluent Design UI using ModernWPF
- **Lightweight and stable**: Minimal CPU and memory usage

## System Requirements

- Windows 10 or Windows 11
- .NET 8.0 Desktop Runtime

## Installation

### Prebuilt App

1. Download the latest version from [Releases](https://github.com/nisesimadao/rounder/releases)
2. Extract the ZIP file
3. Run `Rounder_Windows.exe`

### Build from Source

```powershell
# From the project directory
dotnet build -c Release
```

## Usage

### Daily Use

- **System Tray**: Double-click the Rounder icon to open settings
- **Adjust settings**: Change corner radius (0–40px) and color in real time
- **Individual corners**: Toggle each corner independently (2x2 grid layout)
- **Enable/disable**: Toggle rounded corner effect on or off from the tray menu
- **Quit**: Fully exit the app from the tray menu

### Preset Features

- **Apply presets**: One-click apply saved configurations
- **Save current settings**: Create new preset from current configuration
- **Edit presets**: Rename or delete existing presets
- **Import/Export**: Share and backup presets in JSON format

## Configuration Options

### General Settings
- **Corner radius**: Adjustable from 0 to 40 pixels
- **Corner color**: Choose via color picker or standard colors
- **Corner visibility**: Toggle each corner independently
- **Enable**: Toggle the rounded corner effect
- **Monitor selection**: Choose which monitors to apply rounded corners to
- **Refresh monitors**: Manually refresh the monitor list

### Super Duper Gaming Mode
- **Enable**: Rainbow animation effects and glow
- **Speed control**: Adjust animation speed
- **Glow intensity**: Adjust the glow effect strength

## Technical Details

### Core Technologies
- **.NET 8 (C#)**: Modern runtime
- **Windows Forms**: Tray icon and overlay host
- **WPF (ModernWPF)**: Settings UI with Fluent Design
- **GDI+**: High-performance overlay drawing
- **Win32 API**: Low-level window style manipulation (WS_EX_TRANSPARENT, etc.)
- **System.Text.Json**: Settings persistence

### Performance
- **Low overhead**: Minimal CPU usage
- **Memory efficient**: Native Windows optimizations
- **Real-time updates**: Changes apply instantly using event-driven architecture

## Troubleshooting

### Common Issues

**Q: Rounded corners are not visible**  
A: Ensure the app is enabled in the settings or tray menu. Check if the correct monitors are selected.

**Q: Overlay is not above some windows**  
A: Exclusive fullscreen games, secure desktops (UAC), and the lock screen may appear above the overlay.

**Q: High DPI Scaling**  
A: The app is PerMonitorV2 DPI aware and should scale correctly across different monitors.
