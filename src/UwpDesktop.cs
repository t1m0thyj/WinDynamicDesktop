// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DesktopBridge;

namespace WinDynamicDesktop
{
    abstract class PlatformHelper
    {
        public abstract string GetLocalFolder();

        public abstract void CheckStartOnBoot();

        public abstract void ToggleStartOnBoot();

        public abstract void OpenUpdateLink();
    }

    class UwpDesktop
    {
        private static bool? _isRunningAsUwp;
        private static PlatformHelper helper;

        public static bool IsRunningAsUwp()
        {
            if (!_isRunningAsUwp.HasValue)
            {
                Helpers helpers = new Helpers();
                _isRunningAsUwp = helpers.IsRunningAsUwp();
            }

            return _isRunningAsUwp.Value;
        }

        public static PlatformHelper GetHelper()
        {
            if (helper == null)
            {
                if (!IsRunningAsUwp())
                {
                    helper = new DesktopHelper();
                }
                else
                {
                    helper = new UwpHelper();
                }
            }

            return helper;
        }
    }
}
