namespace Engine.Windowing
{
    public interface IWindow : IDisposable
    {
        IWindowSubsystem Subsystem { get; }

        string Title { get; set; }
        WindowExtent Extent { get; set; }
        bool IsFullScreen { get; }

        bool IsTextInputEnabled { get; set; }//if enabled - collects all text input events(implementation) and emits KeyboardTextInputted
        bool IsClosed { get; }

        VideoMode GetCurrentDisplayVideoMode();
        IReadOnlyCollection<VideoMode> GetAvailableDisplayVideoModes();
        void SetFullscreenMode(VideoMode videoMode);
        void SetWindowedMode();

        event Action<IWindow, WindowClosedEventArgs> Closed;
        event Action<IWindow, WindowResizedEventArgs> Resized;
        event Action<IWindow, WindowMovedEventArgs> Moved;
        event Action<IWindow, WindowMouseFocusGainedEventArgs> MouseFocusGained;
        event Action<IWindow, WindowMouseFocusLeftEventArgs> MouseFocusLeft;
        event Action<IWindow, WindowKeyboardFocusGainedEventArgs> KeyboardFocusGained;
        event Action<IWindow, WindowKeyboardFocusLeftEventArgs> KeyboardFocusLeft;
        event Action<IWindow, WindowMouseCursorMovedEventArgs> MouseCursorMoved;
        event Action<IWindow, WindowMouseButtonToggledEventArgs> MouseButtonToggled;
        event Action<IWindow, WindowMouseWheelScrolledEventArgs> MouseWheelScrolled;
        event Action<IWindow, WindowKeyboardKeyToggledEventArgs> KeyboardKeyToggled;
        event Action<IWindow, WindowKeyboardTextInputtedEventArgs> KeyboardTextInputted;
        event Action<IWindow, WindowDisplayChangedEventArgs> DisplayChanged;
    }
}
