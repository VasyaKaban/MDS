using System.Runtime.InteropServices;

namespace Engine.Windowing
{
    public interface IWindowSubsystem : IDisposable
    {
        IWindow CreateWindow(in WindowCreateInfo info);

        void PollEvents();
         
        KeyaboardKey ScanCodeToKey(uint scanCode);

        uint KeyToScanCode(KeyaboardKey key);
    }
}
