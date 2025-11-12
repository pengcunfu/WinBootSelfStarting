# WinBootSelfStarting

A small WPF tool to manage Windows user startup entries (HKCU Run and per-user Startup folder).

Features
- List startup entries (registry Run and user Startup folder)
- Add registry startup entries (add executable path as HKCU Run value)
- Enable / Disable startup entries (moves registry values to a disabled key or moves files to a disabled folder)
- Remove startup entries
- Search/filter entries

How to build and run locally

Requirements
- .NET SDK 10 (or later) with WPF support
- Windows (WPF desktop app)

Build and run (PowerShell):
```powershell
dotnet build "d:\Data\Desktop\Projects\WinBootSelfStarting\WinBootSelfStarting.csproj" -c Debug
dotnet run --project "d:\Data\Desktop\Projects\WinBootSelfStarting\WinBootSelfStarting.csproj" -c Debug
```

CI / Release

This repository includes a GitHub Actions workflow `.github/workflows/publish.yml` that will build the project on `windows-latest` and upload the published output as a workflow artifact when you push to the default branch.

Notes and safety
- The app modifies per-user registry keys and files under the user Startup folder. Changes are reversible via the UI (enable/disable) but use caution. Machine-wide changes (HKLM) are not implemented and require elevation.
- If you want shortcut (.lnk) creation in the Startup folder instead of registry entries, I can add that.

Next steps
- Add tests and an abstraction around registry/file access to enable unit tests.
- Improve UX: detailed error messages, confirmations, right-click menu, icons.
