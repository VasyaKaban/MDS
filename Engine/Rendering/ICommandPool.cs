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

    public readonly record struct CommandPoolCreateInfo(CommandPoolCreateFlags Flags);

    public interface ICommandPool : IDisposable
    {
        IRenderingContext GetContext();
        ICommandBuffer AllocateCommandBuffer();
    }
}
