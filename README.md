# <img src="Assets/rounder.png" width="40" vertical-align="middle" /> Rounder for Windows

A tray utility to make the corners of your selected screen look rounded.

This is a recreation of the macOS version using native Windows APIs. It features a notification area icon, a click-through topmost overlay window, JSON-based settings storage, multi-monitor selection, presets, and a "Super Gaming Mode" with animations. It runs quietly in the background and does not interfere with the system's normal operation.

<img src="Assets/rounder.png" alt="Icon" />

[日本語版 README](./README_jp.md)

## Project Structure

```
Rounder_Windows/
├── RounderApplicationContext.cs # Main application logic and tray icon
├── OverlayManager.cs            # Overlay window lifecycle management
├── CornerOverlayForm.cs         # Click-through overlay window (GDI+)
├── WpfSettingsWindow.xaml       # Settings window using Modern WPF
├── AppSettings.cs               # Data model for app settings
├── CornerPreset.cs              # Data model for corner presets
├── JsonStore.cs                 # Local JSON storage for settings/presets
├── AppTheme.cs                 # Theme detection and management
├── AppAssets.cs                 # Icon/image asset management
├── Assets/                      # App icons and images
├── Rounder_Windows.csproj       # .NET project file
└── README.md                    # This file
```

## Features

- Background operation: Stays in the Windows notification area (system tray)
- Real-time settings: Instant application of corner radius and color
- Individual corner control: Each corner can be toggled on/off independently
- Preset functionality: Save and switch between frequently used settings
- Import/Export: Share and back up presets in JSON format
- Super Gaming Mode: Special mode with animation effects and glow
- Multi-monitor support: Choose which monitor to apply the rounded corners to
- Refresh monitors: Manually update the monitor list in the settings window
- Modern interface: Fluent Design UI via ModernWPF
- Lightweight and stable: Minimal CPU and memory usage

## System Requirements

- Windows 10 or Windows 11
- .NET 8.0 Desktop Runtime

## Installation

### Pre-built App

1. Download the latest version from [Releases](https://github.com/nisesimadao/rounder_windows/releases)
2. Extract the ZIP file
3. Run `Rounder_Windows.exe`

### Build from Source

```powershell
# Run from the project directory
dotnet build -c Release
```

## Usage

### General Usage

- System tray: Double-click the Rounder icon to open settings
- Change settings: Adjust corner radius (0-40px) and color in real time
- Individual corner control: Toggle each corner on/off independently (2x2 grid layout)
- Enable/Disable: Toggle the rounded corner effect from the tray menu
- Exit: Completely close the app from the tray menu

### Preset Functionality

- Apply preset: Apply saved settings with a single click
- Save current settings: Create a new preset from the current configuration
- Edit preset: Rename or delete presets
- Import/Export: Share and back up presets in JSON format

## Settings Options

### General Settings
- Corner radius: Adjustable from 0 to 40 pixels
- Corner color: Select from the color picker or standard colors
- Corner display: Toggle four corners individually
- Enable: Toggle rounded corner effects
- Monitor selection: Choose the monitor to apply rounded corners
- Refresh monitors: Manually update the monitor list

### Super Gaming Mode
- Enable: Rainbow animation and glow effects
- Speed control: Control the animation speed
- Glow intensity: Adjust the strength