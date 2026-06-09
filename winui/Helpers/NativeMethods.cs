using System;
using System.Runtime.InteropServices;

namespace StarSorter.Helpers
{
    internal partial class NativeMethods
    {
        [DllImport("user32.dll", EntryPoint = "SetWindowLong")]
        internal static extern int SetWindowLong32(IntPtr hWnd, WindowLongIndexFlags nIndex, WinProc newProc);

        [DllImport("user32.dll", EntryPoint = "SetWindowLongPtr")]
        internal static extern IntPtr SetWindowLongPtr64(IntPtr hWnd, WindowLongIndexFlags nIndex, WinProc newProc);

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLongPtr")]
        internal static extern IntPtr SetWindowLongPtr(IntPtr hWnd, int nIndex, IntPtr dwNewLong);

        [DllImport("User32.dll", CharSet = CharSet.Auto, EntryPoint = "SetWindowLong")]
        internal static extern IntPtr SetWindowLong(IntPtr hWnd, int nIndex, IntPtr dwNewLong);


        [DllImport("user32.dll")]
        internal static extern IntPtr CallWindowProc(IntPtr lpPrevWndFunc, IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam);


        internal static bool IsKeyDownHook(IntPtr lWord)
        {
            // The 30th bit tells what the previous key state is with 0 being the "UP" state
            // For more info see https://learn.microsoft.com/windows/win32/winmsg/keyboardproc#lparam-in
            return (lWord >> 30 & 1) == 0;
        }

        internal delegate IntPtr WinProc(IntPtr hWnd, WindowMessage Msg, IntPtr wParam, IntPtr lParam);

        [Flags]
        internal enum WindowLongIndexFlags : int
        {
            GWL_WNDPROC = -4,
        }

        internal enum WindowMessage : int
        {
            WM_GETMINMAXINFO = 0x0024,
        }
    }
}
