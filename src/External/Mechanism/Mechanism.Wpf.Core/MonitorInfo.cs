using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
//using System.Windows;
//using static Mechanism.Wpf.Core.MonitorInfo.NativeMethods;
using static Mechanism.Wpf.Core.NativeMethods;

namespace Mechanism.Wpf.Core
{
    /// <summary>
    /// This class and Mechanism.Wpf.Core.Windows.AppBarWindow were derived from https://github.com/mgaffigan/WpfAppBar
    /// </summary>
    public class MonitorInfo : IEquatable<MonitorInfo>
    {
        /// <summary>
        /// Gets the bounds of the viewport.
        /// </summary>
        public Rect ViewportBounds { get; }

        /// <summary>
        /// Gets the bounds of the work area.
        /// </summary>
        public Rect WorkAreaBounds { get; }

        /// <summary>
        /// Gets a value that determines if the monitor is the primary monitor or not.
        /// </summary>
        public Boolean IsPrimary { get; }

        /// <summary>
        /// Gets the device ID of the monitor.
        /// </summary>
        public String DeviceId { get; }

        internal MonitorInfo(MonitorInfoEx mex)
        {
            this.ViewportBounds = mex.rcMonitor;
            this.WorkAreaBounds = (Rect)mex.rcWork;
            this.IsPrimary = mex.dwFlags.HasFlag(MonitorInfoF.Primary);
            this.DeviceId = mex.szDevice;
        }

        /// <summary>
        /// Gets a collection of all monitors.
        /// </summary>
        public static ObservableCollection<MonitorInfo> AllMonitors
        {
            get
            {
                var monitors = new ObservableCollection<MonitorInfo>();
                MonitorEnumDelegate callback = delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData)
                {
                    MonitorInfoEx mi = new MonitorInfoEx
                    {
                        cbSize = Marshal.SizeOf(typeof(MonitorInfoEx))
                    };
                    if (!GetMonitorInfo(hMonitor, ref mi))
                    {
                        throw new System.ComponentModel.Win32Exception();
                    }

                    monitors.Add(new MonitorInfo(mi));
                    return true;
                };

                EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, callback, IntPtr.Zero);

                return monitors;
            }
        }

        public override String ToString() => DeviceId;

        public override Boolean Equals(Object obj) => Equals(obj as MonitorInfo);

        public override Int32 GetHashCode() => DeviceId.GetHashCode();

        public Boolean Equals(MonitorInfo other) => this.DeviceId == other?.DeviceId;

        public static Boolean operator ==(MonitorInfo a, MonitorInfo b)
        {
            if (ReferenceEquals(a, b))
            {
                return true;
            }

            if (a is null)
            {
                return false;
            }

            return a.Equals(b);
        }

        public static Boolean operator !=(MonitorInfo a, MonitorInfo b) => !(a == b);
    }
}
