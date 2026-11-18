using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Engine.Windowing.Win32
{
    [SupportedOSPlatform("windows10.0")]
    internal class Window : IWindow
    {
        private WindowExtent FromClientToWindowExtent(in WindowExtent extent, WINDOW_STYLE dwStyle, WINDOW_EX_STYLE dwExStyle)
        {
            RECT rect = new RECT(0, 0, (int)extent.Width, (int)extent.Height);
            PInvoke.AdjustWindowRectEx(ref rect, dwStyle, (BOOL)0, dwExStyle);

            return new WindowExtent((uint)(rect.right - rect.left), (uint)(rect.bottom - rect.top));
        }

        public Window(WindowSubsystem subsystem, in WindowCreateInfo info)
        {
            this.subsystem = subsystem;
            isTextInputEnabled = info.TextInputEnabled;

            //State -> ignore

            try
            {
                gcHandle = GCHandle.Alloc(this, GCHandleType.Normal);
                unsafe
                {
                    fixed (char* lpClassName = WindowSubsystem.WINDOW_CLASS_NAME)
                    fixed (char* lpWindowName = info.Title)
                    {
                        WINDOW_STYLE dwStyle = WINDOW_STYLE.WS_OVERLAPPED | WINDOW_STYLE.WS_VISIBLE;
                        WINDOW_EX_STYLE dwExStyle = 0;

                        var winPos = FromClientToWindowExtent(info.Extent, dwStyle, dwExStyle);

                        RECT rect = new RECT(0, 0, (int)info.Extent.Width, (int)info.Extent.Height);
                        PInvoke.AdjustWindowRectEx(ref rect, dwStyle, (BOOL)0, dwExStyle);

                        hWnd = PInvoke.CreateWindowEx(
                            dwExStyle,
                            lpClassName,
                            lpWindowName,
                            dwStyle,
                            PInvoke.CW_USEDEFAULT,
                            PInvoke.CW_USEDEFAULT,
                            (int)winPos.Width,
                            (int)winPos.Height,
                            HWND.Null,
                            HMENU.Null,
                            subsystem.Instance,
                            (void*)GCHandle.ToIntPtr(gcHandle));

                        if (hWnd == HWND.Null)
                            throw new Win32Exception();
                    }
                }
            }
            catch
            {
                gcHandle.Free();
                throw;
            }
        }

        public IWindowSubsystem Subsystem => throw new NotImplementedException();

        public string Title
        {
            get
            {
                Marshal.SetLastPInvokeError(0);//clear last error
                int bufferLength = PInvoke.GetWindowTextLength(hWnd);
                if (bufferLength == 0)//may be an error
                {
                    if (Marshal.GetLastPInvokeError() != 0)
                        throw new Win32Exception();

                    return "";
                }

                Span<char> title = stackalloc char[bufferLength + 1];
                int titleLength = PInvoke.GetWindowText(hWnd, title);
                if (titleLength == 0)
                    throw new Win32Exception();

                return title.ToString();
            }
            set
            {
                unsafe
                {
                    fixed (char* lpString = value)
                    {
                        if (PInvoke.SetWindowText(hWnd, lpString) == 0)
                            throw new Win32Exception();
                    }
                }
            }
        }

        public WindowPosition Position
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public WindowExtent Extent
        {
            get
            {
                if (PInvoke.GetClientRect(hWnd, out RECT rect) == 0)
                    throw new Win32Exception();

                return new WindowExtent((uint)rect.right, (uint)rect.bottom);
            }
            set
            {
                var winPos = FromClientToWindowExtent(
                    value,
                    (WINDOW_STYLE)PInvoke.GetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE),
                    (WINDOW_EX_STYLE)PInvoke.GetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE));

                BOOL res = PInvoke.SetWindowPos(
                    hWnd,
                    HWND.Null,
                    0,
                    0,
                    (int)winPos.Width,
                    (int)winPos.Height,
                    SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER);

                if (res == 0)
                    throw new Win32Exception();
            }
        }
        public WindowState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool TextInputEnabled { get => isTextInputEnabled; set => isTextInputEnabled = value; }

        public HWND Handle { get => hWnd; }

        public void Dispose()
        {
            PInvoke.DestroyWindow(hWnd);
            gcHandle.Free();
        }

        public IEnumerable<VideoMode> GetAvailableDisplayVideoModes()
        {
            throw new NotImplementedException();
        }

        public VideoMode GetCurrentDisplayVideoMode()
        {
            throw new NotImplementedException();
        }

        public void SetDisplayVideoMode(VideoMode videoMode)
        {
            throw new NotImplementedException();
        }

        public void HandleEvent(WindowEventArgs eventArgs)
        {
            switch (eventArgs)
            {
                case WindowClosedEventArgs closed:
                    Closed?.Invoke(this, closed);
                    break;
                case WindowResizedEventArgs resized:
                    Resized?.Invoke(this, resized);
                    break;
                case WindowMovedEventArgs moved:
                    Moved?.Invoke(this, moved);
                    break;
                case WindowMouseFocusGainedEventArgs mouseFocusGained:
                    MouseFocusGained?.Invoke(this, mouseFocusGained);
                    break;
                case WindowMouseFocusLeftEventArgs mouseFocusLeft:
                    MouseFocusLeft?.Invoke(this, mouseFocusLeft);
                    break;
                case WindowKeyboardFocusGainedEventArgs keyboardFocusGained:
                    KeyboardFocusGained?.Invoke(this, keyboardFocusGained);
                    break;
                case WindowKeyboardFocusLeftEventArgs keyboardFocusLeft:
                    KeyboardFocusLeft?.Invoke(this, keyboardFocusLeft);
                    break;
                case WindowMouseCursorMovedEventArgs mouseCursorMoved:
                    MouseCursorMoved?.Invoke(this, mouseCursorMoved);
                    break;
                case WindowMouseButtonPressedEventArgs mouseButtonPressed:
                    MouseButtonPressed?.Invoke(this, mouseButtonPressed);
                    break;
                case WindowMouseWheelScrolledEventArgs mouseWheelScrolled:
                    MouseWheelScrolled?.Invoke(this, mouseWheelScrolled);
                    break;
                case WindowKeyboardKeyToggledEventArgs keyboardKeyToggled:
                    KeyboardKeyToggled?.Invoke(this, keyboardKeyToggled);
                    break;
                case WindowKeyboardTextInputtedEventArgs keyboardTextInputted:
                    KeyboardTextInputted?.Invoke(this, keyboardTextInputted);
                    break;
            }
        }

        public event Action<IWindow, WindowClosedEventArgs>? Closed;
        public event Action<IWindow, WindowResizedEventArgs>? Resized;
        public event Action<IWindow, WindowMovedEventArgs>? Moved;
        public event Action<IWindow, WindowMouseFocusGainedEventArgs>? MouseFocusGained;
        public event Action<IWindow, WindowMouseFocusLeftEventArgs>? MouseFocusLeft;
        public event Action<IWindow, WindowKeyboardFocusGainedEventArgs>? KeyboardFocusGained;
        public event Action<IWindow, WindowKeyboardFocusLeftEventArgs>? KeyboardFocusLeft;
        public event Action<IWindow, WindowMouseCursorMovedEventArgs>? MouseCursorMoved;
        public event Action<IWindow, WindowMouseButtonPressedEventArgs>? MouseButtonPressed;
        public event Action<IWindow, WindowMouseWheelScrolledEventArgs>? MouseWheelScrolled;
        public event Action<IWindow, WindowKeyboardKeyToggledEventArgs>? KeyboardKeyToggled;
        public event Action<IWindow, WindowKeyboardTextInputtedEventArgs>? KeyboardTextInputted;

        private GCHandle gcHandle;
        private HWND hWnd;
        private WindowSubsystem subsystem;
        private bool isTextInputEnabled;
    }
}
