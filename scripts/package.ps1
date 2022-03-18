Import-Module "$env:ProgramFiles\Microsoft Visual Studio\2022\Community\Common7\Tools\Microsoft.VisualStudio.DevShell.dll"
Enter-VsDevShell -VsInstallPath "$env:ProgramFiles\Microsoft Visual Studio\2022\Community"
Set-Location "$(Split-Path $MyInvocation.MyCommand.Path)\.."

$appVersion = ([Xml](Get-Content -Path "src\WinDynamicDesktop.csproj")).Project.PropertyGroup.Version

Remove-Item "dist" -ErrorAction Ignore -Recurse
New-Item -ItemType Directory -Force -Path "dist\msix"

foreach ($platform in @("x86", "x64", "arm64")) {
    dotnet publish src\WinDynamicDesktop.csproj -a $platform -c Release --no-restore --self-contained -p:EnableCompressionInSingleFile=true -p:PublishSingleFile=true
    Move-Item -Path "src\bin\Release\net6.0-windows10.0.19041.0\win-$platform\publish\WinDynamicDesktop.exe" -Destination "dist\WinDynamicDesktop_$appVersion`_$platform`_Portable.exe"

    iscc scripts\installer.iss /dMyAppPlatform="$platform" /dMyAppVersion="$appVersion"

    # TODO Review build flags
    msbuild uwp\WinDynamicDesktop.Package.wapproj /v:m /p:AppxBundle=Never /p:Configuration=Release /p:Platform=$platform
    Move-Item -Path "uwp\AppPackages\WinDynamicDesktop.Package_$appVersion`.0_$platform`_Test\WinDynamicDesktop.Package_$appVersion`.0_$platform`.msix" -Destination "dist\msix"
}

MakeAppx bundle /d "dist\msix" /p "dist\WinDynamicDesktop_$appVersion.msixbundle"
Remove-Item "dist\msix" -Recurse

Get-ChildItem "dist" -Filter *.exe | ForEach-Object {
    $checksum = (Get-FileHash -Path $_.FullName).Hash.ToLower()
    Add-Content -Path "dist\checksums.txt" "$checksum  $($_.BaseName + $_.Extension)"
}
