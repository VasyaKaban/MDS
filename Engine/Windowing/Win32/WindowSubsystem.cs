using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using System.ComponentModel;

namespace Engine.Windowing.Win32
{
    internal class WindowSubsystem : IWindowSubsystem
    {
        private const string WINDOW_CLASS_NAME = "WINDOW_CLASS";

        public WindowSubsystem() 
        {
            hInstance = PInvoke.GetModuleHandle((PCWSTR)null);

            unsafe
            {
                fixed (char* lpszClassName = WINDOW_CLASS_NAME)
                {
                    var wndClassEx = new WNDCLASSEXW
                    {
                        cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
                        style = WNDCLASS_STYLES.CS_VREDRAW | WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_DBLCLKS | WNDCLASS_STYLES.CS_OWNDC,
                        lpfnWndProc = &Window.WindowProc,
                        cbClsExtra = 0,
                        cbWndExtra = 0,
                        hInstance = hInstance,
                        hIcon = HICON.Null,
                        hCursor = HCURSOR.Null,
                        hbrBackground = HBRUSH.Null,
                        lpszMenuName = null,
                        lpszClassName = lpszClassName
                    };

                    var wndClassAtom = PInvoke.RegisterClassEx(wndClassEx);
                    if (wndClassAtom == 0)
                        throw new Win32Exception();
                }
            }
        }

        public IWindow CreateWindow(in WindowCreateInfo info)
        {
            return new Window(info);
        }

        public void Dispose()
        {
            unsafe
            {
                fixed (char* lpClassName = WINDOW_CLASS_NAME)
                {
                    PInvoke.UnregisterClass(lpClassName, hInstance);
                }
            }
        }

        public uint KeyToScanCode(KeyaboardKey key)
        {
            throw new NotImplementedException();
        }

        public void PollEvents()
        {
            throw new NotImplementedException();
        }

        public KeyaboardKey ScanCodeToKey(uint scanCode)
        {
            throw new NotImplementedException();
        }

        private HINSTANCE hInstance;
    }
}
