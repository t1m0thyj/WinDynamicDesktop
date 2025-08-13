param (
    [Parameter(Position=0)][string]$newVersion
)

$appxmanifest = "uwp\Package.appxmanifest"
$csproj = "src\WinDynamicDesktop.csproj"

$xmlDoc = [XML](Get-Content -Path $csproj)
$oldVersion = $xmlDoc.Project.PropertyGroup[0].Version

if (!$newVersion) {
    $newVersion = "$oldVersion-g$(git rev-parse --short HEAD)"
} elseif ($newVersion -eq $oldVersion) {
    Write-Host "Version is already $newVersion"
    exit
}

$xmlDoc.Project.PropertyGroup[0].Version = $newVersion
$xmlDoc.Save("$pwd\$csproj")

$xmlDoc = [XML](Get-Content -Path $appxmanifest)
$newVersion4 = $newVersion -Replace '^(\d+\.\d+\.\d+).*', '$1.0'
$xmlDoc.Package.Identity.setAttribute("Version", $newVersion4)
$xmlDoc.Save("$pwd\$appxmanifest")
