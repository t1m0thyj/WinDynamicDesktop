Set-Location (Split-Path $MyInvocation.MyCommand.Path)

$buildDir = "..\src\bin\Release"
$appVersion = (Get-Item -Path "$buildDir\WinDynamicDesktop.exe").VersionInfo.ProductVersion

Copy-Item -Path "$buildDir\WinDynamicDesktop.exe" -Destination "..\dist\WinDynamicDesktop_$appVersion`_Portable.exe"

iscc .\installer.iss
