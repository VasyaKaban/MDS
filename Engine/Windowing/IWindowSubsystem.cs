namespace Engine.Windowing
{
    public interface IWindowSubsystem : IDisposable
    {
        IWindow CreateWindow(in WindowCreateInfo info);

        void PollEvents();

        KeyboardKey? ScanCodeToKey(uint scanCode);

        uint? KeyToScanCode(KeyboardKey key);

        bool IsKeyPressed(uint scanCode);

        void AcquireMouseCursor(IWindow window);
        void ReleaseMouseCursor();

        static IWindowSubsystem CreateSubsystem()
        {
            if (OperatingSystem.IsWindowsVersionAtLeast(10, 0))
            {
                return new Win32.WindowSubsystem();
            }
            else
                throw new NotImplementedException("Only Win32NT is implemented");
        }
    }
}
