# WinDynamicDesktop
Port of macOS Mojave Dynamic Desktop feature to Windows 10, available on GitHub and the Microsoft Store

[![GitHub releases](https://img.shields.io/github/downloads/t1m0thyj/WinDynamicDesktop/total.svg)](https://github.com/t1m0thyj/WinDynamicDesktop/releases)
[![Gitter chat](https://badges.gitter.im/gitterHQ/gitter.png)](https://gitter.im/t1m0thyj/WinDynamicDesktop)
[![Donate](https://img.shields.io/badge/Donate-PayPal-green.svg)](https://www.paypal.com/cgi-bin/webscr?cmd=_donations&business=H8ZZXM9ABRJFU)

<a href='//www.microsoft.com/store/apps/9NM8N7DQ3Z5F?ocid=badge'><img src='https://assets.windowsphone.com/85864462-9c82-451e-9355-a3d5f874397a/English_get-it-from-MS_InvariantCulture_Default.png' alt='Microsoft Store' width='160'/></a>

## Getting Started

WinDynamicDesktop should run on any version of Windows that has .NET Framework 4.5 or newer installed. If you are using Windows 7 or Vista and your version of .NET Framework is too old, you can install a newer one from [here](https://www.microsoft.com/net/download).

The first time you run WinDynamicDesktop, it will automatically download the wallpapers for the macOS Mojave themes (Mojave Desert and Solar Gradients) and extract them to your disk. I have not included the files directly in this repository for copyright reasons. The app will ask you to choose a theme and you can also import custom themes that have been created by other users.

You will also need to input your location when running the app for the first time. This location is not used for any purpose other than to determine the times of sunrise and sunset where you live. If you are using the Microsoft Store version of the app, you can opt to use the Windows location service instead which will update automatically.

After you have chosen a theme and entered your location, the app will minimize to your system tray and run in the background. Right-clicking on the system tray icon opens a menu with options to change the theme, update the location, refresh the wallpaper manually if necessary, or exit the program. You can also choose to start WinDynamicDesktop when Windows boots or enable dark mode from this menu.

## Frequently Asked Questions

### Why did you develop this?

When the Dynamic Desktop feature was announced for macOS Mojave which shifts through 16 images of the same desert scene taken at different times of day, I wanted to make a Windows program that would do the same thing. Windows 10 natively supports cycling through multiple wallpapers, but not based on the times of sunrise and sunset. WinDynamicDesktop adds that feature to the Windows desktop.

### Can the Windows 10 theme be changed too?

There is an option to enable this in the "More Options" section of the system tray menu. Unfortunately its functionality is limited by the way the system theme setting works in Windows 10. In Windows 10 version 1809 which added a dark theme to File Explorer, it is necessary to restart Explorer before the system theme will change in it (this should be fixed in the next release of Windows). Also the theme for Microsoft Edge is separate from the Windows 10 theme and cannot be controlled by WinDynamicDesktop.

### Can the lockscreen image be changed too?

This is a commonly requested feature to mimic the behavior of macOS and many Linux distros where the lockscreen image is the same as the desktop wallpaper. If you are using the Microsoft Store version of WinDynamicDesktop, you can enable changing the lockscreen image as an experimental feature by editing the `settings.conf` file which is in the same folder as the EXE. Change the setting `"changeLockScreen":false` to `"changeLockScreen":true` (or add it if it doesn't exist), and make sure in the Windows 10 settings app that *Personalization* -> *Lock screen* -> *Background* is set to *Picture*. In order for this to work reliably on Windows, it would be ideal if Microsoft added an option in a future version of Windows to make the lockscreen image mirror the desktop wallpaper.

### How can I customize the images?

You are not limited to the Mojave themes that come preinstalled with the app. Custom themes created by the community can be downloaded [here](https://github.com/t1m0thyj/WinDynamicDesktop/wiki/Community-created-themes). You can also create your own theme that uses whatever wallpaper images you want, by following the instructions in [this tutorial](https://github.com/t1m0thyj/WinDynamicDesktop/wiki/Creating-custom-themes).

### How can I hide the tray icon?

If you want to run the app silently with no icon in the system tray, you can do this by editing the `settings.conf` file which is in the same folder as the EXE. Change the setting `"hideTrayIcon":false` to `"hideTrayIcon":true` (or add it if it doesn't exist), then restart the app.

### How can I change the folder where config files are stored?

If you want the app to store its settings and theme files in a different folder from the default, you can create a file `WinDynamicDesktop.pth` in the same folder as the EXE. Edit the file and add the config path you want to use on the first line.

## Legal and Privacy Stuff
I do not own the wallpaper images used by WinDynamicDesktop, they belong to Apple. The icon used in this program was made by [Roundicons](https://www.flaticon.com/authors/roundicons) from [flaticon.com](https://www.flaticon.com/) and is licensed by [CC 3.0 BY](http://creativecommons.org/licenses/by/3.0/).

When you enter your location, WinDynamicDesktop uses the [LocationIQ API](https://locationiq.org/) to convert it to latitude and longitude. The Microsoft Store version uses the Windows location API instead if permission is granted. Location data is anonymous and never saved without your consent.
