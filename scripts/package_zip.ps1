Set-Location (Split-Path $MyInvocation.MyCommand.Path)

$buildDir = "..\src\bin\Release"
$appVersion = (Get-Item -path "$buildDir\WinDynamicDesktop.exe").VersionInfo.ProductVersion

python .\clean_release.py "$buildDir"

7z a "..\dist\WinDynamicDesktop_Portable_$appVersion.zip" "$buildDir\*"
