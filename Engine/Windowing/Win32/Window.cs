using System;
using System.Collections.Generic;
using System.Reflection.PortableExecutable;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Windows.Win32;
using Windows.Win32.Foundation;
using System.ComponentModel;

namespace Engine.Windowing.Win32
{
    internal class Window : IWindow
    {
        [UnmanagedCallersOnly(CallConvs = [typeof(CallConvStdcall)])]
        public static LRESULT WindowProc(HWND hWnd, uint uMsg, WPARAM wParam, LPARAM lParam)
        {
            
        }

        public Window(Subsystem subsystem, in WindowCreateInfo info)
        {
           this.subsystem = subsystem;
        }

        public IWindowSubsystem Subsystem => throw new NotImplementedException();

        public string Title 
        {
            get
            {
                int length = PInvoke.GetWindowTextLength(hWnd) + 1;
                Span<char> title = stackalloc char[length];
                int actualLength = PInvoke.GetWindowText(hWnd, title);
                if (actualLength == 0)
                    throw new Win32Exception();
            }
            set => throw new NotImplementedException(); }
        public WindowPosition Position { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public WindowExtent Extent { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public WindowState State { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool TextInputEnabled { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        public void Dispose()
        {
            PInvoke.DestroyWindow(hWnd);
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

        public event Action<IWindow, WindowClosedEventArgs>? Closed;
        public event Action<IWindow, WindowResizedEventArgs>? Resized;
        public event Action<IWindow, WindowMovedEventArgs>? Moved;
        public event Action<IWindow, WindowMinimizedEventArgs>? Minimized;
        public event Action<IWindow, WindowMaximizedEventArgs>? Maximized;
        public event Action<IWindow, WindowRestoredEventArgs>? Restored;
        public event Action<IWindow, WindowMouseFocusGainedEventArgs>? MouseFocusGained;
        public event Action<IWindow, WindowMouseFocusLeftEventArgs>? MouseFocusLeft;
        public event Action<IWindow, WindowKeyboardFocusGainedEventArgs>? KeyboardFocusGained;
        public event Action<IWindow, WindowKeyboardFocusLeftEventArgs>? KeyboardFocusLeft;
        public event Action<IWindow, WindowMouseCursorMovedEventArgs>? MouseCursorMoved;
        public event Action<IWindow, WindowMouseButtonPressedEventArgs>? MouseButtonPressed;
        public event Action<IWindow, WindowMouseWheelScrolledEventArgs>? MouseWheelScrolled;
        public event Action<IWindow, WindowKeyboardKeyToggledEventArgs>? KeyboardKeyToggled;
        public event Action<IWindow, WindowKeyboardTextInputtedEventArgs>? KeyboardTextInputted;

        private HWND hWnd;
        private Subsystem subsystem;
        private bool isTextInputEnabled;
    }
}
