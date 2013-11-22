using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace Discussions.webkit_host
{
    public class WinAPI
    {         
        public const int WS_EX_TRANSPARENT = 0x00000020;
        public const int GWL_EXSTYLE = (-20);

        [DllImport("user32.dll")]
        public static extern int GetWindowLong(IntPtr hwnd, int index);

        [DllImport("user32.dll")]
        public static extern int SetWindowLong(IntPtr hwnd, int index, int newStyle);

        public static void SetHitTestVisible(Window wnd, bool visible)
        {
            // Get this window's handle
            IntPtr hwnd = new WindowInteropHelper(wnd).Handle;

            // Change the extended window style to include WS_EX_TRANSPARENT
            int extendedStyle = WinAPI.GetWindowLong(hwnd, WinAPI.GWL_EXSTYLE);

            if (visible)
                WinAPI.SetWindowLong(hwnd, WinAPI.GWL_EXSTYLE, extendedStyle & ~WinAPI.WS_EX_TRANSPARENT);
            else
                WinAPI.SetWindowLong(hwnd, WinAPI.GWL_EXSTYLE, extendedStyle | WinAPI.WS_EX_TRANSPARENT);
        }
    }
}