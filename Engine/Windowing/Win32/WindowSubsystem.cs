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
    internal sealed class WindowSubsystem : IWindowSubsystem
    {
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

            Span<HKL> keyboardLayouts = stackalloc HKL[layoutCount];
            if (PInvoke.GetKeyboardLayoutList(keyboardLayouts) == 0)
                throw new Win32Exception();

            virtualKeyToNameMapping = new string?[256];
            foreach (HKL layout in keyboardLayouts)
            {
                for (int vk = 0; vk < 256; vk++)
                {
                    if (virtualKeyToNameMapping[vk] != null)
                        continue;

                    char name = (char)PInvoke.MapVirtualKeyEx((uint)vk, MAP_VIRTUAL_KEY_TYPE.MAPVK_VK_TO_CHAR, layout);
                    if (char.IsSurrogate(name))
                        continue;

                    virtualKeyToNameMapping[vk] = name.ToString();
                }
            }

            foreach (var (ps2ScanCode, key) in PS2Set1StableScanCodeMapping.MAPPING)
            {
                scanCodeToKeyMapping.Add(ps2ScanCode, key);
                keyToScanCodeMapping.Add(key, ps2ScanCode);
            }

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

        public bool IsKeyPressed(uint scanCode)
        {
            uint vk = PInvoke.MapVirtualKeyEx(scanCode, MAP_VIRTUAL_KEY_TYPE.MAPVK_VSC_TO_VK_EX, HKL.Null);
            return PInvoke.GetAsyncKeyState((int)vk) < 0;
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

        private void EnqueueEvent(Event winEvent)
        {
            eventQueue.Enqueue(winEvent);
        }

        private struct Event
        {
            public Window win;
            public WindowEventArgs args;
        }

        private static MouseButtons GetMouseButtonsFromWParam(WPARAM wParam)
        {
            MODIFIERKEYS_FLAGS rawButtons = PInvoke.GET_KEYSTATE_WPARAM(wParam);

            MouseButtons buttons = 0;
            if (rawButtons.HasFlag(MODIFIERKEYS_FLAGS.MK_LBUTTON))
                buttons |= MouseButtons.Left;
            if (rawButtons.HasFlag(MODIFIERKEYS_FLAGS.MK_RBUTTON))
                buttons |= MouseButtons.Right;
            if (rawButtons.HasFlag(MODIFIERKEYS_FLAGS.MK_MBUTTON))
                buttons |= MouseButtons.Middle;
            if (rawButtons.HasFlag(MODIFIERKEYS_FLAGS.MK_XBUTTON1))
                buttons |= MouseButtons.X1;
            if (rawButtons.HasFlag(MODIFIERKEYS_FLAGS.MK_XBUTTON2))
                buttons |= MouseButtons.X2;

            return buttons;
        }

        private static KeyboardModifiers GetKeyboardModifiers()
        {
            KeyboardModifiers modifiers = 0;
            if (PInvoke.GetAsyncKeyState((int)VIRTUAL_KEY.VK_LSHIFT) < 0)
                modifiers |= KeyboardModifiers.LeftShift;
            if (PInvoke.GetAsyncKeyState((int)VIRTUAL_KEY.VK_RSHIFT) < 0)
                modifiers |= KeyboardModifiers.RightShift;
            if (PInvoke.GetAsyncKeyState((int)VIRTUAL_KEY.VK_LCONTROL) < 0)
                modifiers |= KeyboardModifiers.LeftControl;
            if (PInvoke.GetAsyncKeyState((int)VIRTUAL_KEY.VK_RCONTROL) < 0)
                modifiers |= KeyboardModifiers.RightControl;
            if (PInvoke.GetAsyncKeyState((int)VIRTUAL_KEY.VK_LMENU) < 0)
                modifiers |= KeyboardModifiers.LeftAlt;
            if (PInvoke.GetAsyncKeyState((int)VIRTUAL_KEY.VK_RMENU) < 0)
                modifiers |= KeyboardModifiers.RightAlt;
            if (PInvoke.GetAsyncKeyState((int)VIRTUAL_KEY.VK_LWIN) < 0)
                modifiers |= KeyboardModifiers.LeftGUI;
            if (PInvoke.GetAsyncKeyState((int)VIRTUAL_KEY.VK_RWIN) < 0)
                modifiers |= KeyboardModifiers.RightGUI;

            return modifiers;
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
                    if (PInvoke.SetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWLP_USERDATA, (nint)createStruct->lpCreateParams) == 0 &&
                        Marshal.GetLastPInvokeError() != 0)
                        return (LRESULT)(-1);
                }
            }
            else
            {
                bool returnDefault = true;

                nint userdata = PInvoke.GetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWLP_USERDATA);
                if (userdata != 0)
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

                            var extent = new WindowExtent(width, height);
                            subsystem.EnqueueEvent(new Event { win = win, args = new WindowResizedEventArgs(timeMs, extent) });

                            win.UpdateWindowedModeState();
                            break;
                        case PInvoke.WM_MOVE:
                            int x = PInvoke.GET_X_LPARAM(lParam);
                            int y = PInvoke.GET_Y_LPARAM(lParam);

                            var position = new WindowPosition(x, y);
                            subsystem.EnqueueEvent(new Event { win = win, args = new WindowMovedEventArgs(timeMs, position) });

                            win.UpdateWindowedModeState();
                            break;
                        case PInvoke.WM_MOUSEMOVE:
                            if (PInvoke.GetActiveWindow() == hWnd)
                            {
                                WindowPosition cursorPosition = new WindowPosition(PInvoke.GET_X_LPARAM(lParam), PInvoke.GET_Y_LPARAM(lParam));
                                MouseButtons buttons = GetMouseButtonsFromWParam(wParam);

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

                                    win.CaptureMouse(cursorPosition);

                                    subsystem.EnqueueEvent(new Event { win = win, args = new WindowMouseFocusGainedEventArgs(timeMs, cursorPosition, buttons) });
                                }
                                else
                                {
                                    WindowPosition deltaPosition = new WindowPosition(cursorPosition.X - win.MouseCursorPosition.X, cursorPosition.Y - win.MouseCursorPosition.Y);
                                    win.CaptureMouse(cursorPosition);

                                    subsystem.EnqueueEvent(new Event { win = win, args = new WindowMouseCursorMovedEventArgs(timeMs, cursorPosition, deltaPosition, buttons) });
                                }
                            }
                            break;
                        case PInvoke.WM_MOUSELEAVE:
                            win.UncaptureMouse();

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
                                        MouseButtons.Left,
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
                                        MouseButtons.Right,
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
                                        MouseButtons.Middle,
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
                                        PInvoke.HIWORD((uint)wParam.Value) == PInvoke.XBUTTON1 ? MouseButtons.X1 : MouseButtons.X2,
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
                        case PInvoke.WM_KEYDOWN:
                        case PInvoke.WM_KEYUP:
                        case PInvoke.WM_SYSKEYDOWN:
                        case PInvoke.WM_SYSKEYUP:
                            uint uLParam = (uint)lParam.Value;
                            ushort repeatCount = PInvoke.LOWORD(uLParam);
                            ushort rawScanCode = (byte)((uLParam >> 16) & 0xFF);
                            if (((uLParam >> 24) & 0b1) == 1)//isExtended
                                rawScanCode |= 0xE000;

                            bool isPressed = ((uLParam >> 31) & 0b1) == 0;

                            VIRTUAL_KEY vk = (VIRTUAL_KEY)(wParam.Value & 0xFF);
                            if (vk == VIRTUAL_KEY.VK_SHIFT || vk == VIRTUAL_KEY.VK_MENU || vk == VIRTUAL_KEY.VK_CONTROL)
                                vk = (VIRTUAL_KEY)PInvoke.MapVirtualKeyEx(rawScanCode, MAP_VIRTUAL_KEY_TYPE.MAPVK_VSC_TO_VK_EX, HKL.Null);

                            uint ps2ScanCode = 0;
                            if (vk == VIRTUAL_KEY.VK_PAUSE && rawScanCode == 0x45)//set real Pause scan code
                                ps2ScanCode = 0xE11D45;
                            else if (vk == VIRTUAL_KEY.VK_NUMLOCK && rawScanCode == 0xE045)
                                ps2ScanCode = 0x45;
                            else
                                ps2ScanCode = rawScanCode;

                            KeyboardKey key;
                            if (!subsystem.scanCodeToKeyMapping.TryGetValue(ps2ScanCode, out key))
                            {
                                string? vkName = subsystem.virtualKeyToNameMapping[(uint)vk];
                                if (vkName != null)
                                {
                                    key = new KeyboardKey(vkName);
                                    subsystem.scanCodeToKeyMapping.Add(ps2ScanCode, key);
                                    subsystem.keyToScanCodeMapping.TryAdd(key, ps2ScanCode);
                                }
                                else
                                {
                                    key = new KeyboardKey(SpecialKey.Unknown);
                                    subsystem.scanCodeToKeyMapping.Add(ps2ScanCode, key);
                                }
                            }

                            subsystem.EnqueueEvent(
                                new Event
                                {
                                    win = win,
                                    args = new WindowKeyboardKeyToggledEventArgs(
                                        timeMs,
                                        ps2ScanCode,
                                        GetKeyboardModifiers(),
                                        key,
                                        isPressed,
                                        repeatCount)
                                });
                            break;
                        case PInvoke.WM_CHAR:
                            if (win.IsTextInputEnabled)
                            {
                                string? text = win.AppendCharMessageCharacter((char)wParam.Value);
                                if (text != null)
                                {
                                    subsystem.EnqueueEvent(
                                        new Event
                                        {
                                            win = win,
                                            args = new WindowKeyboardTextInputtedEventArgs(
                                                timeMs,
                                                GetKeyboardModifiers(),
                                                text)
                                        });
                                }
                            }
                            break;
                        case PInvoke.WM_SYSCOMMAND:
                            if ((int)(wParam.Value & 0xFFF0) == PInvoke.SC_KEYMENU)
                                returnDefault = false;
                            break;
                        case PInvoke.WM_DISPLAYCHANGE:
                            win.UpdateMonitor();
                            subsystem.EnqueueEvent(
                                new Event
                                {
                                    win = win,
                                    args = new WindowDisplayChangedEventArgs(timeMs)
                                });
                            break;
                    }
                }

                return (returnDefault ? PInvoke.DefWindowProc(hWnd, uMsg, wParam, lParam) : (LRESULT)0);
            }

            return (LRESULT)0;
        }

        private HINSTANCE hInstance;
        private Queue<Event> eventQueue;

        private string?[] virtualKeyToNameMapping;
        private Dictionary<uint, KeyboardKey> scanCodeToKeyMapping;
        private Dictionary<KeyboardKey, uint> keyToScanCodeMapping;
    }
}
