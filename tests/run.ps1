$ErrorActionPreference = 'Stop'
$root = Split-Path $PSScriptRoot
$csc = & "${env:ProgramFiles(x86)}\Microsoft Visual Studio\Installer\vswhere.exe" -latest -products * -find 'MSBuild\**\Bin\Roslyn\csc.exe' | Select-Object -First 1
& $csc /nologo /langversion:7.3 "/out:$root\build\SelfCheck.exe" "$root\src\Logic.cs" "$PSScriptRoot\SelfCheck.cs"
if ($LASTEXITCODE) { exit 1 }
& "$root\build\SelfCheck.exe"
if ($LASTEXITCODE) { exit 1 }
# The real config.ini must load with the same values it holds.
$type = [Reflection.Assembly]::LoadFile("$root\build\MacStyle.exe").GetType('MacStyle.Config')
$cfg = $type.GetMethod('Load').Invoke($null, @())
$bad = 0
foreach ($line in Get-Content "$env:APPDATA\MacStyle\config.ini" -ErrorAction SilentlyContinue) {
    $k, $v = $line.Split('=', 2)
    $f = $type.GetField($k.Substring(0, 1).ToUpper() + $k.Substring(1))
    $x = $f.GetValue($cfg)
    $s = if ($x -is [bool]) { [string][int]$x } elseif ($x -is [double]) { $x.ToString('0.00', [Globalization.CultureInfo]::InvariantCulture) } else { [string]$x }
    if ($s -ne $v) { $bad++; "config mismatch ${k}: file=$v loaded=$s" }
}
"config.ini: $bad mismatches; notifyEnabled default=$($cfg.NotifyEnabled)"
exit $bad
