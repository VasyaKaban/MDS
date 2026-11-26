namespace Engine.Windowing
{
    public delegate void WindowEventHandler<A>(IWindow window, in A args) 
        where A: allows ref struct;

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
        void SetFullscreenMode(in VideoMode videoMode);
        void SetWindowedMode();

        event WindowEventHandler<WindowClosedEventArgs> Closed;
        event WindowEventHandler<WindowResizedEventArgs> Resized;
        event WindowEventHandler<WindowMovedEventArgs> Moved;
        event WindowEventHandler<WindowMouseFocusGainedEventArgs> MouseFocusGained;
        event WindowEventHandler<WindowMouseFocusLeftEventArgs> MouseFocusLeft;
        event WindowEventHandler<WindowKeyboardFocusGainedEventArgs> KeyboardFocusGained;
        event WindowEventHandler<WindowKeyboardFocusLeftEventArgs> KeyboardFocusLeft;
        event WindowEventHandler<WindowMouseCursorMovedEventArgs> MouseCursorMoved;
        event WindowEventHandler<WindowMouseButtonToggledEventArgs> MouseButtonToggled;
        event WindowEventHandler<WindowMouseWheelScrolledEventArgs> MouseWheelScrolled;
        event WindowEventHandler<WindowKeyboardKeyToggledEventArgs> KeyboardKeyToggled;
        event WindowEventHandler<WindowKeyboardTextInputtedEventArgs> KeyboardTextInputted;
        event WindowEventHandler<WindowDisplayChangedEventArgs> DisplayChanged;
    }
}
