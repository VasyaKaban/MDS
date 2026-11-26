using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Rendering
{
    public readonly record struct FrambufferCreateInfo(
        ReadOnlyMemory<IImage> ColorAttachments,
        IImage? DepthStencilAttachment
    );

    public interface IFramebuffer : IDisposable
    {
        IRenderingContext GetContext();
    }
}
