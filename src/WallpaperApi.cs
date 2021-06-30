// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace WinDynamicDesktop
{
    // Code based on https://www.neowin.net/forum/topic/1035559-fade-effect-when-changing-wallpaper/
    public struct WALLPAPEROPT
    {
        public static readonly int SizeOf = Marshal.SizeOf(typeof(WALLPAPEROPT));
    }

    public enum WallPaperStyle : int
    {
        WPSTYLE_CENTER = 0,
        WPSTYLE_TILE = 1,
        WPSTYLE_STRETCH = 2,
        WPSTYLE_MAX = 3
    }

    [Flags]
    public enum AD_Apply : int
    {
        SAVE = 0x00000001,
        HTMLGEN = 0x00000002,
        REFRESH = 0x00000004,
        ALL = SAVE | HTMLGEN | REFRESH,
        FORCE = 0x00000008,
        BUFFERED_REFRESH = 0x00000010,
        DYNAMICREFRESH = 0x00000020
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COMPONENTSOPT
    {
        public static readonly int SizeOf = Marshal.SizeOf(typeof(COMPONENTSOPT));
        public int dwSize;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fEnableComponents;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fActiveDesktop;
    }

    [Flags]
    public enum CompItemState : int
    {
        NORMAL = 0x00000001,
        FULLSCREEN = 00000002,
        SPLIT = 0x00000004,
        VALIDSIZESTATEBITS = NORMAL | SPLIT | FULLSCREEN,
        VALIDSTATEBITS = NORMAL | SPLIT | FULLSCREEN | unchecked((int)0x80000000) | 0x40000000
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COMPSTATEINFO
    {
        public static readonly int SizeOf = Marshal.SizeOf(typeof(COMPSTATEINFO));
        public int dwSize;
        public int iLeft;
        public int iTop;
        public int dwWidth;
        public int dwHeight;
        public CompItemState dwItemState;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct COMPPOS
    {
        public const int COMPONENT_TOP = 0x3FFFFFFF;
        public const int COMPONENT_DEFAULT_LEFT = 0xFFFF;
        public const int COMPONENT_DEFAULT_TOP = 0xFFFF;
        public static readonly int SizeOf = Marshal.SizeOf(typeof(COMPPOS));

        public int dwSize;
        public int iLeft;
        public int iTop;
        public int dwWidth;
        public int dwHeight;
        public int izIndex;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fCanResize;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fCanResizeX;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fCanResizeY;
        public int iPreferredLeftPercent;
        public int iPreferredTopPercent;
    }

    public enum CompType : int
    {
        HTMLDOC = 0,
        PICTURE = 1,
        WEBSITE = 2,
        CONTROL = 3,
        CFHTML = 4
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 8)]
    public struct COMPONENT
    {
        private const int INTERNET_MAX_URL_LENGTH = 2084;
        public static readonly int SizeOf = Marshal.SizeOf(typeof(COMPONENT));

        public int dwSize;
        public int dwID;
        public CompType iComponentType;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fChecked;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fDirty;
        [MarshalAs(UnmanagedType.Bool)]
        public bool fNoScroll;
        public COMPPOS cpPos;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 260)]
        public string wszFriendlyName;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = INTERNET_MAX_URL_LENGTH)]
        public string wszSource;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = INTERNET_MAX_URL_LENGTH)]
        public string wszSubscribedURL;

        public int dwCurItemState;
        public COMPSTATEINFO csiOriginal;
        public COMPSTATEINFO csiRestored;
    }

    public enum DtiAddUI : int
    {
        DEFAULT = 0x00000000,
        DISPSUBWIZARD = 0x00000001,
        POSITIONITEM = 0x00000002,
    }

    [Flags]
    public enum ComponentModify : int
    {
        TYPE = 0x00000001,
        CHECKED = 0x00000002,
        DIRTY = 0x00000004,
        NOSCROLL = 0x00000008,
        POS_LEFT = 0x00000010,
        POS_TOP = 0x00000020,
        SIZE_WIDTH = 0x00000040,
        SIZE_HEIGHT = 0x00000080,
        POS_ZINDEX = 0x00000100,
        SOURCE = 0x00000200,
        FRIENDLYNAME = 0x00000400,
        SUBSCRIBEDURL = 0x00000800,
        ORIGINAL_CSI = 0x00001000,
        RESTORED_CSI = 0x00002000,
        CURITEMSTATE = 0x00004000,
        ALL = TYPE | CHECKED | DIRTY | NOSCROLL | POS_LEFT | SIZE_WIDTH | SIZE_HEIGHT | POS_ZINDEX | SOURCE |
            FRIENDLYNAME | POS_TOP | SUBSCRIBEDURL | ORIGINAL_CSI | RESTORED_CSI | CURITEMSTATE
    }

    [Flags]
    public enum AddURL : int
    {
        SILENT = 0x0001
    }

    [ComImport]
    [Guid("F490EB00-1240-11D1-9888-006097DEACF9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IActiveDesktop
    {
        [PreserveSig]
        int ApplyChanges(AD_Apply dwFlags);
        [PreserveSig]
        int GetWallpaper([MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pwszWallpaper, int cchWallpaper,
            int dwReserved);
        [PreserveSig]
        int SetWallpaper([MarshalAs(UnmanagedType.LPWStr)] string pwszWallpaper, int dwReserved);
        [PreserveSig]
        int GetWallpaperOptions(ref WALLPAPEROPT pwpo, int dwReserved);
        [PreserveSig]
        int SetWallpaperOptions(ref WALLPAPEROPT pwpo, int dwReserved);
        [PreserveSig]
        int GetPattern([MarshalAs(UnmanagedType.LPWStr)] System.Text.StringBuilder pwszPattern, int cchPattern,
            int dwReserved);
        [PreserveSig]
        int SetPattern([MarshalAs(UnmanagedType.LPWStr)] string pwszPattern, int dwReserved);
        [PreserveSig]
        int GetDesktopItemOptions(ref COMPONENTSOPT pco, int dwReserved);
        [PreserveSig]
        int SetDesktopItemOptions(ref COMPONENTSOPT pco, int dwReserved);
        [PreserveSig]
        int AddDesktopItem(ref COMPONENT pcomp, int dwReserved);
        [PreserveSig]
        int AddDesktopItemWithUI(IntPtr hwnd, ref COMPONENT pcomp, DtiAddUI dwFlags);
        [PreserveSig]
        int ModifyDesktopItem(ref COMPONENT pcomp, ComponentModify dwFlags);
        [PreserveSig]
        int RemoveDesktopItem(ref COMPONENT pcomp, int dwReserved);
        [PreserveSig]
        int GetDesktopItemCount(out int lpiCount, int dwReserved);
        [PreserveSig]
        int GetDesktopItem(int nComponent, ref COMPONENT pcomp, int dwReserved);
        [PreserveSig]
        int GetDesktopItemByID(IntPtr dwID, ref COMPONENT pcomp, int dwReserved);
        [PreserveSig]
        int GenerateDesktopItemHtml([MarshalAs(UnmanagedType.LPWStr)] string pwszFileName, ref COMPONENT pcomp,
            int dwReserved);
        [PreserveSig]
        int AddUrl(IntPtr hwnd, [MarshalAs(UnmanagedType.LPWStr)] string pszSource, ref COMPONENT pcomp,
            AddURL dwFlags);
        [PreserveSig]
        int GetDesktopItemBySource([MarshalAs(UnmanagedType.LPWStr)] string pwszSource, ref COMPONENT pcomp,
            int dwReserved);
    }

    public class ActiveDesktopWrapper
    {
        static readonly Guid CLSID_ActiveDesktop = new Guid("{75048700-EF1F-11D0-9888-006097DEACF9}");

        public static IActiveDesktop GetActiveDesktop()
        {
            Type typeActiveDesktop = Type.GetTypeFromCLSID(CLSID_ActiveDesktop);
            return (IActiveDesktop)Activator.CreateInstance(typeActiveDesktop);
        }
    }

    public class WallpaperApi
    {
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
        private static extern IntPtr FindWindow(string lpClassName, string lpWindowName);

        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        private static extern int SendMessageTimeout(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam, uint fuFlags,
            uint uTimeout, out IntPtr result);

        public static void EnableTransitions()
        {
            IntPtr result = IntPtr.Zero;
            SendMessageTimeout(FindWindow("Progman", null), 0x52c, IntPtr.Zero, IntPtr.Zero, 0, 500, out result);
        }

        public static void SyncVirtualDesktops(string imagePath)
        {
            if (UwpDesktop.IsVirtualDesktopSupported())
            {
                VirtualDesktopApi.SetWallpaper(imagePath);
            }
        }

        public static void SetWallpaper(string imagePath)
        {
            EnableTransitions();

            ThreadStart threadStarter = () =>
            {
                IActiveDesktop _activeDesktop = ActiveDesktopWrapper.GetActiveDesktop();
                _activeDesktop.SetWallpaper(imagePath, 0);
                _activeDesktop.ApplyChanges(AD_Apply.ALL | AD_Apply.FORCE);

                Marshal.ReleaseComObject(_activeDesktop);
                SyncVirtualDesktops(imagePath);
            };
            Thread thread = new Thread(threadStarter);
            thread.SetApartmentState(ApartmentState.STA);  // Set the thread to STA (required!)
            thread.Start();
            thread.Join(2000);
        }
    }
}
