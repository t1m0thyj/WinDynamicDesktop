// This Source Code Form is subject to the terms of the Mozilla Public
// License, v. 2.0. If a copy of the MPL was not distributed with this
// file, You can obtain one at http://mozilla.org/MPL/2.0/.

using System;
using System.Timers;

namespace WinDynamicDesktop
{
    internal interface ISchedulerTimer : IDisposable
    {
        bool AutoReset { get; set; }
        double Interval { get; set; }
        event ElapsedEventHandler Elapsed;
        void Start();
        void Stop();
    }

    internal sealed class SchedulerTimer : Timer, ISchedulerTimer { }
}
