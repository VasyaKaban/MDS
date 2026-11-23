using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Engine.Windowing.Win32
{
    [SupportedOSPlatform("windows10.0")]
    internal sealed class Window : IWindow
    {
        private static WindowExtent FromClientToWindowExtent(in WindowExtent extent, WINDOW_STYLE dwStyle, WINDOW_EX_STYLE dwExStyle)
        {
            RECT rect = new RECT(0, 0, (int)extent.Width, (int)extent.Height);
            PInvoke.AdjustWindowRectEx(ref rect, dwStyle, (BOOL)0, dwExStyle);

            return new WindowExtent((uint)(rect.right - rect.left), (uint)(rect.bottom - rect.top));
        }

        private void SaveWindowedModeState()
        {
            unsafe
            {
                RECT clientRect = new RECT();
                PInvoke.GetClientRect(hWnd, &clientRect);

                Point point = new Point(0, 0);
                PInvoke.ClientToScreen(hWnd, ref point);

                windowedModeSavedPosition = new WindowPosition(point.X, point.Y);
                windowedModeSavedExtent = new WindowExtent((uint)(clientRect.right - clientRect.left), (uint)(clientRect.bottom - clientRect.top));
            }
        }

        private void RestoreWindowedMode()
        {
            DEVMODEW devMode = new DEVMODEW { dmSize = (ushort)Marshal.SizeOf<DEVMODEW>() };
            
            //get registry display mode
            PInvoke.EnumDisplaySettingsEx(monitorDeviceName, ENUM_DISPLAY_SETTINGS_MODE.ENUM_REGISTRY_SETTINGS, ref devMode, 0);

            //restore window styles
            var style = PInvoke.GetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
            style |= (nint)WINDOW_STYLE.WS_OVERLAPPEDWINDOW;
            PInvoke.SetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, style);

            //restore window position
            //restore window extent
            PInvoke.SetWindowPos(
                hWnd,
                HWND.HWND_NOTOPMOST,
                windowedModeSavedPosition.X,
                windowedModeSavedPosition.Y,
                (int)windowedModeSavedExtent.Width,
                (int)windowedModeSavedExtent.Height,
                0);

            //set registry display mode
            unsafe
            {
                PInvoke.ChangeDisplaySettingsEx(monitorDeviceName, devMode, 0, null);
            }

            isFullScreen = false;
            SaveWindowedModeState();
        }

        public Window(WindowSubsystem subsystem, in WindowCreateInfo info)
        {
            this.subsystem = subsystem;
            isTextInputEnabled = info.TextInputEnabled;
            isFullScreen = false;

            try
            {
                gcHandle = GCHandle.Alloc(this, GCHandleType.Normal);
                unsafe
                {
                    fixed (char* lpClassName = WindowSubsystem.WINDOW_CLASS_NAME)
                    fixed (char* lpWindowName = info.Title)
                    {
                        WINDOW_STYLE dwStyle = WINDOW_STYLE.WS_OVERLAPPEDWINDOW | WINDOW_STYLE.WS_VISIBLE;
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

            UpdateMonitor();

            //save windowed mode saved state
            SaveWindowedModeState();
        }

        public IWindowSubsystem Subsystem => subsystem;

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

                return title.Slice(0, titleLength).ToString();
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
                if (isFullScreen)//reset fullscreen and set windowed mode
                {
                    RestoreWindowedMode();
                }
                else
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

                SaveWindowedModeState();
            }
        }

        public bool IsFullScreen { get => isFullScreen; }

        public bool IsTextInputEnabled { get => isTextInputEnabled; set => isTextInputEnabled = value; }
        public bool IsClosed { get => isClosed; set => isClosed = value; }

        public HWND Handle { get => hWnd; }
        public bool IsMouseCaptured { get => isMouseCaptured; set => isMouseCaptured = value; }
        public WindowPosition PreviousMouseCursorPosition { get => previousMouseCursorPosition; set => previousMouseCursorPosition = value; }

        public void Dispose()
        {
            PInvoke.DestroyWindow(hWnd);
            gcHandle.Free();
        }

        public IReadOnlyCollection<VideoMode> GetAvailableDisplayVideoModes()
        {
            return devModes.Keys;
        }

        public VideoMode GetCurrentDisplayVideoMode()
        {
            DEVMODEW devMode = new DEVMODEW { dmSize = (ushort)Marshal.SizeOf<DEVMODEW>() };
            PInvoke.EnumDisplaySettingsEx(monitorDeviceName, ENUM_DISPLAY_SETTINGS_MODE.ENUM_CURRENT_SETTINGS, ref devMode, 0);

            return new VideoMode(devMode.dmPelsWidth, devMode.dmPelsHeight, devMode.dmDisplayFrequency);
        }

        public void SetFullscreenMode(VideoMode videoMode)
        {
            var devModeInfo = devModes[videoMode];

            //set display mode
            unsafe
            {
                PInvoke.ChangeDisplaySettingsEx(monitorDeviceName, devModeInfo.DevMode, CDS_TYPE.CDS_FULLSCREEN, null);
            }

            //get video mode position
            MONITORINFO monInfo = new MONITORINFO { cbSize = (uint)Marshal.SizeOf<MONITORINFOEXW>() };
            PInvoke.GetMonitorInfo(hMonitor, ref monInfo);

            //change window styles
            var style = PInvoke.GetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE);
            style &= ~(nint)WINDOW_STYLE.WS_OVERLAPPEDWINDOW;
            PInvoke.SetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE, style);

            //set IsFullScreen state before SetWindowPos call
            isFullScreen = true;

            //change window position
            //change window extent
            PInvoke.SetWindowPos(
                hWnd,
                HWND.HWND_TOPMOST,
                monInfo.rcMonitor.left,
                monInfo.rcMonitor.top,
                (int)devModeInfo.DevMode.dmPelsWidth,
                (int)devModeInfo.DevMode.dmPelsHeight,
                0);
        }

        public void SetWindowedMode()
        {
            if (IsFullScreen)
                RestoreWindowedMode();
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
                case WindowMouseButtonToggledEventArgs mouseButtonToggled:
                    MouseButtonToggled?.Invoke(this, mouseButtonToggled);
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
                case WindowDisplayChangedEventArgs displayChanged:
                    DisplayChanged?.Invoke(this, displayChanged);
                    break;
            }
        }

        public string? AppendCharMessageCharacter(char c)
        {
            if (char.IsHighSurrogate(c))
            {
                highSurrogate = c;
                return null;
            }
            else if (char.IsLowSurrogate(c))
            {
                return new string(highSurrogate, c);
            }
            else
                return c.ToString();
        }

        public void UpdateMonitor()
        {
            hMonitor = PInvoke.MonitorFromWindow(hWnd, MONITOR_FROM_FLAGS.MONITOR_DEFAULTTONEAREST);

            MONITORINFOEXW monitorInfoEx = new MONITORINFOEXW();
            monitorInfoEx.monitorInfo.cbSize = (uint)Marshal.SizeOf<MONITORINFOEXW>();

            PInvoke.GetMonitorInfo(hMonitor, ref monitorInfoEx.monitorInfo);

            monitorDeviceName = monitorInfoEx.szDevice.ToString();

            devModes = new(capacity: 32);

            for (uint devModeIndex = 0; true; devModeIndex++)
            {
                DEVMODEW devMode = new DEVMODEW { dmSize = (ushort)Marshal.SizeOf<DEVMODEW>() };
                if (PInvoke.EnumDisplaySettingsEx(monitorDeviceName, (ENUM_DISPLAY_SETTINGS_MODE)devModeIndex, ref devMode, 0) != 0)
                {
                    if (!(devMode.dmFields.HasFlag(DEVMODE_FIELD_FLAGS.DM_BITSPERPEL) &&
                        devMode.dmFields.HasFlag(DEVMODE_FIELD_FLAGS.DM_PELSWIDTH) &&
                        devMode.dmFields.HasFlag(DEVMODE_FIELD_FLAGS.DM_PELSHEIGHT) &&
                        devMode.dmFields.HasFlag(DEVMODE_FIELD_FLAGS.DM_DISPLAYFREQUENCY)))
                    {
                        continue;
                    }

                    if (devMode.dmBitsPerPel < 32)
                        continue;

                    DevModeInfo devModeInfo = new DevModeInfo { DevMode = devMode, Index = devModeIndex };
                    VideoMode videoMode = new VideoMode(devMode.dmPelsWidth, devMode.dmPelsHeight, devMode.dmDisplayFrequency);

                    devModes.TryAdd(videoMode, devModeInfo);
                }
                else
                    break;
            }
        }

        public void UpdateWindowedModeSavedPosition(WindowPosition position)
        {
            if(!IsFullScreen)
                windowedModeSavedPosition = position;
        }

        public void UpdateWindowedModeSavedExtent(WindowExtent extent)
        {
            if (!IsFullScreen)
                windowedModeSavedExtent = extent;
        }

        public event Action<IWindow, WindowClosedEventArgs>? Closed;
        public event Action<IWindow, WindowResizedEventArgs>? Resized;
        public event Action<IWindow, WindowMovedEventArgs>? Moved;
        public event Action<IWindow, WindowMouseFocusGainedEventArgs>? MouseFocusGained;
        public event Action<IWindow, WindowMouseFocusLeftEventArgs>? MouseFocusLeft;
        public event Action<IWindow, WindowKeyboardFocusGainedEventArgs>? KeyboardFocusGained;
        public event Action<IWindow, WindowKeyboardFocusLeftEventArgs>? KeyboardFocusLeft;
        public event Action<IWindow, WindowMouseCursorMovedEventArgs>? MouseCursorMoved;
        public event Action<IWindow, WindowMouseButtonToggledEventArgs>? MouseButtonToggled;
        public event Action<IWindow, WindowMouseWheelScrolledEventArgs>? MouseWheelScrolled;
        public event Action<IWindow, WindowKeyboardKeyToggledEventArgs>? KeyboardKeyToggled;
        public event Action<IWindow, WindowKeyboardTextInputtedEventArgs>? KeyboardTextInputted;
        public event Action<IWindow, WindowDisplayChangedEventArgs>? DisplayChanged;

        private GCHandle gcHandle = new();
        private HWND hWnd = new();
        private WindowSubsystem subsystem;
        private bool isTextInputEnabled = false;
        private bool isClosed = false;
        private bool isMouseCaptured = false;
        private WindowPosition previousMouseCursorPosition = new(0, 0);
        private char highSurrogate = '0';

        private HMONITOR hMonitor = new();
        private string monitorDeviceName = "";

        private record struct DevModeInfo(DEVMODEW DevMode, uint Index);

        private Dictionary<VideoMode, DevModeInfo> devModes = new();
        private WindowPosition windowedModeSavedPosition;
        private WindowExtent windowedModeSavedExtent;
        private bool isFullScreen;
    }
}
