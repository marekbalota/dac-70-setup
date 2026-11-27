; Inno Setup script for the Windows installer. The workflow compiles it with
;   ISCC.exe /DAppVersion=X.Y.Z /DSourceDir=<dotnet publish folder> /O<output folder> DAC70Setup.iss
; It installs the same self-contained files as the Windows zip, for the current user by default,
; so no administrator rights are needed.

#ifndef AppVersion
  #error Pass the version with /DAppVersion=X.Y.Z
#endif
#ifndef SourceDir
  #error Pass the dotnet publish folder with /DSourceDir=<folder>
#endif

#define AppName "DAC-70 Setup"
#define AppExeName "DAC70Setup.exe"

[Setup]
; Never change the AppId: Windows recognises later versions as upgrades of the same app by it
AppId={{0B3BE638-E47B-41CE-9668-6E95408BF3F9}
AppName={#AppName}
AppVersion={#AppVersion}
AppPublisher=Marek Balota
AppPublisherURL=https://github.com/marekbalota/dac-70-setup
VersionInfoVersion={#AppVersion}
DefaultDirName={autopf}\{#AppName}
DisableProgramGroupPage=yes
PrivilegesRequired=lowest
PrivilegesRequiredOverridesAllowed=dialog
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
OutputBaseFilename=DAC70Setup-v{#AppVersion}-windows-x64-installer
SetupIconFile={#SourceDir}\appicon.ico
UninstallDisplayIcon={app}\{#AppExeName}
WizardStyle=modern
Compression=lzma2
SolidCompression=yes
ShowLanguageDialog=auto

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{autoprograms}\{#AppName}"; Filename: "{app}\{#AppExeName}"
Name: "{autodesktop}\{#AppName}"; Filename: "{app}\{#AppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#AppExeName}"; Description: "{cm:LaunchProgram,{#AppName}}"; Flags: nowait postinstall skipifsilent
