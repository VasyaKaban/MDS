using System.ComponentModel;
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
        public Window(WindowSubsystem subsystem, in WindowCreateInfo info)
        {
            this.subsystem = subsystem;
            isTextInputEnabled = info.TextInputEnabled;
            isFullScreen = false;
            monitorDeviceName = "";
            devModes = new();
            windowedModePlacement = new() { length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>() };

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

                        var winExtent = FromClientToWindowExtent(info.Extent, dwStyle, dwExStyle);

                        RECT rect = new RECT(0, 0, (int)info.Extent.Width, (int)info.Extent.Height);
                        PInvoke.AdjustWindowRectEx(ref rect, dwStyle, (BOOL)0, dwExStyle);

                        hWnd = PInvoke.CreateWindowEx(
                            dwExStyle,
                            lpClassName,
                            lpWindowName,
                            dwStyle,
                            PInvoke.CW_USEDEFAULT,
                            PInvoke.CW_USEDEFAULT,
                            (int)winExtent.Width,
                            (int)winExtent.Height,
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
                int bufferLength = PInvoke.GetWindowTextLength(hWnd);
                if (bufferLength == 0)
                    return "";

                Span<char> title = stackalloc char[bufferLength + 1];
                int titleLength = PInvoke.GetWindowText(hWnd, title);

                return title.Slice(0, titleLength).ToString();
            }
            set
            {
                unsafe
                {
                    fixed (char* lpString = value)
                    {
                        PInvoke.SetWindowText(hWnd, lpString);
                    }
                }
            }
        }

        public WindowExtent Extent
        {
            get
            {
                PInvoke.GetClientRect(hWnd, out RECT rect);

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
                    var winExtent = FromClientToWindowExtent(
                        value,
                        (WINDOW_STYLE)PInvoke.GetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWL_STYLE),
                        (WINDOW_EX_STYLE)PInvoke.GetWindowLongPtr(hWnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE));

                    PInvoke.SetWindowPos(
                        hWnd,
                        HWND.Null,
                        0,
                        0,
                        (int)winExtent.Width,
                        (int)winExtent.Height,
                        SET_WINDOW_POS_FLAGS.SWP_NOMOVE | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE | SET_WINDOW_POS_FLAGS.SWP_NOZORDER);
                }

                SaveWindowedModeState();
            }
        }

        public bool IsFullScreen { get => isFullScreen; }

        public bool IsTextInputEnabled { get => isTextInputEnabled; set => isTextInputEnabled = value; }
        public bool IsClosed { get => isClosed; set => isClosed = value; }

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

        public bool IsMouseCaptured { get => isMouseCaptured; }
        public WindowPosition MouseCursorPosition { get => previousMouseCursorPosition; }

        public void CaptureMouse(in WindowPosition cursorPosition)
        {
            isMouseCaptured = true;
            previousMouseCursorPosition = cursorPosition;
        }

        public void UncaptureMouse()
        {
            isMouseCaptured = false;
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

        public void UpdateWindowedModeState()
        {
            if (!IsFullScreen)
                SaveWindowedModeState();
        }

        private static WindowExtent FromClientToWindowExtent(in WindowExtent extent, WINDOW_STYLE dwStyle, WINDOW_EX_STYLE dwExStyle)
        {
            RECT rect = new RECT(0, 0, (int)extent.Width, (int)extent.Height);
            PInvoke.AdjustWindowRectEx(ref rect, dwStyle, (BOOL)0, dwExStyle);

            return new WindowExtent((uint)(rect.right - rect.left), (uint)(rect.bottom - rect.top));
        }

        private void SaveWindowedModeState()
        {
            PInvoke.GetWindowPlacement(hWnd, ref windowedModePlacement);
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

            //set registry display mode
            unsafe
            {
                PInvoke.ChangeDisplaySettingsEx(monitorDeviceName, devMode, 0, null);
            }

            //restore placement
            PInvoke.SetWindowPlacement(hWnd, windowedModePlacement);

            isFullScreen = false;
            SaveWindowedModeState();
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

        private GCHandle gcHandle;
        private HWND hWnd;
        private WindowSubsystem subsystem;
        private bool isTextInputEnabled;
        private bool isClosed;
        private bool isMouseCaptured;
        private WindowPosition previousMouseCursorPosition;
        private char highSurrogate;

        private HMONITOR hMonitor;
        private string monitorDeviceName;

        private record struct DevModeInfo(DEVMODEW DevMode, uint Index);

        private Dictionary<VideoMode, DevModeInfo> devModes;
        private WINDOWPLACEMENT windowedModePlacement = new() { length = (uint)Marshal.SizeOf<WINDOWPLACEMENT>() };
        private bool isFullScreen;
    }
}
