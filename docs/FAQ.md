# Rounder for Windows FAQ

## What does Rounder do?

Rounder places small click-through, topmost overlay windows at the four corners of selected displays. It does not patch the Windows shell or display driver.

## Does it need administrator access?

No. Rounder runs as a normal user application.

## Does it need screen-recording or accessibility permission?

No. Rounder does not read screen contents.

## I hid the tray icon. How do I open Settings again?

Launch `Rounder_Windows.exe` again. The already-running Rounder process will open Settings; a second instance is not created.

## Will Settings open every time I sign in?

No. Launch-at-login uses `--background`, so automatic startup is silent.

## Can I select no displays?

Yes. Turning off every item under Displays keeps Rounder running while intentionally showing no overlays. The choice is preserved across launches.

## What happens when I connect a new monitor?

Newly detected displays are selected by default, while existing displays you previously deselected stay deselected.

## Does it work over exclusive fullscreen applications?

Rounder stays topmost for normal desktop and borderless-fullscreen use, but secure desktops, the lock screen, and some exclusive-fullscreen applications can appear above normal application windows.

## Where are settings stored?

`%AppData%\Rounder\settings.json` and `%AppData%\Rounder\presets.json`.

## Do release builds require the .NET Runtime?

No. GitHub Release executables and installers are self-contained. Building from source requires the .NET 9 SDK.
