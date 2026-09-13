# Contributing

Contributions and bug reports are welcome.

## Development setup

- Windows 10 or 11
- .NET 9 SDK

Build with:

```powershell
dotnet build .\Rounder_Windows.csproj -c Release
```

When changing overlays, test at least Rounded, Squircle, and Polygon on a primary display and, when available, a mixed-DPI multi-monitor setup. When changing startup or tray behavior, verify normal launch, `--background`, hidden tray icon recovery, and single-instance activation.

Keep README and changelog entries in sync with user-visible behavior.
