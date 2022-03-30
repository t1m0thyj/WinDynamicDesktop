Import-Module "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
Enter-VsDevShell -VsInstallPath "$env:ProgramFiles\Microsoft Visual Studio\2022\Community"
Set-Location "$(Split-Path $MyInvocation.MyCommand.Path)\.."

$appPlatforms = @("x86", "x64", "arm64")
$appVersion = ([Xml](Get-Content -Path "src\WinDynamicDesktop.csproj")).Project.PropertyGroup.Version

Remove-Item "dist" -ErrorAction Ignore -Recurse
New-Item -ItemType Directory -Force -Path "dist"

dotnet restore src\WinDynamicDesktop.sln

foreach ($platform in $appPlatforms) {
    dotnet publish src\WinDynamicDesktop.csproj -a $platform -c Release --no-restore --self-contained -p:EnableCompressionInSingleFile=true -p:IncludeNativeLibrariesForSelfExtract=true -p:PublishSingleFile=true
    Copy-Item -Path "src\bin\Release\net6.0-windows10.0.19041.0\win-$platform\publish\WinDynamicDesktop.exe" -Destination "dist\WinDynamicDesktop_$appVersion`_$platform`_Portable.exe"

    iscc scripts\installer.iss /dMyAppPlatform="$platform" /dMyAppVersion="$appVersion"
}

msbuild uwp\WinDynamicDesktop.Package.wapproj /v:m /p:AppxBundle=Always /p:AppxBundlePlatforms=$($appPlatforms -join '|') /p:AppxPackageSigningEnabled=false /p:Configuration=Release /p:Platform=$($appPlatforms[0]) /p:UapAppxPackageBuildMode=StoreOnly
Copy-Item -Path "uwp\AppPackages\WinDynamicDesktop.Package_$appVersion`*.msixupload" -Destination "dist\WinDynamicDesktop_$appVersion`.msixupload"

Get-ChildItem "dist" -Filter *.exe | ForEach-Object {
    $checksum = (Get-FileHash -Path $_.FullName).Hash.ToLower()
    Add-Content -Path "dist\checksums.txt" "$checksum  $($_.BaseName + $_.Extension)"
}
