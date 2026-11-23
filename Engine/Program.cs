using Engine.Windowing;

namespace Engine
{
    class Program
    {
        public static void Main(string[] args)
        {
            using IWindowSubsystem subsystem = IWindowSubsystem.CreateSubsystem();
            using IWindow window = subsystem.CreateWindow(
                new WindowCreateInfo
                {
                    Title = "amogus",
                    Extent = new WindowExtent(800, 600),
                    TextInputEnabled = false
                });

            ulong eventId = 0;

            window.Closed += (_, args) =>
            {
                Console.Out.WriteLine($"#{eventId++};=========================================================================");
                Console.Out.WriteLine($"Closed: {args.TimeMs}ms");
            };

            /*window.Resized += (_, args) =>
            {
                Console.Out.WriteLine($"#{eventId++};=========================================================================");
                Console.Out.WriteLine($"Resized: {args.TimeMs}ms");
                Console.Out.WriteLine($"Extent: {args.Extent.Width}; {args.Extent.Height}");
            };

            window.Moved += (_, args) =>
            {
                Console.Out.WriteLine($"#{eventId++};=========================================================================");
                Console.Out.WriteLine($"Moved: {args.TimeMs}ms");
                Console.Out.WriteLine($"Position: {args.Position.X}; {args.Position.Y}");
            };

            window.MouseFocusGained += (_, args) =>
            {
                Console.Out.WriteLine($"#{eventId++};=========================================================================");
                Console.Out.WriteLine($"MouseFocusGained: {args.TimeMs}ms");
                Console.Out.WriteLine($"CursorPosition: {args.CursorPosition.X}; {args.CursorPosition.Y}");
                Console.Out.WriteLine($"Buttons: {args.Buttons}");
            };

            window.MouseFocusLeft += (_, args) =>
            {
                Console.Out.WriteLine($"#{eventId++};=========================================================================");
                Console.Out.WriteLine($"MouseFocusLeft: {args.TimeMs}ms");
            };

            window.KeyboardFocusGained += (_, args) =>
            {
                Console.Out.WriteLine($"#{eventId++};=========================================================================");
                Console.Out.WriteLine($"KeyboardFocusGained: {args.TimeMs}ms");
            };

            window.KeyboardFocusLeft += (_, args) =>
            {
                Console.Out.WriteLine($"#{eventId++};=========================================================================");
                Console.Out.WriteLine($"KeyboardFocusLeft: {args.TimeMs}ms");
            };

            window.MouseCursorMoved += (_, args) =>
            {
                Console.Out.WriteLine($"#{eventId++};=========================================================================");
                Console.Out.WriteLine($"MouseCursorMoved: {args.TimeMs}ms");
                Console.Out.WriteLine($"CursorPosition: {args.CursorPosition.X}; {args.CursorPosition.Y}");
                Console.Out.WriteLine($"DirectionDelta: {args.DirectionDelta.X}; {args.DirectionDelta.Y}");
                Console.Out.WriteLine($"Buttons: {args.Buttons}");
            };

            window.MouseButtonToggled += (_, args) =>
            {
                Console.Out.WriteLine($"#{eventId++};=========================================================================");
                Console.Out.WriteLine($"MouseButtonToggled: {args.TimeMs}ms");
                Console.Out.WriteLine($"CursorPosition: {args.CursorPosition.X}; {args.CursorPosition.Y}");
                Console.Out.WriteLine($"Button: {args.Button}");
                Console.Out.WriteLine($"IsPressed: {args.IsPressed}");
            };

            window.MouseWheelScrolled += (_, args) =>
            {
                Console.Out.WriteLine($"#{eventId++};=========================================================================");
                Console.Out.WriteLine($"MouseWheelScrolled: {args.TimeMs}ms");
                Console.Out.WriteLine($"CursorPosition: {args.CursorPosition.X}; {args.CursorPosition.Y}");
                Console.Out.WriteLine($"VerticalScrollDelta: {args.VerticalScrollDelta}");
                Console.Out.WriteLine($"HorizontalScrollDelta: {args.HorizontalScrollDelta}");
                Console.Out.WriteLine($"Buttons: {args.Buttons}");
            };*/

            window.KeyboardKeyToggled += (win, args) =>
            {
                if (!args.IsPressed)
                    return;

                if (args.Key is SpecialKey.Enter)
                {
                    var modes = win.GetAvailableDisplayVideoModes();
                    foreach (var mode in modes)
                    {
                        if (mode.Width >= 2560 && mode.Height >= 1440 && mode.RefreshRate > 90)
                        {
                            win.SetFullscreenMode(mode);
                            break;
                        }
                    }

                }
                else if (args.Key is SpecialKey.BackSpace)
                {
                    win.SetWindowedMode();
                }

                /*Console.Out.WriteLine($"#{eventId++};=========================================================================");
                Console.Out.WriteLine($"KeyboardKeyToggled: {args.TimeMs}ms");
                Console.Out.WriteLine($"ScanCode: {args.ScanCode:x}");
                Console.Out.WriteLine($"Modifiers: {args.Modifiers}");
                Console.Out.WriteLine($"IsPressed: {args.IsPressed}");
                Console.Out.WriteLine($"RepeatCount: {args.RepeatCount}");
                switch (args.Key)
                {
                    case SpecialKey specialKey:
                        Console.Out.WriteLine($"SpecialKey: {specialKey}");
                        break;
                    case string keyName:
                        Console.Out.WriteLine($"KeyName: {keyName}");
                        break;
                }*/
            };

            Stack<(int, int)> consolePositionsState = new();

            /*window.KeyboardTextInputted += (_, args) =>
            {
                foreach(Rune rune in args.Text.EnumerateRunes())
                {
                    if (rune.Value == '\r')
                    {
                        consolePositionsState.Push((Console.CursorLeft, Console.CursorTop));
                        Console.Out.WriteLine();
                    }
                    else if (rune.Value == '\b')
                    {
                        if(Console.CursorLeft != 0)
                            Console.Out.Write("\b \b");
                        else
                        {
                            if (consolePositionsState.Count != 0)
                            {
                                var (left, top) = consolePositionsState.Peek();
                                Console.SetCursorPosition(left, top);
                                if(left != 0)
                                    Console.Out.Write("\b \b");
                            }
                        }
                    }
                    else
                        Console.Out.Write(rune);
                }

                //Console.Out.WriteLine($"#{eventId++};=========================================================================");
                //Console.Out.WriteLine($"KeyboardTextInputted: {args.TimeMs}ms");
                //Console.Out.WriteLine($"Modifiers: {args.Modifiers}");
                //Console.Out.WriteLine($"Text: {args.Text}");
            };*/

            /*window.DisplayChanged += (win, args) =>
            {
                var currentVideoMode = win.GetCurrentDisplayVideoMode();
                var displayModes = win.GetAvailableDisplayVideoModes();

                Console.Out.WriteLine($"#{eventId++};=========================================================================");
                Console.Out.WriteLine($"DisplayChanged: {args.TimeMs}ms");
                Console.Out.WriteLine($"Current: {currentVideoMode.Width}x{currentVideoMode.Height}:{currentVideoMode.RefreshRate}Hz");
                foreach (var (mode, index) in displayModes.Select((mode, index) => (mode, index)))
                {
                    Console.Out.WriteLine($"Mode({index}): {mode.Width}x{mode.Height}:{mode.RefreshRate}Hz");
                }

            };*/

            while (!window.IsClosed)
            {
                subsystem.PollEvents();
            }
        }
    }
}
