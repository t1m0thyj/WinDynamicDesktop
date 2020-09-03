Set-Location (Split-Path $MyInvocation.MyCommand.Path)

Get-ChildItem -Path "..\src\packages\CefSharp.*\build\CefSharp.*.targets" | ForEach-Object {
    $oldText = Get-Content -Path $_.FullName | Out-String
    $newText = $oldText -replace "$$\(Platform\)","$$(CefSharpPlatform)"

    if ($newText -ne $oldText) {
        Copy-Item -Path $_.FullName -Destination "$($_.FullName).bak"
        Set-Content -Path $_.FullName -Value $newText
    }
}
