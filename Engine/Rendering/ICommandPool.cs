using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Rendering
{
    [Flags]
    public enum CommandPoolCreateFlags
    {
        None = 0,
        Transient = 1 << 0
    }

    public struct CommandPoolCreateInfo
    {
        public CommandPoolCreateFlags flags;
    }

    public interface ICommandPool : IDisposable
    {
        IRenderingContext GetContext();
        ICommandBuffer AllocateCommandBuffer();
    }
}
