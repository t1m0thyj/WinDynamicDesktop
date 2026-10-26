// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.InteropServices;

namespace WinDynamicDesktop.COM
{
    // Internal interface layout reference:
    // https://github.com/MScholtes/PSVirtualDesktop/blob/master/VirtualDesktop.ps1
    internal static class VirtualDesktopWallpaperApi
    {
        private static readonly Guid CLSID_ImmersiveShell =
            new Guid("C2F03A33-21F5-47FA-B4BB-156362A2F239");
        private static readonly Guid CLSID_VirtualDesktopManagerInternal =
            new Guid("C5E0CDCA-7B6E-41B2-9FC4-D93975CC467B");

        internal static bool IsSupported => OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621);

        internal static bool TrySetWallpaperForAllDesktops(string imagePath)
        {
            if (!IsSupported)
            {
                return false;
            }

            object shell = null;
            object manager = null;
            try
            {
                Type shellType = Type.GetTypeFromCLSID(CLSID_ImmersiveShell, true);
                shell = Activator.CreateInstance(shellType);
                IServiceProvider serviceProvider = (IServiceProvider)shell;
                Guid service = CLSID_VirtualDesktopManagerInternal;
                Guid interfaceId = typeof(IVirtualDesktopManagerInternal22621).GUID;
                manager = serviceProvider.QueryService(ref service, ref interfaceId);

                using HString path = HString.Create(imagePath);
                if (Environment.OSVersion.Version.Build >= 26100)
                {
                    ((IVirtualDesktopManagerInternal26100)manager)
                        .UpdateWallpaperPathForAllDesktops(path);
                }
                else
                {
                    ((IVirtualDesktopManagerInternal22621)manager)
                        .UpdateWallpaperPathForAllDesktops(path);
                }

                return true;
            }
            catch (Exception exc)
            {
                LoggingHandler.LogMessage("Could not update wallpaper for all virtual desktops: {0}",
                    exc.ToString());
                LoggingHandler.LogError(exc);
                return false;
            }
            finally
            {
                if (manager != null && Marshal.IsComObject(manager))
                {
                    Marshal.ReleaseComObject(manager);
                }
                if (shell != null && Marshal.IsComObject(shell))
                {
                    Marshal.ReleaseComObject(shell);
                }
            }
        }

        [StructLayout(LayoutKind.Sequential)]
        internal readonly struct HString : IDisposable
        {
            private readonly IntPtr value;

            private HString(IntPtr value)
            {
                this.value = value;
            }

            internal static HString Create(string value)
            {
                Marshal.ThrowExceptionForHR(WindowsCreateString(value, value.Length, out IntPtr handle));
                return new HString(handle);
            }

            public void Dispose()
            {
                if (value != IntPtr.Zero)
                {
                    WindowsDeleteString(value);
                }
            }

            [DllImport("combase.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int WindowsCreateString([MarshalAs(UnmanagedType.LPWStr)] string sourceString,
                int length, out IntPtr hstring);

            [DllImport("combase.dll", CallingConvention = CallingConvention.StdCall)]
            private static extern int WindowsDeleteString(IntPtr hstring);
        }

        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("6D5140C1-7436-11CE-8034-00AA006009FA")]
        private interface IServiceProvider
        {
            [return: MarshalAs(UnmanagedType.IUnknown)]
            object QueryService(ref Guid service, ref Guid interfaceId);
        }

        // This private Windows interface changes between feature updates. Only the final method is invoked;
        // placeholders document and preserve the vtable slot used on Windows 11 22H2 and 23H2.
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("53F5CA0B-158F-4124-900C-057158060B27")]
        private interface IVirtualDesktopManagerInternal22621
        {
            void Slot00();
            void Slot01();
            void Slot02();
            void Slot03();
            void Slot04();
            void Slot05();
            void Slot06();
            void Slot07();
            void Slot08();
            void Slot09();
            void Slot10();
            void Slot11();
            void Slot12();
            void Slot13();
            void UpdateWallpaperPathForAllDesktops(HString path);
        }

        // Windows 11 24H2 added SwitchDesktopAndMoveForegroundView at slot 7.
        [ComImport]
        [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
        [Guid("53F5CA0B-158F-4124-900C-057158060B27")]
        private interface IVirtualDesktopManagerInternal26100
        {
            void Slot00();
            void Slot01();
            void Slot02();
            void Slot03();
            void Slot04();
            void Slot05();
            void Slot06();
            void Slot07();
            void Slot08();
            void Slot09();
            void Slot10();
            void Slot11();
            void Slot12();
            void Slot13();
            void Slot14();
            void UpdateWallpaperPathForAllDesktops(HString path);
        }
    }
}
