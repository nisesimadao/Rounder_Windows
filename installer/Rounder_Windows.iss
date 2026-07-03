#define MyAppName "Rounder for Windows"
#define MyAppExeName "Rounder_Windows.exe"
#ifndef MyAppVersion
#define MyAppVersion "2.1.4"
#endif
#ifndef PublishDir
#define PublishDir "..\artifacts\release\Rounder_Windows-win-x64-singlefile"
#endif

[Setup]
AppId={{2B4EB35B-650D-4F0F-9D94-89C7624D5324}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher=Nisesimadao
AppPublisherURL=https://github.com/nisesimadao/Rounder_Windows
AppSupportURL=https://github.com/nisesimadao/Rounder_Windows/issues
AppUpdatesURL=https://github.com/nisesimadao/Rounder_Windows/releases
DefaultDirName={autopf}\Rounder
DefaultGroupName=Rounder
DisableProgramGroupPage=yes
OutputDir=..\artifacts\installer
OutputBaseFilename=Rounder_Windows_Setup
SetupIconFile=..\Assets\rounder.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
PrivilegesRequired=lowest
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoCompany=Nisesimadao
VersionInfoDescription=Rounder for Windows Setup
VersionInfoProductName=Rounder for Windows
VersionInfoProductVersion={#MyAppVersion}

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#PublishDir}\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion

[Icons]
Name: "{group}\Rounder"; Filename: "{app}\{#MyAppExeName}"
Name: "{group}\Uninstall Rounder"; Filename: "{uninstallexe}"
Name: "{autodesktop}\Rounder"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,Rounder}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
Type: filesandordirs; Name: "{app}"
