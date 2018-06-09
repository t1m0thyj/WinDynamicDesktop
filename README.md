# WinDynamicDesktop
Port of macOS Mojave Dynamic Desktop feature to Windows 10

## Why did you develop this?

When the Dynamic Desktop feature was announced for macOS Mojave which shifts through 16 images of the same desert scene taken at different times of day, I wanted to write a Windows program that would do the same thing. Windows has the ability built-in to cycle through different wallpapers, but only at regular intervals, not based on the times of sunrise and sunset. This program adds that feature for the Mojave wallpapers to the Windows desktop.

## How do I use this?

The first time you run WinDynamicDesktop, it will automatically download the macOS Mojave wallpapers from [here](https://files.rb.gd/mojave_dynamic.zip) and extract them to your disk. I have not included the files directly in this repository for copyright reasons.

You will also need to input your location when running the program for the first time. This location is not used for any purpose other than to determine the times of sunrise and sunset where you live.

After entering your location, the program will minimize to your system tray and run in the background. If you ever want to change the location, right-click on the system tray icon and select Settings. The program can also be exited via the right-click menu of the system tray icon.

The program does not yet have an option built-in to automatically start when Windows boots. To make it do this, create a shortcut to the EXE in the following folder: `%APPDATA%\Microsoft\Windows\Start Menu\Programs\Startup`.

## Legal and privacy stuff
I do not own the wallpaper pictures used by WinDynamicDesktop, they belong to Apple.

When you enter your location, WinDynamicDesktop uses the [LocationIQ service](https://locationiq.org/) to convert your location to latitude and longitude. Your location info is never sent anywhere without your consent.

To get sunrise and sunset times, WinDynamicDesktop uses the free API available at [sunrise-sunset.org](https://sunrise-sunset.org/).
