using Engine.Windowing;
using System;
using System.Collections.Generic;
using System.Text;

namespace Engine
{
    class Program
    {
        public static void Main(string[] args)
        {
            using IWindowSubsystem subsystem = IWindowSubsystem.CreateSubsystem();
            using IWindow window = subsystem.CreateWindow(new WindowCreateInfo("amogus", new WindowExtent(800, 600), WindowState.Windowed, false));

            window.Closed += (_, args) =>
            {
                Console.Out.WriteLine($"Closed: {args.TimeMs}ms");
            };

            window.Resized += (_, args) =>
            {
                Console.Out.WriteLine($"Resized: {args.TimeMs}ms");
                Console.Out.WriteLine($"Extent: {args.Extent.Width}; {args.Extent.Height}");
            };

            window.Moved += (_, args) =>
            {
                Console.Out.WriteLine($"Moved: {args.TimeMs}ms");
                Console.Out.WriteLine($"Position: {args.Position.X}; {args.Position.Y}");
            };

            window.MouseFocusGained += (_, args) =>
            {
                Console.Out.WriteLine($"MouseFocusGained: {args.TimeMs}ms");
                Console.Out.WriteLine($"CursorPosition: {args.CursorPosition.X}; {args.CursorPosition.Y}");
                Console.Out.WriteLine($"Buttons: {args.Buttons}");
            };

            window.MouseFocusLeft += (_, args) =>
            {
                Console.Out.WriteLine($"MouseFocusLeft: {args.TimeMs}ms");
            };

            window.KeyboardFocusGained += (_, args) =>
            {
                Console.Out.WriteLine($"KeyboardFocusGained: {args.TimeMs}ms");
            };

            window.KeyboardFocusLeft += (_, args) =>
            {
                Console.Out.WriteLine($"KeyboardFocusLeft: {args.TimeMs}ms");
            };

            window.MouseCursorMoved += (_, args) =>
            {
                Console.Out.WriteLine($"MouseCursorMoved: {args.TimeMs}ms");
                Console.Out.WriteLine($"CursorPosition: {args.CursorPosition.X}; {args.CursorPosition.Y}");
                Console.Out.WriteLine($"DirectionDelta: {args.DirectionDelta.X}; {args.DirectionDelta.Y}");
                Console.Out.WriteLine($"Buttons: {args.Buttons}");
            };

            window.MouseButtonToggled += (_, args) =>
            {
                Console.Out.WriteLine($"MouseButtonToggled: {args.TimeMs}ms");
                Console.Out.WriteLine($"CursorPosition: {args.CursorPosition.X}; {args.CursorPosition.Y}");
                Console.Out.WriteLine($"Button: {args.Button}");
                Console.Out.WriteLine($"IsPressed: {args.IsPressed}");
            };

            window.MouseWheelScrolled += (_, args) =>
            {
                Console.Out.WriteLine($"MouseWheelScrolled: {args.TimeMs}ms");
                Console.Out.WriteLine($"CursorPosition: {args.CursorPosition.X}; {args.CursorPosition.Y}");
                Console.Out.WriteLine($"VerticalScrollDelta: {args.VerticalScrollDelta}");
                Console.Out.WriteLine($"HorizontalScrollDelta: {args.HorizontalScrollDelta}");
                Console.Out.WriteLine($"Buttons: {args.Buttons}");
            };

            while(!window.IsClosed)
            {
                subsystem.PollEvents();
            }
        }
    }
}
