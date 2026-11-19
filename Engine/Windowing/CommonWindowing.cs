namespace Engine.Windowing
{
    public class WindowExtent
    {
        public WindowExtent(uint width, uint height)
        {
            Width = width;
            Height = height;
        }

        public uint Width { get; }
        public uint Height { get; }
    }

    public class WindowPosition
    {
        public const int DEFAULT_POSITION = int.MaxValue;

        public WindowPosition(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; }
        public int Y { get; }
    }

    public enum WindowState
    {
        Windowed,
        FullScreen
    }

    public class WindowCreateInfo
    {
        public WindowCreateInfo(string title, WindowExtent extent, WindowState state, bool textInputEnabled)
        {
            Title = title;
            Extent = extent;
            State = state;
            TextInputEnabled = textInputEnabled;
        }

        public string Title { get; }
        public WindowExtent Extent { get; }//client rect
        public WindowState State { get; }
        public bool TextInputEnabled { get; }
    }

    public abstract class WindowEventArgs
    {
        protected WindowEventArgs(ulong timeMs)
        {
            TimeMs = timeMs;
        }

        public ulong TimeMs { get; }
    }

    public class WindowClosedEventArgs : WindowEventArgs
    {
        public WindowClosedEventArgs(ulong timeMs) : base(timeMs)
        {
        }
    }

    public class WindowResizedEventArgs : WindowEventArgs
    {
        public WindowResizedEventArgs(ulong timeMs, WindowExtent extent) : base(timeMs)
        {
            Extent = extent;
        }

        public WindowExtent Extent { get; }
    }

    public class WindowMovedEventArgs : WindowEventArgs
    {
        public WindowMovedEventArgs(ulong timeMs, WindowPosition position) : base(timeMs)
        {
            Position = position;
        }

        public WindowPosition Position { get; }
    }

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

    public class WindowMouseFocusGainedEventArgs : WindowEventArgs
    {
        public WindowMouseFocusGainedEventArgs(ulong timeMs, WindowPosition cursorPosition, MouseButton buttons) : base(timeMs)
        {
            CursorPosition = cursorPosition;
            Buttons = buttons;
        }

        public WindowPosition CursorPosition { get; }//relative to the current window
        public MouseButton Buttons { get; }
    }

    public class WindowMouseFocusLeftEventArgs : WindowEventArgs
    {
        public WindowMouseFocusLeftEventArgs(ulong timeMs) : base(timeMs)
        {
        }
    }

    public class WindowKeyboardFocusGainedEventArgs : WindowEventArgs
    {
        public WindowKeyboardFocusGainedEventArgs(ulong timeMs) : base(timeMs)
        {
        }
    }

    public class WindowKeyboardFocusLeftEventArgs : WindowEventArgs
    {
        public WindowKeyboardFocusLeftEventArgs(ulong timeMs) : base(timeMs)
        {
        }
    }

    public class WindowMouseCursorMovedEventArgs : WindowEventArgs
    {
        public WindowMouseCursorMovedEventArgs(ulong timeMs, WindowPosition cursorPosition, WindowPosition directionDelta, MouseButton buttons) : base(timeMs)
        {
            CursorPosition = cursorPosition;
            DirectionDelta = directionDelta;
            Buttons = buttons;
        }

        public WindowPosition CursorPosition { get; }//relative to the current window
        public WindowPosition DirectionDelta { get; }//delta between previous position and current
        public MouseButton Buttons { get; }
    }

    public class WindowMouseButtonToggledEventArgs : WindowEventArgs
    {
        public WindowMouseButtonToggledEventArgs(ulong timeMs, WindowPosition cursorPosition, MouseButton button, uint clicks, bool isPressed) : base(timeMs)
        {
            CursorPosition = cursorPosition;
            Button = button;
            Clicks = clicks;
            IsPressed = isPressed;
        }

        public WindowPosition CursorPosition { get; }//relative to the current window
        public MouseButton Button { get; }
        public uint Clicks { get; }
        public bool IsPressed { get; }
    }

    public class WindowMouseWheelScrolledEventArgs : WindowEventArgs
    {
        public WindowMouseWheelScrolledEventArgs(ulong timeMs, WindowPosition cursorPosition, MouseButton buttons, float horizontalScrollDelta, float verticalScrollDelta) : base(timeMs)
        {
            CursorPosition = cursorPosition;
            Buttons = buttons;
            HorizontalScrollDelta = horizontalScrollDelta;
            VerticalScrollDelta = verticalScrollDelta;
        }

        public WindowPosition CursorPosition { get; }//relative to the current window
        public MouseButton Buttons { get; }
        public float HorizontalScrollDelta { get; }
        public float VerticalScrollDelta { get; }
    }

    [Flags]
    public enum KeyboardModifiers : uint
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

    public union KeyboardKey(SpecialKey, string);//either predefined key or key name based on available keyboard layouts

    public class WindowKeyboardKeyToggledEventArgs : WindowEventArgs
    {
        public WindowKeyboardKeyToggledEventArgs(ulong timeMs, uint scanCode, KeyboardModifiers modifiers, KeyboardKey key, bool isPressed, uint repeatCount) : base(timeMs)
        {
            ScanCode = scanCode;
            Modifiers = modifiers;
            Key = key;
            IsPressed = isPressed;
            RepeatCount = repeatCount;
        }

        public uint ScanCode { get; }
        public KeyboardModifiers Modifiers { get; }
        public KeyboardKey Key { get; }
        public bool IsPressed { get; }
        public uint RepeatCount { get; }
    }

    public class WindowKeyboardTextInputtedEventArgs : WindowEventArgs
    {
        public WindowKeyboardTextInputtedEventArgs(ulong timeMs, KeyboardModifiers modifiers, string text) : base(timeMs)
        {
            Modifiers = modifiers;
            Text = text;
        }

        public KeyboardModifiers Modifiers { get; }
        public string Text { get; }
    }

    public record VideoMode(uint Width, uint Height, uint RefreshRate);
}
