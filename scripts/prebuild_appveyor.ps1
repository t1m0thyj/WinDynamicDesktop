# Update global assemblyinfo with build number
# https://gist.github.com/FeodorFitsner/82dad08650d72268e48d

$assemblyFile = "$env:APPVEYOR_BUILD_FOLDER\src\Properties\AssemblyInfo.cs"

$regex = new-object System.Text.RegularExpressions.Regex ('(AssemblyVersion(Attribute)?\s*\(\s*\")([\d\.]*)(\"\s*\))',
         [System.Text.RegularExpressions.RegexOptions]::MultiLine)

$content = [IO.File]::ReadAllText($assemblyFile)

$version = $null
$match = $regex.Match($content)
if($match.Success) {
    $version = $match.groups[3].value
}

# new version
$version = "$version.$env:APPVEYOR_BUILD_NUMBER"

# update assembly info
$content = $regex.Replace($content, '${1}' + $version + '${4}')
[IO.File]::WriteAllText($assemblyFile, $content)

# update AppVeyor build
Update-AppveyorBuild -Version $version
