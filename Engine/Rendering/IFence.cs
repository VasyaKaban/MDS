using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Rendering
{
    public interface IFence : IDisposable
    {
        IRenderingContext GetContext();
        void Wait(ulong timeoutNs);
    }
}
