using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Engine.Windowing.Win32
{
    [SupportedOSPlatform("windows10.0")]
    internal class WindowSubsystem : IWindowSubsystem
    {
        public struct Event
        {
            public Window win;
            public WindowEventArgs args;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        private static LRESULT WindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
        {
            if (uMsg == PInvoke.WM_CREATE)
            {
                unsafe
                {
                    ///lpCreateParams = class Window

                    CREATESTRUCTW* createStruct = (CREATESTRUCTW*)lParam.Value;
                    if (PInvoke.SetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWLP_USERDATA, (nint)createStruct->lpCreateParams) == 0)
                        return (LRESULT)(-1);
                }
            }
            else
            {
                nint userdata = PInvoke.GetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWLP_USERDATA);
                GCHandle gcHandle = GCHandle.FromIntPtr(userdata);
                Window win = (Window)gcHandle.Target!;
                WindowSubsystem subsystem = (WindowSubsystem)win.Subsystem;

                bool returnDefault = false;
                ulong timeMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                switch (uMsg)
                {
                    case PInvoke.WM_CLOSE:
                        subsystem.EnqueueEvent(new Event { win = win, args = new WindowClosedEventArgs(timeMs) });
                        break;
                    case PInvoke.WM_SIZE:
                        uint width = (uint)(lParam.Value & 0xFFFF);
                        uint height = (uint)((lParam.Value >> 16) & 0xFFFF);
                        subsystem.EnqueueEvent(new Event { win = win, args = new WindowResizedEventArgs(timeMs, new WindowExtent(width, height)) });
                        break;
                    case PInvoke.WM_MOVE:
                        int x = (int)(lParam.Value & 0xFFFF);
                        int y = (int)((lParam.Value >> 16) & 0xFFFF);
                        subsystem.EnqueueEvent(new Event { win = win, args = new WindowMovedEventArgs(timeMs, new WindowPosition(x, y)) });
                        break;
                    default:
                        returnDefault = true;
                        break;
                }

                gcHandle.Free();

                return (returnDefault ? PInvoke.DefWindowProc(hWnd, uMsg, wParam, lParam) : (LRESULT)0);
            }

            return (LRESULT)0;
        }

        public const string WINDOW_CLASS_NAME = "WINDOW_CLASS";

        public HINSTANCE Instance { get => hInstance; }

        public WindowSubsystem()
        {
            hInstance = PInvoke.GetModuleHandle((PCWSTR)null);
            eventQueue = new();

            unsafe
            {
                fixed (char* lpszClassName = WINDOW_CLASS_NAME)
                {
                    var wndClassEx = new WNDCLASSEXW
                    {
                        cbSize = (uint)Marshal.SizeOf<WNDCLASSEXW>(),
                        style = WNDCLASS_STYLES.CS_VREDRAW | WNDCLASS_STYLES.CS_HREDRAW | WNDCLASS_STYLES.CS_DBLCLKS | WNDCLASS_STYLES.CS_OWNDC,
                        lpfnWndProc = &WindowProc,
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
            Window win = new Window(this, info);
            return win;
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
            MSG msg;
            BOOL res;
            while ((res = PInvoke.GetMessage(out msg, HWND.Null, 0, 0)) != 0)
            {
                if (res == -1)
                    throw new Win32Exception();
                else
                {
                    PInvoke.TranslateMessage(in msg);
                }
            }

            while (eventQueue.Count > 0)
            {
                Event winEvent = eventQueue.Dequeue();
                winEvent.win.HandleEvent(winEvent.args);
            }
        }

        public KeyaboardKey ScanCodeToKey(uint scanCode)
        {
            throw new NotImplementedException();
        }

        public void EnqueueEvent(Event winEvent)
        {
            eventQueue.Enqueue(winEvent);
        }

        private HINSTANCE hInstance;
        private Queue<Event> eventQueue;
    }
}
