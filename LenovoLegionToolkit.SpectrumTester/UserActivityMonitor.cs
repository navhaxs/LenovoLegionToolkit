using System.Runtime.InteropServices;

namespace SpectrumBacklightTimeout
{
    public sealed class UserActivityMonitor {
        public UserActivityMonitor() {
            _lastInputInfo.CbSize = Marshal.SizeOf(_lastInputInfo);
        }

        /// <summary>Determines the time of the last user activity (any mouse activity or key press).</summary>
        /// <returns>The time of the last user activity.</returns>

        public DateTime LastActivity => DateTime.Now - this.InactivityPeriod;

        /// <summary>The amount of time for which the user has been inactive (no mouse activity or key press).</summary>

        public TimeSpan InactivityPeriod {
            get {
                GetLastInputInfo(ref _lastInputInfo);
                uint elapsedMilliseconds = (uint)Environment.TickCount - _lastInputInfo.DwTime;
                elapsedMilliseconds = Math.Min(elapsedMilliseconds, int.MaxValue);
                return TimeSpan.FromMilliseconds(elapsedMilliseconds);
            }
        }

        /// <summary>Struct used by Windows API function GetLastInputInfo()</summary>
        struct LastInputInfo {
            public int CbSize;
            public uint DwTime;
        }

        [DllImport("user32.dll")]
        [DefaultDllImportSearchPaths(DllImportSearchPath.System32)]
        [return: MarshalAs(UnmanagedType.Bool)]

        static extern bool GetLastInputInfo(ref LastInputInfo plii);

        LastInputInfo _lastInputInfo;
    }
}
