using System.Runtime.InteropServices;

namespace Engine.Windowing
{
    public interface IWindowSubsystem : IDisposable
    {
        IWindow CreateWindow(in WindowCreateInfo info);

        void PollEvents();
         
        KeyaboardKey ScanCodeToKey(uint scanCode);

        uint KeyToScanCode(KeyaboardKey key);

        static IWindowSubsystem CreateSubsystem()
        {
            if (Environment.OSVersion.Platform == PlatformID.Win32NT)
                return new Win32.WindowSubsystem();
            else
                throw new NotImplementedException("Only Win32 platform is implemented")
        }
    }
}
