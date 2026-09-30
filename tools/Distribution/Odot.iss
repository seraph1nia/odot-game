#ifndef SourceDir
  #error SourceDir is required
#endif
#ifndef OutputDir
  #error OutputDir is required
#endif
#ifndef OutputName
  #error OutputName is required
#endif
#ifndef AppVersion
  #error AppVersion is required
#endif

[Setup]
AppId={{D875B866-CA9C-4C70-914F-DBBD6507EE41}
AppName=Odot
AppVersion={#AppVersion}
AppVerName=Odot {#AppVersion}
DefaultDirName={localappdata}\Programs\Odot
DefaultGroupName=Odot
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
OutputDir={#OutputDir}
OutputBaseFilename={#OutputName}
Compression=lzma2/max
SolidCompression=yes
CloseApplications=yes
RestartApplications=no
UninstallDisplayIcon={app}\game\odot.exe
DisableProgramGroupPage=yes
WizardStyle=modern

[InstallDelete]
Type: filesandordirs; Name: "{app}\game"

[Files]
Source: "{#SourceDir}\*"; DestDir: "{app}\game"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{group}\Odot"; Filename: "{app}\game\odot.exe"; WorkingDir: "{app}\game"

[Run]
Filename: "{app}\game\odot.exe"; Description: "Launch Odot"; Flags: nowait postinstall skipifsilent
