Set-Location (Split-Path $MyInvocation.MyCommand.Path)

$buildDir = "..\src\bin\Release"
$distDir = "..\dist"
$appVersion = (Get-Item -Path "$buildDir\WinDynamicDesktop.exe").VersionInfo.ProductVersion

New-Item -Path $distDir -ItemType Directory -Force
Copy-Item -Path "$buildDir\WinDynamicDesktop.exe" -Destination "$distDir\WinDynamicDesktop_$appVersion`_Portable.exe"

iscc .\installer.iss

$installerHash = (Get-FileHash -Path "$distDir\WinDynamicDesktop_$appVersion`_Setup.exe").Hash.ToLower()
$portableHash = (Get-FileHash -Path "$distDir\WinDynamicDesktop_$appVersion`_Portable.exe").Hash.ToLower()
$appVersion3 = $appVersion -Replace "\.0$"
$checksumsTxt = "$distDir\checksums.txt"

Remove-Item $checksumsTxt -ErrorAction Ignore
Add-Content -Path $checksumsTxt "$installerHash  WinDynamicDesktop_$appVersion3`_Setup.exe"
Add-Content -Path $checksumsTxt "$portableHash  WinDynamicDesktop_Portable.exe"
