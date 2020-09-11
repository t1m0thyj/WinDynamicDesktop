Set-Location (Split-Path $MyInvocation.MyCommand.Path)

$buildDir = "..\src\bin\Release"
$appVersion = (Get-Item -path "$buildDir\WinDynamicDesktop.exe").VersionInfo.ProductVersion

python .\clean_release.py "$buildDir"

7z a "..\dist\WinDynamicDesktop_$appVersion`_Portable.zip" "$buildDir\*"

iscc .\installer.iss
