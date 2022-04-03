param (
    [Parameter(Position=0)][string]$newVersion
)

$appxmanifest = "uwp\Package.appxmanifest"
$csproj = "src\WinDynamicDesktop.csproj"

$xmlDoc = [XML](Get-Content -Path $csproj)
$oldVersion = $xmlDoc.Project.PropertyGroup.Version

if (!$newVersion) {
    $newVersion = "$oldVersion-$(git rev-parse --short HEAD)"
}

$xmlDoc.Project.PropertyGroup.Version = $newVersion
$xmlDoc.Save($csproj)

$xmlDoc = [XML](Get-Content -Path $appxmanifest)
$newVersion4 = $newVersion -Replace '^(\d+\.\d+\.\d+).*', '$1.0'
$xmlDoc.Package.Identity.setAttribute("Version", $newVersion4)
$xmlDoc.Save($appxmanifest)
