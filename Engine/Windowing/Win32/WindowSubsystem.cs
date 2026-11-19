using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.System.SystemServices;
using Windows.Win32.UI.Input.KeyboardAndMouse;
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

        private static MouseButton GetMouseButtonsFromWParam(WPARAM wParam)
        {
            MODIFIERKEYS_FLAGS rawButtons = PInvoke.GET_KEYSTATE_WPARAM(wParam);
       
            MouseButton buttons = MouseButton.None;
            if (rawButtons.HasFlag(MODIFIERKEYS_FLAGS.MK_LBUTTON))
                buttons |= MouseButton.Left;
            if (rawButtons.HasFlag(MODIFIERKEYS_FLAGS.MK_RBUTTON))
                buttons |= MouseButton.Right;
            if (rawButtons.HasFlag(MODIFIERKEYS_FLAGS.MK_MBUTTON))
                buttons |= MouseButton.Middle;
            if (rawButtons.HasFlag(MODIFIERKEYS_FLAGS.MK_XBUTTON1))
                buttons |= MouseButton.X1;
            if (rawButtons.HasFlag(MODIFIERKEYS_FLAGS.MK_XBUTTON2))
                buttons |= MouseButton.X2;

            return buttons;
        }

        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        private static LRESULT WindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
        {
            if (uMsg == PInvoke.WM_CREATE)
            {
                unsafe
                {
                    ///lpCreateParams = class Window

                    Marshal.SetLastPInvokeError(0);
                    CREATESTRUCTW* createStruct = (CREATESTRUCTW*)lParam.Value;
                    if (PInvoke.SetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWLP_USERDATA, (nint)createStruct->lpCreateParams) == 0 && Marshal.GetLastPInvokeError() != 0)
                        return (LRESULT)(-1);
                }
            }
            else
            {
                bool returnDefault = true;

                nint userdata = PInvoke.GetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWLP_USERDATA);
                if(userdata != 0)
                {
                    GCHandle gcHandle = GCHandle.FromIntPtr(userdata);
                    Window win = (Window)gcHandle.Target!;
                    WindowSubsystem subsystem = (WindowSubsystem)win.Subsystem;

                    ulong timeMs = (ulong)DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    switch (uMsg)
                    {
                        case PInvoke.WM_CLOSE:
                            win.IsClosed = true;
                            subsystem.EnqueueEvent(new Event { win = win, args = new WindowClosedEventArgs(timeMs) });
                            break;
                        case PInvoke.WM_SIZE:
                            uint width = PInvoke.LOWORD((uint)lParam.Value);
                            uint height = PInvoke.HIWORD((uint)lParam.Value);
                            subsystem.EnqueueEvent(new Event { win = win, args = new WindowResizedEventArgs(timeMs, new WindowExtent(width, height)) });
                            break;
                        case PInvoke.WM_MOVE:
                            int x = PInvoke.GET_X_LPARAM(lParam);
                            int y = PInvoke.GET_Y_LPARAM(lParam);
                            subsystem.EnqueueEvent(new Event { win = win, args = new WindowMovedEventArgs(timeMs, new WindowPosition(x, y)) });
                            break;
                        case PInvoke.WM_MOUSEMOVE:
                            if (PInvoke.GetActiveWindow() == hWnd)
                            {
                                WindowPosition cursorPosition = new WindowPosition(PInvoke.GET_X_LPARAM(lParam), PInvoke.GET_Y_LPARAM(lParam));
                                MouseButton buttons = GetMouseButtonsFromWParam(wParam);

                                if (!win.IsMouseCaptured)//emit focus gain
                                {
                                    TRACKMOUSEEVENT eventTrack = new TRACKMOUSEEVENT
                                    {
                                        cbSize = (uint)Marshal.SizeOf<TRACKMOUSEEVENT>(),
                                        dwFlags = TRACKMOUSEEVENT_FLAGS.TME_LEAVE,
                                        hwndTrack = hWnd,
                                        dwHoverTime = PInvoke.HOVER_DEFAULT
                                    };
                                    PInvoke.TrackMouseEvent(ref eventTrack);

                                    win.IsMouseCaptured = true;
                                    win.PreviousMouseCursorPosition = cursorPosition;

                                    subsystem.EnqueueEvent(new Event { win = win, args = new WindowMouseFocusGainedEventArgs(timeMs, cursorPosition, buttons) });
                                }
                                else
                                {
                                    WindowPosition deltaPosition = new WindowPosition(cursorPosition.X - win.PreviousMouseCursorPosition.X, cursorPosition.Y - win.PreviousMouseCursorPosition.Y);
                                    win.PreviousMouseCursorPosition = cursorPosition;

                                    subsystem.EnqueueEvent(new Event { win = win, args = new WindowMouseCursorMovedEventArgs(timeMs, cursorPosition, deltaPosition, buttons) });
                                }
                            }
                            break;
                        case PInvoke.WM_MOUSELEAVE:
                            win.IsMouseCaptured = false;

                            subsystem.EnqueueEvent(new Event { win = win, args = new WindowMouseFocusLeftEventArgs(timeMs) });
                            break;
                        case PInvoke.WM_SETFOCUS:
                            subsystem.EnqueueEvent(new Event { win = win, args = new WindowKeyboardFocusGainedEventArgs(timeMs) });
                            break;
                        case PInvoke.WM_KILLFOCUS:
                            subsystem.EnqueueEvent(new Event { win = win, args = new WindowKeyboardFocusLeftEventArgs(timeMs) });
                            break;
                        case PInvoke.WM_LBUTTONDOWN:
                        case PInvoke.WM_LBUTTONUP:
                        case PInvoke.WM_LBUTTONDBLCLK:
                            subsystem.EnqueueEvent(
                                new Event
                                {
                                    win = win,
                                    args = new WindowMouseButtonToggledEventArgs(
                                        timeMs,
                                        new WindowPosition(PInvoke.GET_X_LPARAM(lParam), PInvoke.GET_Y_LPARAM(lParam)),
                                        MouseButton.Left,
                                        (uint)(uMsg == PInvoke.WM_LBUTTONDBLCLK ? 2 : 1),
                                        uMsg != PInvoke.WM_LBUTTONUP)
                                });
                            break;
                        case PInvoke.WM_RBUTTONDOWN:
                        case PInvoke.WM_RBUTTONUP:
                        case PInvoke.WM_RBUTTONDBLCLK:
                            subsystem.EnqueueEvent(
                                new Event
                                {
                                    win = win,
                                    args = new WindowMouseButtonToggledEventArgs(
                                        timeMs,
                                        new WindowPosition(PInvoke.GET_X_LPARAM(lParam), PInvoke.GET_Y_LPARAM(lParam)),
                                        MouseButton.Right,
                                        (uint)(uMsg == PInvoke.WM_RBUTTONDBLCLK ? 2 : 1),
                                        uMsg != PInvoke.WM_RBUTTONUP)
                                });
                            break;
                        case PInvoke.WM_MBUTTONDOWN:
                        case PInvoke.WM_MBUTTONUP:
                        case PInvoke.WM_MBUTTONDBLCLK:
                            subsystem.EnqueueEvent(
                                new Event
                                {
                                    win = win,
                                    args = new WindowMouseButtonToggledEventArgs(
                                        timeMs,
                                        new WindowPosition(PInvoke.GET_X_LPARAM(lParam), PInvoke.GET_Y_LPARAM(lParam)),
                                        MouseButton.Middle,
                                        (uint)(uMsg == PInvoke.WM_MBUTTONDBLCLK ? 2 : 1),
                                        uMsg != PInvoke.WM_MBUTTONUP)
                                });
                            break;
                        case PInvoke.WM_XBUTTONDOWN:
                        case PInvoke.WM_XBUTTONUP:
                        case PInvoke.WM_XBUTTONDBLCLK:
                            subsystem.EnqueueEvent(
                                new Event
                                {
                                    win = win,
                                    args = new WindowMouseButtonToggledEventArgs(
                                        timeMs,
                                        new WindowPosition(PInvoke.GET_X_LPARAM(lParam), PInvoke.GET_Y_LPARAM(lParam)),
                                        PInvoke.HIWORD((uint)wParam.Value) == PInvoke.XBUTTON1 ? MouseButton.X1 : MouseButton.X2,
                                        (uint)(uMsg == PInvoke.WM_XBUTTONDBLCLK ? 2 : 1),
                                        uMsg != PInvoke.WM_XBUTTONUP)
                                });
                            break;
                        case PInvoke.WM_MOUSEWHEEL:
                            subsystem.EnqueueEvent(
                                new Event
                                {
                                    win = win,
                                    args = new WindowMouseWheelScrolledEventArgs(
                                        timeMs,
                                        new WindowPosition(PInvoke.GET_X_LPARAM(lParam), PInvoke.GET_Y_LPARAM(lParam)),
                                        GetMouseButtonsFromWParam(wParam),
                                        0,
                                        (float)PInvoke.GET_WHEEL_DELTA_WPARAM(wParam) / PInvoke.WHEEL_DELTA)
                                });
                            break;
                    }
                }

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
            scanCodeToKeyMapping = new();
            keyToScanCodeMapping = new();

            int layoutCount = PInvoke.GetKeyboardLayoutList(Span<HKL>.Empty);
            if (layoutCount == 0)
                throw new Win32Exception();

            keyboardLayouts = new HKL[layoutCount];
            if(PInvoke.GetKeyboardLayoutList(keyboardLayouts) == 0)
                throw new Win32Exception();

            //fill mappings

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

        public uint? KeyToScanCode(KeyboardKey key)
        {
            if (keyToScanCodeMapping.TryGetValue(key, out uint value))
                return value;

            return null;
        }

        public void PollEvents()
        {
            BOOL res;
            while ((res = PInvoke.PeekMessage(out MSG msg, HWND.Null, 0, 0, PEEK_MESSAGE_REMOVE_TYPE.PM_REMOVE)) != 0)
            {
                if (res == -1)
                    throw new Win32Exception();
                else
                {
                    PInvoke.DispatchMessage(in msg);
                    PInvoke.TranslateMessage(in msg);
                }
            }

            while (eventQueue.Count > 0)
            {
                Event winEvent = eventQueue.Dequeue();
                winEvent.win.HandleEvent(winEvent.args);
            }
        }

        public KeyboardKey? ScanCodeToKey(uint scanCode)
        {
            if (scanCodeToKeyMapping.TryGetValue(scanCode, out KeyboardKey value))
                return value;

            return null;
        }

        public void EnqueueEvent(Event winEvent)
        {
            eventQueue.Enqueue(winEvent);
        }

        private HINSTANCE hInstance;
        private Queue<Event> eventQueue;

        private HKL[] keyboardLayouts;
        private Dictionary<uint, KeyboardKey> scanCodeToKeyMapping;
        private Dictionary<KeyboardKey, uint> keyToScanCodeMapping;
    }
}
