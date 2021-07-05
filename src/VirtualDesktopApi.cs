// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Runtime.InteropServices;

namespace WinDynamicDesktop
{
    // https://github.com/Grabacr07/VirtualDesktop/pull/57
    [ComImport]
    [Guid("92ca9dcd-5622-4bba-a805-5e9f541bd8c9")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IObjectArray
    {
        uint GetCount();
        void GetAt(uint iIndex, ref Guid riid, [Out, MarshalAs(UnmanagedType.Interface)] out object ppvObject);
    }

    [ComImport]
    [Guid("6d5140c1-7436-11ce-8034-00aa006009fa")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IServiceProvider
    {
        [return: MarshalAs(UnmanagedType.IUnknown)]
        object QueryService(ref Guid service, ref Guid riid);
    }

    [ComImport]
    [Guid("536d3495-b208-4cc9-ae26-de8111275bf8")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IVirtualDesktop
    {
        void IsViewVisible();
        Guid GetId();
        void Proc5();
        void GetName();
        void GetWallpaperPath();
    }

    [ComImport]
    [Guid("b2f925b9-5a0f-4d2e-9f4d-2b1507593c10")]
    [InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
    public interface IVirtualDesktopManagerInternal
    {
        void GetCount();
        void MoveViewToDesktop();
        void CanViewMoveDesktops();
        IVirtualDesktop GetCurrentDesktop(IntPtr hWndOrMon);
        IObjectArray GetDesktops(IntPtr hWndOrMon);
        void GetAdjacentDesktop();
        void SwitchDesktop();
        void CreateDesktopW();
        void MoveDesktop();
        void RemoveDesktop();
        void FindDesktop();
        void Proc14();
        void SetName();
        void SetWallpaperPath(IVirtualDesktop pDesktop, [MarshalAs(UnmanagedType.HString)] string chPath);
        void Proc17();
        void Proc18();
        void Proc19();
        void Proc20();
    }

    public class ImmersiveShellWrapper
    {
        static readonly Guid CLSID_ImmersiveShell = new Guid("c2f03a33-21f5-47fa-b4bb-156362a2f239");
        static readonly Guid CLSID_VirtualDesktopManagerInternal = new Guid("c5e0cdca-7b6e-41b2-9fc4-d93975cc467b");

        public static IVirtualDesktopManagerInternal GetVirtualDesktopManager()
        {
            IServiceProvider shell = GetImmersiveShell();
            return (IVirtualDesktopManagerInternal)shell.QueryService(CLSID_VirtualDesktopManagerInternal,
                typeof(IVirtualDesktopManagerInternal).GUID);
        }

        private static IServiceProvider GetImmersiveShell()
        {
            Type typeImmersiveShell = Type.GetTypeFromCLSID(CLSID_ImmersiveShell);
            return (IServiceProvider)Activator.CreateInstance(typeImmersiveShell);
        }
    }

    public class VirtualDesktopApi
    {
        private static IVirtualDesktopManagerInternal manager;

        public static void SetWallpaper(string imagePath)
        {
            for (int attempts = 0; attempts < 2; attempts++)
            {
                if (manager == null || attempts > 0)
                {
                    manager = ImmersiveShellWrapper.GetVirtualDesktopManager();
                }

                try
                {
                    UnsafeSetWallpaper(imagePath);
                    break;
                }
                catch (COMException)
                {
                    continue;
                }
            }
        }

        private static void UnsafeSetWallpaper(string imagePath)
        {
            Guid currentDesktopId = manager.GetCurrentDesktop(IntPtr.Zero).GetId();
            IObjectArray objectArray = manager.GetDesktops(IntPtr.Zero);

            for (uint i = 0u; i < objectArray.GetCount(); i++)
            {
                objectArray.GetAt(i, typeof(IVirtualDesktop).GUID, out object virtualDesktop);
                if ((virtualDesktop as IVirtualDesktop).GetId() != currentDesktopId)
                {
                    manager.SetWallpaperPath((IVirtualDesktop)virtualDesktop, imagePath);
                }
            }
        }
    }
}
