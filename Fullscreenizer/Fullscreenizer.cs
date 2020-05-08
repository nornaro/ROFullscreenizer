using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace Fullscreenizer
{
    public partial class Fullscreenizer
	{
        public Fullscreenizer()
        {
            byte counter = countWindows();
            Process.Start(@".\NornaRO.EXE");
            while (counter+1 != countWindows()) { }
            while (!fullscreenizeWindow("NornaRO")) { }
            Environment.Exit(0);
        }
        byte countWindows()
        {
            byte counter = 0;
            List<IntPtr> visibleWindows = getVisibleWindows(true);
            foreach (IntPtr hwnd in visibleWindows)
                if (getWindowText(hwnd).Contains("NornaRO"))
                    counter++;
            return counter;
        }
		bool fullscreenizeWindow( string wnd )
		{
            bool found = false;
            List<IntPtr> visibleWindows = getVisibleWindows(true);
            foreach (IntPtr hwnd in visibleWindows)
                if (getWindowText(hwnd).Contains(wnd)) {
                    makeWindowBorderless(hwnd);
                    setWindowPos(hwnd, System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Left, System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Top, System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Width, System.Windows.Forms.Screen.PrimaryScreen.WorkingArea.Height, SetWindowPosFlags.SWP_FRAMECHANGED);
                    found = true;
                }
            return found;
        }

        // For enumeration of top-level windows.
        public delegate bool EnumDesktopWindowsDelegate(IntPtr hWnd, int lParam);
        [DllImport("user32.dll")]
        static extern bool EnumDesktopWindows(IntPtr hDesktop, EnumDesktopWindowsDelegate lpfn, IntPtr lParam);

        // Window text (title) related.
        [DllImport("user32.dll")]
        static extern int GetWindowTextLength(IntPtr hWnd);
        [DllImport("user32.dll")]
        static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

        // Window style stuff.
        static readonly int GWL_STYLE = -16;
        static readonly int WS_CAPTION = 0x00C00000;
        static readonly int WS_THICKFRAME = 0x00040000;
        static readonly int WS_MINIMIZE = 0x20000000;
        static readonly int WS_MAXIMIZE = 0x01000000;
        static readonly int WS_SYSMENU = 0x00080000;
        static readonly int BORDERLESS_FLAGS = ~(WS_CAPTION | WS_THICKFRAME | WS_MINIMIZE | WS_MAXIMIZE | WS_SYSMENU);
        [DllImport("user32.dll", EntryPoint = "GetWindowLong")]
        static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll", EntryPoint = "GetWindowLongPtr")]
        static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);
        static IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex)
        {
            if (IntPtr.Size == 8)
                return GetWindowLongPtr64(hWnd, nIndex);
            return GetWindowLongPtr32(hWnd, nIndex);
        }
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        static extern int SetWindowLong32(IntPtr hWnd, int nIndex, int dwNewLong);
        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, int nIndex, IntPtr dwNewLong);
        static IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong)
        {
            if (IntPtr.Size == 8)
                return SetWindowLongPtr64(hWnd, nIndex, dwNewLong);
            return new IntPtr(SetWindowLong32(hWnd, nIndex, dwNewLong.ToInt32()));
        }

        // Window rect stuff.
        static readonly IntPtr HWND_TOP = new IntPtr(0);
        [Flags]
        public enum SetWindowPosFlags : uint
        {
            SWP_FRAMECHANGED = 0x0020,
            SWP_NOREPOSITION = 0x0200,
            SWP_NOSIZE = 0x0001
        }
        [DllImport("user32.dll", SetLastError = true)]
        static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

        public static List<IntPtr> getVisibleWindows(bool ignoreWindowsWithoutTitle = true, bool ignoreCommonWindows = true)
        {
            List<IntPtr> windows = new List<IntPtr>();
            EnumDesktopWindowsDelegate proc = delegate (IntPtr hwnd, int lparam)
            {
                string windowText = getWindowText(hwnd);
                windows.Add(hwnd);
                return true;
            };
            EnumDesktopWindows(IntPtr.Zero, proc, IntPtr.Zero);
            return windows;
        }

        public static string getWindowText(IntPtr hwnd)
        {
            int size = GetWindowTextLength(hwnd);
            if (size <= 0)
                return "";
            size += 1;
            StringBuilder sb = new StringBuilder(size);
            GetWindowText(hwnd, sb, sb.Capacity);
            return sb.ToString();
        }

        public static void setWindowPos(IntPtr hwnd, int x, int y, int width, int height, SetWindowPosFlags flags)
        {
            SetWindowPos(hwnd, HWND_TOP, x, y, width, height, (uint)flags);
        }

        public static void makeWindowBorderless(IntPtr hwnd)
        {
            int style = GetWindowLongPtr(hwnd, GWL_STYLE).ToInt32();
            style &= BORDERLESS_FLAGS;
            SetWindowLongPtr(hwnd, GWL_STYLE, new IntPtr(style));
        }
    }
}