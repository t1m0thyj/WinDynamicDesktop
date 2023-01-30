# Changelog

## `5.2.0`

* Added dark UI theme using Mica (only for Windows 11 22H2)
* Added Burmese translation (thanks Febri)
* Fixed user-defined sunrise and sunset times being stored incorrectly for some locales ([#487](https://github.com/t1m0thyj/WinDynamicDesktop/issues/487))
* Fixed IndexOutOfRangeException that could occur when Select Theme dialog is opened and there are multiple monitors ([#498](https://github.com/t1m0thyj/WinDynamicDesktop/issues/498))
* Fixed error when PowerShell scripts run with blank imagePath parameter because there is no active theme

## `5.1.0`

**Note:** This version removes syncing wallpaper across virtual desktops in Windows 11 which used an experimental API that is too unstable to support. If you still want this functionality, install the [Sync Virtual Desktops](https://github.com/t1m0thyj/WDD-scripts/tree/master/experimental#synchronize-virtual-desktops) script.

* Added Ventura Abstract theme ([#451](https://github.com/t1m0thyj/WinDynamicDesktop/issues/451))
* Added dropdown in theme download dialog to select alternate mirrors that may be faster
* Added "Hide system tray icon" to menu which hides tray icon until the next time the app is manually launched ([#464](https://github.com/t1m0thyj/WinDynamicDesktop/issues/464))
* Added Estonian translation (thanks ST)
* Fixed error when applying settings and there is no active theme ([#457](https://github.com/t1m0thyj/WinDynamicDesktop/issues/457))
* Fixed theme not updating on displays that were connected when device is sleeping
* Fixed error when theme download is cancelled
* Fixed PowerShell scripts being invoked multiple times with the same arguments
* Fixed incorrect theme name shown in bold when there is no active theme

## `5.0.3`

* Fixed IndexOutOfRangeException when Select Theme dialog is opened and there are multiple monitors ([#446](https://github.com/t1m0thyj/WinDynamicDesktop/issues/446))

## `5.0.2`

**Note:** Version 5.0.0 had a bug causing automatic update checking to be disabled for fresh installs. If you want automatic updates, check that they are enabled in the system tray menu: More Options -> Check for updates once a week.

* Updated LocationIQ geocoding provider to stop using endpoint that will be deprecated on June 1
* Fixed display names shown in wrong order in theme dialog ([#420](https://github.com/t1m0thyj/WinDynamicDesktop/issues/420))
* Fixed background not updating when night mode is toggled ([#425](https://github.com/t1m0thyj/WinDynamicDesktop/issues/425))
* Fixed user-defined sunrise and sunset times being stored in wrong locale format ([#432](https://github.com/t1m0thyj/WinDynamicDesktop/issues/432))
* Added Azerbaijani and Hebrew translations (thanks Arzu and elie7han)

## `5.0.1`

* Fixed error when wallpaper changes and Change Lockscreen Image is enabled
* Fixed error when wallpaper changes and PowerShell scripts are enabled
* Disabled virtual desktop support for Windows 11 Insider builds ([#421](https://github.com/t1m0thyj/WinDynamicDesktop/issues/421))

## `5.0.0`

* Added support for setting separate background on multiple monitors
* Added support for syncing lockscreen image with desktop background
* Added time of day to preview for downloaded themes
* Updated from .NET Framework to .NET 6 which is bundled with the app (thanks @sungaila for helping with this)
* Added Amharic, Catalan, Javanese, Kazakh, Persian, Portuguese, and Thai translations (thanks Yonas, Joaquim, Raihan, Janat, Ali, João, and Chananthip)

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
