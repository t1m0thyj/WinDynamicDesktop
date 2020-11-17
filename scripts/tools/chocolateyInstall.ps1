
$ErrorActionPreference = 'Stop';
$toolsDir   = "$(Split-Path -parent $MyInvocation.MyCommand.Definition)"
$url        = '{{installerUrl}}'

$packageArgs = @{
  packageName   = $env:ChocolateyPackageName
  unzipLocation = $toolsDir
  fileType      = 'EXE'
  url           = $url

  softwareName  = 'WinDynamicDesktop*'

  checksum      = '{{installerChecksum}}'
  checksumType  = 'sha256'

  silentArgs    = '/VERYSILENT /SUPPRESSMSGBOXES /NORESTART /SP- /RESTARTAPPLICATIONS'
  validExitCodes= @(0)
}

Install-ChocolateyPackage @packageArgs
