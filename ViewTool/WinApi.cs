using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ViewTool
{
    internal static class WinApi
    {
        public static uint EnableClickThrough(Form form)
        {
            const int GWL_ExStyle = -20;
            const int WS_EX_Transparent = 0x20;
            const int WS_EX_Layered = 0x80000;
            const int LWA_Alpha = 0x2;
            uint initialWindowStyle = GetWindowLong(form.Handle, GWL_ExStyle);
            SetWindowLong(form.Handle, GWL_ExStyle, initialWindowStyle | WS_EX_Layered | WS_EX_Transparent);
            SetLayeredWindowAttributes(form.Handle, 0, (byte)(255 * form.Opacity), LWA_Alpha);
            return initialWindowStyle;
        }
        public static void DisableClickThrough(Form form, uint initialWindowStyle)
        {
            const int GWL_ExStyle = -20;
            const int WS_EX_Layered = 0x80000;
            SetWindowLong(form.Handle, GWL_ExStyle, initialWindowStyle | WS_EX_Layered);
        }


        [DllImport("user32.dll", SetLastError = true)]
        static extern UInt32 GetWindowLong(IntPtr hWnd, int nIndex);
        [DllImport("user32.dll")]
        static extern Int32 SetWindowLong(IntPtr hWnd, int nIndex, UInt32 dwNewLong);
        [DllImport("user32.dll")]
        static extern bool SetLayeredWindowAttributes(IntPtr hwnd, uint crKey, byte bAlpha, uint dwFlags);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;

            public int Width
            {
                get { return Right - Left; }
                set { Right = Left + value; }
            }
            public int Height
            {
                get { return Bottom - Top; }
                set { Bottom = Top + value; }
            }
        }
    }
}