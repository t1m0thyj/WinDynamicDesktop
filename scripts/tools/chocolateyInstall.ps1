
$ErrorActionPreference = 'Stop';
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = '{{installerUrl}}'
$url64      = '{{installerUrl64}}'

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'EXE'
  url           = $url
  url64         = $url64

  softwareName  = 'WinDynamicDesktop*'

  checksum      = '{{installerChecksum}}'
  checksumType  = 'sha256'
  checksum64    = '{{installerChecksum64}}'
  checksumType64= 'sha256'

  silentArgs    = '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /RESTARTAPPLICATIONS'
  validExitCodes= @(0)
}

Install-ChocolateyPackage @packageArgs
