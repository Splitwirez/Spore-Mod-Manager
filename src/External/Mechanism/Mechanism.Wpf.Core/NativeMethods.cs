using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Mechanism.Wpf.Core
{
    public static class NativeMethods
    {
        [DllImport("dwmapi.dll", EntryPoint = "#127")]
        internal static extern void DwmGetColorizationParameters(ref DwmColorizationParams param);

        public struct DwmColorizationParams
        {
            public UInt32 ColorizationColor,
                ColorizationAfterglow,
                ColorizationColorBalance,
                ColorizationAfterglowBalance,
                ColorizationBlurBalance,
                ColorizationGlassReflectionIntensity,
                ColorizationOpaqueBlend;
        }

        ///Extensions
        [DllImport("gdi32.dll")]
        internal static extern bool DeleteObject(IntPtr hObject);

        ///CompositingWindow
        /*[DllImport("user32.dll")]
            private static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

            [StructLayout(LayoutKind.Sequential)]
            internal struct WindowCompositionAttributeData
            {
                public WindowCompositionAttribute Attribute;
                public IntPtr Data;
                public int SizeOfData;
            }

            internal enum WindowCompositionAttribute
            {
                WCA_ACCENT_POLICY = 19
            }

            internal enum AccentState
            {
                ACCENT_DISABLED = 0,
                ACCENT_ENABLE_GRADIENT = 1,
                ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
                ACCENT_ENABLE_BLURBEHIND = 3,
                ACCENT_INVALID_STATE = 4
            }*/

        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmExtendFrameIntoClientArea(IntPtr hwnd, ref MARGINS margins);

        [StructLayout(LayoutKind.Sequential)]
        public struct MARGINS
        {
            public int leftWidth;
            public int rightWidth;
            public int topHeight;
            public int bottomHeight;
        }

        [DllImport("dwmapi.dll", PreserveSig = true)]
        public static extern int DwmSetWindowAttribute(IntPtr hwnd, Int32 attr, ref Int32 attrValue, Int32 attrSize);

        [Flags]
        public enum DwmWindowAttribute
        {
            NCRenderingEnabled = 1,
            NCRenderingPolicy,
            TransitionsForceDisabled,
            AllowNCPaint,
            CaptionButtonBounds,
            NonClientRtlLayout,
            ForceIconicRepresentation,
            Flip3DPolicy,
            ExtendedFrameBounds,
            HasIconicBitmap,
            DisallowPeek,
            ExcludedFromPeek,
            Last
        }

        [Flags]
        public enum DwmNCRenderingPolicy
        {
            UseWindowStyle,
            Disabled,
            Enabled,
            Last
        }

        [DllImport("user32.dll")]
        public static extern int GetSystemMetrics(int nIndex);

        [DllImport("dwmapi.dll")]
        public static extern int DwmGetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, out RECT pvAttribute, int cbAttribute);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

        public static Int32 ScSizeHtLeft = 1;
        public static Int32 ScSizeHtRight = 2;
        public static Int32 ScSizeHtTop = 3;
        public static Int32 ScSizeHtTopLeft = 4;
        public static Int32 ScSizeHtTopRight = 5;
        public static Int32 ScSizeHtBottom = 6;
        public static Int32 ScSizeHtBottomLeft = 7;
        public static Int32 ScSizeHtBottomRight = 8;

        //public static Int32 WmSysCommand = 0x0112;

        [return: MarshalAs(UnmanagedType.Bool)]
        [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);

        public static uint WmClose = 0x0010;

        [DllImport("user32.dll")]
        internal static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

        [StructLayout(LayoutKind.Sequential)]
        internal struct WindowCompositionAttributeData
        {
            public WindowCompositionAttribute Attribute;
            public IntPtr Data;
            public int SizeOfData;
        }

        internal enum WindowCompositionAttribute
        {
            // ...
            WCA_ACCENT_POLICY = 19
            // ...
        }

        /*internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_INVALID_STATE = 4
        }*/

        internal enum AccentState
        {
            ACCENT_DISABLED = 0,
            ACCENT_ENABLE_GRADIENT = 1,
            ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
            ACCENT_ENABLE_BLURBEHIND = 3,
            ACCENT_ENABLE_ACRYLICBLURBEHIND = 4,
            ACCENT_INVALID_STATE = 5
        }

        [StructLayout(LayoutKind.Sequential)]
        internal struct AccentPolicy
        {
            public AccentState AccentState;
            public int AccentFlags;
            public int GradientColor;
            public int AnimationId;
        }


        [DllImport("dwmapi.dll")]
        static extern Int32 DwmIsCompositionEnabled(out Boolean enabled);

        public static bool DwmIsCompositionEnabled()
        {
            if (Environment.OSVersion.Version.Major >= 6)
            {
                DwmIsCompositionEnabled(out bool returnValue);
                return returnValue;
            }
            else
                return false;
        }

        [DllImport("user32.dll", SetLastError = true)]
        public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);


        public static IntPtr SetWindowLong(IntPtr hWnd, Int32 nIndex, Int32 dwNewLong)/* => IntPtr.Size == 8
        ? SetWindowLongPtr64(hWnd, nIndex, dwNewLong)
        : SetWindowLong32(hWnd, nIndex, dwNewLong);*/
        {
            if (IntPtr.Size == 4)
                return SetWindowLong32(hWnd, nIndex, dwNewLong);
            else
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
        }

        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        static extern IntPtr SetWindowLong32(IntPtr hWnd, Int32 nIndex, Int32 dwNewLong);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, Int32 nIndex, Int32 dwNewLong);


        public const Int32
            GwlStyle = -16,
            GwlExstyle = -20;

        public const Int32
            WsCaption = 0x00C00000,
            WsBorder = 0x00800000,
            WsSizeBox = 0x00040000;

        public const Int32
            WsExToolwindow = 0x00000080,
            WsExTransparent = 0x00000020,
            WsExNoActivate = 0x08000000;


        public static IntPtr GetWindowLong(IntPtr hWnd, Int32 nIndex)/* => IntPtr.Size == 8
        ? GetWindowLongPtr64(hWnd, nIndex)
        : GetWindowLongPtr32(hWnd, nIndex);*/
        {
            if (IntPtr.Size == 4)
                return GetWindowLong32(hWnd, nIndex);
            else
                return GetWindowLongPtr64(hWnd, nIndex);
        }

        [DllImport("user32.dll", EntryPoint = "GetWindowLong", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindowLong32(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr", CharSet = CharSet.Auto)]
        public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);


        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetWindowRect(IntPtr hWnd, out RECT lpRect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;        // x position of upper-left corner
            public int Top;         // y position of upper-left corner
            public int Right;       // x position of lower-right corner
            public int Bottom;      // y position of lower-right corner
        }


        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateRectRgn(int nLeftRect, int nTopRect, int nRightRect, int nBottomRect);


        [DllImport("user32.dll")]
        public static extern int GetWindowRgn(IntPtr hWnd, IntPtr hRgn);

        public enum RegionFlags
        {
            ERROR = 0,
            NULLREGION = 1,
            SIMPLEREGION = 2,
            COMPLEXREGION = 3,
        }


        [DllImport("dwmapi.dll")]
        public static extern void DwmEnableBlurBehindWindow(IntPtr hwnd, ref DWM_BLURBEHIND blurBehind);

        [StructLayout(LayoutKind.Sequential)]
        public struct DWM_BLURBEHIND
        {
            public DWM_BB dwFlags;
            public bool fEnable;
            public IntPtr hRgnBlur;
            public bool fTransitionOnMaximized;

            public DWM_BLURBEHIND(bool enabled)
            {
                fEnable = enabled ? true : false;
                hRgnBlur = IntPtr.Zero;
                fTransitionOnMaximized = true;
                dwFlags = DWM_BB.Enable;
            }

            public System.Drawing.Region Region
            {
                get { return System.Drawing.Region.FromHrgn(hRgnBlur); }
            }

            public bool TransitionOnMaximized
            {
                get { return fTransitionOnMaximized; }
                set
                {
                    fTransitionOnMaximized = value ? true : false;
                    dwFlags |= DWM_BB.TransitionMaximized;
                }
            }

            public void SetRegion(System.Drawing.Graphics graphics, System.Drawing.Region region)
            {
                hRgnBlur = region.GetHrgn(graphics);
                dwFlags |= DWM_BB.BlurRegion;
            }
        }

        [Flags]
        public enum DWM_BB
        {
            Enable = 1,
            BlurRegion = 2,
            TransitionMaximized = 4
        }


        ///MonitorInfo
        public delegate Boolean MonitorEnumDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref Rect lprcMonitor, IntPtr dwData);

        [DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
        public static extern Boolean GetMonitorInfo(IntPtr hMonitor, ref MonitorInfoEx lpmi);

        const int MONITOR_DEFAULTTONEAREST = 2;

        [DllImport("user32.dll")]
        static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

        public static MonitorInfoEx MonitorFromHandle(IntPtr hwnd) =>
            Marshal.PtrToStructure<MonitorInfoEx>(MonitorFromWindow(hwnd, MONITOR_DEFAULTTONEAREST));

        [DllImport("user32.dll")]
        public static extern Boolean EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, MonitorEnumDelegate lpfnEnum, IntPtr dwData);

        private const Int32 CchDeviceName = 32;

        [Flags]
        public enum MonitorInfoF
        {
            Primary = 0x1
        }

        [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
        public struct MonitorInfoEx
        {
            public Int32 cbSize;
            public Rect rcMonitor;
            public Rect rcWork;
            public MonitorInfoF dwFlags;
            [MarshalAs(UnmanagedType.ByValTStr, SizeConst = CchDeviceName)]
            public String szDevice;
        }

        public enum ABM
        {
            New = 0,
            Remove,
            QueryPos,
            SetPos,
            GetState,
            GetTaskbarPos,
            Activate,
            GetAutoHideBar,
            SetAutoHideBar,
            WindowPosChanged,
            SetState
        }

        public enum ABN
        {
            StateChange = 0,
            PosChanged,
            FullScreenApp,
            WindowArrange
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct WindowPos
        {
            public IntPtr hwnd;
            public IntPtr hwndInsertAfter;
            public Int32 x;
            public Int32 y;
            public Int32 cx;
            public Int32 cy;
            public Int32 flags;

            public System.Windows.Rect Bounds
            {
                get { return new System.Windows.Rect(x, y, cx, cy); }
                set
                {
                    x = (Int32)value.X;
                    y = (Int32)value.Y;
                    cx = (Int32)value.Width;
                    cy = (Int32)value.Height;
                }
            }
        }

        public const Int32
            SwpNoSize = 0x0001,
            SwpNoMove = 0x0002,
            SwpNoZOrder = 0x0004,
            SwpNoActivate = 0x0010;

        public const Int32
            WmActivate = 0x0006,
            WmMove = 0x0003,
            WmSize = 0x0005,
            WmSysCommand = 0x0112,
            WmWindowPosChanging = 0x0046,
            WmWindowPosChanged = 0x0047;

        public const Int32
            ScMove = 0xF010;

        [DllImport("shell32.dll", ExactSpelling = true)]
        public static extern UInt32 SHAppBarMessage(ABM dwMessage, ref AppBarData pData);

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public Rect(Int32 left, Int32 top, Int32 right, Int32 bottom)
            {
                Left = left;
                Top = top;
                Right = right;
                Bottom = bottom;
            }

            public Int32 Left;
            public Int32 Top;
            public Int32 Right;
            public Int32 Bottom;
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct AppBarData
        {
            public Int32 cbSize;
            public IntPtr hWnd;
            public Int32 uCallbackMessage;
            public Int32 uEdge;
            public Rect rc;
            public IntPtr lParam;
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        public static extern Int32 RegisterWindowMessage(String msg);

        /*[DllImport("user32.dll", CharSet = CharSet.Auto)]
        public static extern IntPtr SendMessage(IntPtr hWnd, Int32 Msg, Int32 wParam, Int32 lParam);*/

        /*public static Rect SysWinRectToNativeRect(SysWinRect rect)
        {
            return new Rect((int)rect.Left, (int)rect.Top, (int)rect.Right, (int)rect.Bottom);
        }*/

        [DllImport("User32")]
        public static extern IntPtr GetWindow(IntPtr hWnd, uint wCmd);

        [DllImport("user32.dll")]
        public static extern IntPtr GetWindowDC(IntPtr hWnd);
        [DllImport("user32.dll")]
        public static extern IntPtr ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("gdi32.dll")]
        public static extern bool BitBlt(IntPtr hObject, int nXDest, int nYDest, int nWidth, int nHeight, IntPtr hObjectSource, int nXSrc, int nYSrc, int dwRop);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleBitmap(IntPtr hDC, int nWidth, int nHeight);
        [DllImport("gdi32.dll")]
        public static extern IntPtr CreateCompatibleDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern bool DeleteDC(IntPtr hDC);
        [DllImport("gdi32.dll")]
        public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

        [StructLayout(LayoutKind.Sequential)]
        public struct POINT
        {
            public int X;
            public int Y;

            public POINT(int x, int y)
            {
                this.X = x;
                this.Y = y;
            }

            public static implicit operator System.Drawing.Point(POINT p)
            {
                return new System.Drawing.Point(p.X, p.Y);
            }

            public static implicit operator POINT(System.Drawing.Point p)
            {
                return new POINT(p.X, p.Y);
            }
        }

        [DllImport("user32.dll")]
        public static extern bool ScreenToClient(IntPtr hWnd, ref POINT lpPoint);

        [DllImport("user32.dll", EntryPoint = "DefWindowProcW")]
        public static extern IntPtr DefWindowProc(IntPtr hWnd, uint msg, IntPtr wParam, IntPtr lParam);

        public enum WmNcHitTestValue
        {
            HtError = -2,
            HtTransparent = -1,
            HtNowhere = 0,
            HtClient = 1,
            HtCaption = 2,
            HtSysMenu = 3,
            HtGrowBox = 4,
            HtMenu = 5,
            HtHScroll = 6,
            HtVScroll = 7,
            HtMinButton = 8,
            HtMaxButton = 9,
            HtLeft = 10,
            HtRight = 11,
            HtTop = 12,
            HtTopLeft = 13,
            HtTopRight = 14,
            HtBottom = 15,
            HtBottomLeft = 16,
            HtBottomRight = 17,
            HtBorder = 18,
            HtObject = 19,
            HtClose = 20,
            HtHelp = 21
        }

        [DllImport("user32.dll")]
        public static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll")]
        public static extern bool ShowWindowAsync(IntPtr hWnd, int nCmdShow);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool GetCursorPos(out POINT lpPoint);

        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool SetCursorPos(int x, int y);
    }
}
