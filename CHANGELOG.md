# Changelog

## `4.7.0`

* Added support for syncing wallpaper across virtual desktops in Windows 11
* Added Arabic U.A.E., Hungarian, and Icelandic translations (thanks mustafa, DJ Phoenix, and Anonymous)
* Fixed startup task being created for all users in installer instead of current user

## `4.6.0`

* Added Monterey Abstract theme
* Added estimate of disk space usage in theme previewer
* Added Arabic and Ukrainian translations (thanks Abdul Rahim and Віталій)
* Fixed timezone calculations failing in Oceania
* Fixed config reload overwriting changes on disk
* Fixed open dialogs preventing app from exiting

## `4.5.0`

* Added installer option to install for all users
* Added ability to favorite themes to move them to top of list
* Added Croatian, Danish, Finnish, Swedish, and Traditional Chinese translations (thanks Denis Bogdan, Anders Ferdinandus, Klokki, Christian, and Williamrob104)
* Reduced memory usage of theme previewer (thanks @cjvaughter)
* Fixed error when location has different timezone that may be a day ahead of or behind the system clock
* Fixed error when multiple users run the app simultaneously on the same machine
* Fixed hang when invalid images cause thumbnail generation to fail
* Fixed missing data that was not being supplied to PowerShell scripts
* Fixed error when downloading new translations from POEditor

## `4.4.0`

* Added 8 new themes from Big Sur 11.0.1
* Added Greek and Vietnamese translations (thanks masterjunior24 and Truong Huynh)
* Fixed issue where wrong preview image was shown for time of day in theme dialog

## `4.3.1`

* Fixed error when 2-image themes (e.g., Big Sur Abstract) are selected in theme dialog

## `4.3.0`

* Redesigned theme selector dialog to show carousel that autocycles through images (thanks @cjvaughter)
* Added Dutch, Korean, and Indonesian translations (thanks Samuel, jihwan_bong, and @ChrisG661)
* Fixed error when Windows 10 default wallpaper missing

## `4.2.0`

* Added Big Sur and Big Sur Abstract themes
* Added Bulgarian and Japanese translations (thanks Marin and Syoyusensation)
* Improved error checking for theme JSON when new themes are installed
* Fixed check for updates failing on Windows 7
