# Changelog

All notable changes to Rounder for Windows are tracked here.

## v2.3.1

- Replaced the custom/iNKORE-styled Windows UI with the built-in .NET 9 WPF Fluent theme across Settings, tray quick controls, preset editing, and prompt dialogs.
- Rebuilt the tray panel as a native WPF Fluent flyout for visual consistency with Settings.
- Restored rounded navigation tabs, optically centered Fluent glyphs, and circular quick-color swatches while keeping the built-in .NET 9 Fluent theme.
- Restored circular quick-color swatches and normalized Fluent icon alignment across the sidebar and tray actions.
- Brought the Windows port up to the macOS Rounder 2.3.1 feature level where Windows allows it.
- Added tray quick controls for enable/disable, corner radius, shape, quick colors, individual corners, and Super Duper Gaming Mode.
- Restored Rounder-style icon navigation, rounded selected tabs, and segmented corner-shape controls while keeping the built-in .NET 9 Fluent theme underneath.
- Added a **Show tray icon** setting. Rounder keeps running when the icon is hidden.
- Added single-instance activation: launching Rounder again reopens Settings instead of creating a second process.
- Launch-at-login now starts silently with `--background`, including when the tray icon is hidden.
- Aligned the Settings layout with current Rounder: matching section order, wider sidebar, segmented corner-shape selector, quick color controls, and conditional Gaming controls.
- Removed the obsolete Permissions section from the Windows Settings UI.
- Corrected About-page technology text so it describes the Windows implementation rather than the macOS implementation.
- Fixed display selection so selecting no displays is preserved and previously deselected displays do not come back on the next launch.
- Newly connected displays are selected by default without resetting existing display choices.
- Updated English and Japanese README files to the current Rounder structure and added real Windows screenshots.
- Added Windows FAQ, Privacy, Security, and Contributing documents.

## v2.1.5

- Matched Gaming Mode rendering more closely to macOS Rounder.
- Added automatic GitHub Release publishing from the project version.
- Kept single-file and Inno Setup release packaging in the CI workflow.

## v2.1.4

- Ported the core Rounder feature set to Windows.
- Added selectable monitors, presets, Rounded / Squircle / Polygon cutouts, login startup, and Super Duper Gaming Mode.
- Added mixed-DPI overlay positioning and Fluent-style WPF settings.
