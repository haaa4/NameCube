; Script generated by the Inno Setup Script Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#define MyAppName "学号魔方"
#define MyAppVersion "Alpha-3"
#define MyAppPublisher "哈阿斯"
#define MyAppURL "https://github.com/haaa4/NameCube"
#define MyAppExeName "学号魔方.exe"
#define MyAppAssocName MyAppName + ""
#define MyAppAssocExt ".myp"
#define MyAppAssocKey StringChange(MyAppAssocName, " ", "") + MyAppAssocExt

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{A68C6C27-1F78-4F72-8936-E1D0229B1724}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
;AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\{#MyAppName}
ChangesAssociations=yes
DisableProgramGroupPage=yes
InfoBeforeFile=D:\C#\NameCube\NameCube\bin\Need\注意.txt
; Remove the following line to run in administrative install mode (install for all users.)
PrivilegesRequired=lowest
OutputDir=D:\C#\NameCube\NameCube\bin\OutPut
OutputBaseFilename=学号魔方-Alpha-3
SetupIconFile=D:\C#\NameCube\NameCube\icon.ico
Compression=lzma
SolidCompression=yes
WizardStyle=modern

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "chinese"; MessagesFile: "compiler:Languages\Chinese.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked

[Files]
Source: "D:\C#\NameCube\NameCube\bin\App\{#MyAppExeName}"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\config.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\Masuit.Tools.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\Masuit.Tools.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\Masuit.Tools.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\Microsoft.Bcl.AsyncInterfaces.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\Microsoft.Bcl.AsyncInterfaces.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\Newtonsoft.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.Buffers.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.IO.Pipelines.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.IO.Pipelines.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.Memory.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.Memory.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.Numerics.Vectors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.Numerics.Vectors.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.Runtime.CompilerServices.Unsafe.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.Text.Encodings.Web.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.Text.Encodings.Web.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.Text.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.Text.Json.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.Threading.Tasks.Extensions.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.ValueTuple.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\System.ValueTuple.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\Wpf.Ui.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\Wpf.Ui.Abstractions.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\Wpf.Ui.Abstractions.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\Wpf.Ui.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\Wpf.Ui.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\Wpf.Ui.xml"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\学号魔方.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "D:\C#\NameCube\NameCube\bin\App\学号魔方.pdb"; DestDir: "{app}"; Flags: ignoreversion
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Registry]
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocExt}\OpenWithProgids"; ValueType: string; ValueName: "{#MyAppAssocKey}"; ValueData: ""; Flags: uninsdeletevalue
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}"; ValueType: string; ValueName: ""; ValueData: "{#MyAppAssocName}"; Flags: uninsdeletekey
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\DefaultIcon"; ValueType: string; ValueName: ""; ValueData: "{app}\{#MyAppExeName},0"
Root: HKA; Subkey: "Software\Classes\{#MyAppAssocKey}\shell\open\command"; ValueType: string; ValueName: ""; ValueData: """{app}\{#MyAppExeName}"" ""%1"""
Root: HKA; Subkey: "Software\Classes\Applications\{#MyAppExeName}\SupportedTypes"; ValueType: string; ValueName: ".myp"; ValueData: ""

[Icons]
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"; Flags: nowait postinstall skipifsilent

