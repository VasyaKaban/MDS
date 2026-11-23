namespace Engine.Windowing
{
    public record struct WindowExtent(uint Width, uint Height);

    public record struct WindowPosition(int X, int Y)
    {
        public const int DEFAULT_POSITION = int.MaxValue;
    }

    public struct WindowCreateInfo
    {
        public string Title { get; set; }
        public WindowExtent Extent { get; set; }//client rect
        public bool TextInputEnabled { get; set; }
    }

    public closed class WindowEventArgs(ulong timeMs)
    {
        public ulong TimeMs { get; } = timeMs;
    }

    public class WindowClosedEventArgs(ulong timeMs) : WindowEventArgs(timeMs)
    {
    }

    public class WindowResizedEventArgs(ulong timeMs, WindowExtent extent) : WindowEventArgs(timeMs)
    {
        public WindowExtent Extent { get; } = extent;
    }

    public class WindowMovedEventArgs(ulong timeMs, WindowPosition position) : WindowEventArgs(timeMs)
    {
        public WindowPosition Position { get; } = position;
    }

    [Flags]
    public enum MouseButtons : uint
    {
        Left = 1 << 0,
        Right = 1 << 1,
        Middle = 1 << 2,
        X1 = 1 << 3,
        X2 = 1 << 4
    }

    public class WindowMouseFocusGainedEventArgs(ulong timeMs, WindowPosition cursorPosition, MouseButtons buttons) : WindowEventArgs(timeMs)
    {
        public WindowPosition CursorPosition { get; } = cursorPosition;
        public MouseButtons Buttons { get; } = buttons;
    }

    public class WindowMouseFocusLeftEventArgs(ulong timeMs) : WindowEventArgs(timeMs)
    {
    }

    public class WindowKeyboardFocusGainedEventArgs(ulong timeMs) : WindowEventArgs(timeMs)
    {
    }

    public class WindowKeyboardFocusLeftEventArgs(ulong timeMs) : WindowEventArgs(timeMs)
    {
    }

    public class WindowMouseCursorMovedEventArgs(
        ulong timeMs,
        WindowPosition cursorPosition,
        WindowPosition directionDelta,
        MouseButtons buttons) : WindowEventArgs(timeMs)
    {
        public WindowPosition CursorPosition { get; } = cursorPosition;
        public WindowPosition DirectionDelta { get; } = directionDelta;
        public MouseButtons Buttons { get; } = buttons;
    }

    public class WindowMouseButtonToggledEventArgs(
        ulong timeMs,
        WindowPosition cursorPosition,
        MouseButtons button,
        uint clicks,
        bool isPressed) : WindowEventArgs(timeMs)
    {
        public WindowPosition CursorPosition { get; } = cursorPosition;
        public MouseButtons Button { get; } = button;
        public uint Clicks { get; } = clicks;
        public bool IsPressed { get; } = isPressed;
    }

    public class WindowMouseWheelScrolledEventArgs(
        ulong timeMs,
        WindowPosition cursorPosition,
        MouseButtons buttons,
        float horizontalScrollDelta,
        float verticalScrollDelta) : WindowEventArgs(timeMs)
    {
        public WindowPosition CursorPosition { get; } = cursorPosition;
        public MouseButtons Buttons { get; } = buttons;
        public float HorizontalScrollDelta { get; } = horizontalScrollDelta;
        public float VerticalScrollDelta { get; } = verticalScrollDelta;
    }

    [Flags]
    public enum KeyboardModifiers : uint
    {
        LeftShift = 1 << 0,
        RightShift = 1 << 1,
        LeftControl = 1 << 2,
        RightControl = 1 << 3,
        LeftAlt = 1 << 4,
        RightAlt = 1 << 5,
        LeftGUI = 1 << 6,//Win
        RightGUI = 1 << 7
    }

    public enum SpecialKey : uint
    {
        Unknown = 0,
        Enter,
        Escape,
        BackSpace,
        Tab,
        Space,
        CapsLock,
        F1,
        F2,
        F3,
        F4,
        F5,
        F6,
        F7,
        F8,
        F9,
        F10,
        F11,
        F12,
        PrintScreen,
        SysRq,
        ScrollLock,
        Break,
        Pause,
        Insert,
        Home,
        PageUp,
        Delete,
        End,
        PageDown,
        RightArrow,
        LeftArrow,
        DownArrow,
        UpArrow,
        NumLock,
        KeypadDivide,
        KeypadMultiply,
        KeypadSubtract,
        KeypadAdd,
        KeypadEnter,
        Keypad1,
        Keypad2,
        Keypad3,
        Keypad4,
        Keypad5,
        Keypad6,
        Keypad7,
        Keypad8,
        Keypad9,
        Keypad0,
        KeypadPeriod,
        Menu,
        KeyboardPower,
        KeypadEqual,
        F13,
        F14,
        F15,
        F16,
        F17,
        F18,
        F19,
        F20,
        F21,
        F22,
        F23,
        F24,
        BrazilKeypad,
        Ro, //Keyboard Int'l 1
        Kana, //Keyboard Intl'2
        Yen, //Keyboard Int'l 3
        Henkan, //Keyboard Int'l 4
        Muhenkan, //Keyboard Int'l 5
        PC9800KeypadPeriod, //Keyboard Int'l 6
        Hanguel, //Keyboard Lang 1
        Hanja, //Keyboard Lang 2
        Katakana, //Keyboard Lang 3
        Hiragana, //Keyboard Lang 4
        ZenkakuHankaku, //Keyboard Lang 5
        LeftControl,
        LeftShift,
        LeftAlt,
        LeftGUI,
        RightControl,
        RightShift,
        RightAlt,
        RightGUI,
        ScanNextTrack,
        ScanPreviousTrack,
        Stop,
        PlayPause,
        Mute,
        VolumeUp,
        VolumeDown,
        MediaSelect,
        Mail,
        Calculator,
        MyComputer,
        WWWSearch,
        WWWHome,
        WWWBack,
        WWWForward,
        WWWStop,
        WWWRefresh,
        WWWFavorites
    }

    public union KeyboardKey(SpecialKey, string);//either predefined key or key name based on available keyboard layouts

    public class WindowKeyboardKeyToggledEventArgs(
        ulong timeMs,
        uint scanCode,
        KeyboardModifiers modifiers,
        KeyboardKey key,
        bool isPressed,
        uint repeatCount) : WindowEventArgs(timeMs)
    {
        public uint ScanCode { get; } = scanCode;
        public KeyboardModifiers Modifiers { get; } = modifiers;
        public KeyboardKey Key { get; } = key;
        public bool IsPressed { get; } = isPressed;
        public uint RepeatCount { get; } = repeatCount;
    }

    public class WindowKeyboardTextInputtedEventArgs(ulong timeMs, KeyboardModifiers modifiers, string text) : WindowEventArgs(timeMs)
    {
        public KeyboardModifiers Modifiers { get; } = modifiers;
        public string Text { get; } = text;
    }

    public record VideoMode(uint Width, uint Height, uint RefreshRate);

    public class WindowDisplayChangedEventArgs(ulong timeMs) : WindowEventArgs(timeMs)
    {
    }
}
