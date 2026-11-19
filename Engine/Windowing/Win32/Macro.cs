using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.SystemServices;

namespace Engine.Windowing.Win32
{
    internal static class Win32PInvokeMacroExtensions
    {
        extension(PInvoke)
        {
            public static ushort HIWORD(uint l)
            {
                return (ushort)(l >> 16);
            }

            public static ushort LOWORD(uint l)
            {
                return (ushort)l;
            }

            public static int GET_X_LPARAM(LPARAM lp)
            {
                return (short)lp.Value;
            }

            public static int GET_Y_LPARAM(LPARAM lp)
            {
                return (short)(lp.Value >> 16);
            }

            public static short GET_WHEEL_DELTA_WPARAM(WPARAM wParam)
            {
                return (short)(wParam.Value >> 16);
            }

            public static MODIFIERKEYS_FLAGS GET_KEYSTATE_WPARAM(WPARAM wParam)
            {
                return (MODIFIERKEYS_FLAGS)((ushort)wParam.Value);
            }
        }
    }
}
