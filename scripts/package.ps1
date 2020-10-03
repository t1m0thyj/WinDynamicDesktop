Set-Location (Split-Path $MyInvocation.MyCommand.Path)

$buildDir = "..\src\bin\Release"
$distDir = "..\dist"
$appVersion = (Get-Item -Path "$buildDir\WinDynamicDesktop.exe").VersionInfo.ProductVersion

New-Item -Path $distDir -ItemType Directory -Force
Copy-Item -Path "$buildDir\WinDynamicDesktop.exe" -Destination "$distDir\WinDynamicDesktop_$appVersion`_Portable.exe"

iscc .\installer.iss
