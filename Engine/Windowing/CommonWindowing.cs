namespace Engine.Windowing
{
    public readonly record struct WindowExtent(uint Width, uint Height);

    public readonly record struct WindowPosition(int X, int Y)
    {
        public const int DEFAULT_POSITION = int.MaxValue;
    }

    public readonly record struct WindowCreateInfo(
        string Title,
        WindowExtent Extent,
        bool TextInputEnabled);

    public readonly record struct WindowClosedEventArgs(ulong TimeMs);

    public readonly record struct WindowResizedEventArgs(ulong TimeMs, WindowExtent Extent);

    public readonly record struct WindowMovedEventArgs(ulong TimeMs, WindowPosition Position);

    [Flags]
    public enum MouseButton : uint
    {
        None = 0,
        Left = 1 << 0,
        Right = 1 << 1,
        Middle = 1 << 2,
        X1 = 1 << 3,
        X2 = 1 << 4
    }

    public readonly record struct WindowMouseFocusGainedEventArgs(
        ulong TimeMs,
        WindowPosition CursorPosition,
        MouseButton Buttons);

    public readonly record struct WindowMouseFocusLeftEventArgs(ulong TimeMs);

    public readonly record struct WindowKeyboardFocusGainedEventArgs(ulong TimeMs);

    public readonly record struct WindowKeyboardFocusLeftEventArgs(ulong TimeMs);

    public readonly record struct WindowMouseCursorMovedEventArgs(
        ulong TimeMs,
        WindowPosition CursorPosition,
        WindowPosition DirectionDelta,
        MouseButton Buttons);

    public readonly record struct WindowMouseButtonToggledEventArgs(
        ulong TimeMs,
        WindowPosition CursorPosition,
        MouseButton Button,
        uint Clicks,
        bool IsPressed);

    public readonly record struct WindowMouseWheelScrolledEventArgs(
        ulong TimeMs,
        WindowPosition CursorPosition,
        MouseButton Buttons,
        float HorizontalScrollDelta,
        float VerticalScrollDelta);

    [Flags]
    public enum KeyboardModifier : uint
    {
        None = 0,
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

    public readonly union KeyboardKey(SpecialKey, string);//either predefined key or key name based on available keyboard layouts

    public readonly record struct WindowKeyboardKeyToggledEventArgs(
        ulong TimeMs,
        uint ScanCode,
        KeyboardModifier Modifiers,
        KeyboardKey Key,
        bool IsPressed,
        uint RepeatCount);

    public readonly record struct WindowKeyboardTextInputtedEventArgs(
        ulong TimeMs,
        KeyboardModifier Modifiers,
        string Text);

    public readonly record struct VideoMode(uint Width, uint Height, uint RefreshRate);

    public readonly record struct WindowDisplayChangedEventArgs(ulong TimeMs);

    public readonly union EventArgs(
        WindowClosedEventArgs,
        WindowResizedEventArgs,
        WindowMovedEventArgs,
        WindowMouseFocusGainedEventArgs,
        WindowMouseFocusLeftEventArgs,
        WindowKeyboardFocusGainedEventArgs,
        WindowKeyboardFocusLeftEventArgs,
        WindowMouseCursorMovedEventArgs,
        WindowMouseButtonToggledEventArgs,
        WindowMouseWheelScrolledEventArgs,
        WindowKeyboardKeyToggledEventArgs,
        WindowKeyboardTextInputtedEventArgs,
        WindowDisplayChangedEventArgs);
}
