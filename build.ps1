param([string]$Out = 'MacStyle.exe')
$ErrorActionPreference = 'Stop'
$csc = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
$fw = "$env:windir\Microsoft.NET\Framework64\v4.0.30319"
$refs = @('mscorlib', 'System', 'System.Core', 'System.Data', 'System.Drawing', 'System.Windows.Forms', 'System.Xml', 'System.Management', 'System.Web.Extensions', 'Microsoft.CSharp', 'System.Runtime', 'System.Runtime.WindowsRuntime', 'System.Runtime.InteropServices.WindowsRuntime', 'System.Xaml', 'WPF\PresentationCore', 'WPF\PresentationFramework', 'WPF\WindowsBase') | % { "/r:$fw\$_.dll" }
$refs += @('Foundation', 'Media', 'Storage', 'Devices', 'UI', 'ApplicationModel') | % { "/r:$env:windir\System32\WinMetadata\Windows.$_.winmd" }
New-Item -ItemType Directory -Force "$PSScriptRoot\build" | Out-Null
& $csc /nologo /noconfig /nostdlib /unsafe /target:winexe /platform:anycpu /optimize+ /langversion:7.3 "/win32icon:$PSScriptRoot\src\MacStyle.ico" "/win32manifest:$PSScriptRoot\src\app.manifest" '/nowarn:0618,0649,0169' "/out:$PSScriptRoot\build\$Out" $refs "$PSScriptRoot\src\*.cs"
if ($LASTEXITCODE) { exit $LASTEXITCODE }
Get-Item "$PSScriptRoot\build\$Out" | Select-Object Name, Length
if ($Out -eq 'MacStyle.exe') {
    & $csc /nologo /noconfig /target:winexe /platform:anycpu /optimize+ /langversion:7.3 "/win32icon:$PSScriptRoot\src\MacStyle.ico" "/resource:$PSScriptRoot\build\MacStyle.exe,MacStyle.exe" "/out:$PSScriptRoot\build\MacStyle-Update-v2.exe" "/r:$fw\System.dll" "/r:$fw\System.Windows.Forms.dll" "$PSScriptRoot\updater\Updater.cs"
    if ($LASTEXITCODE) { exit $LASTEXITCODE }
    Get-Item "$PSScriptRoot\build\MacStyle-Update-v2.exe" | Select-Object Name, Length
}
