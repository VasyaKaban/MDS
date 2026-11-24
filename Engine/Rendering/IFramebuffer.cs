using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Rendering
{
    public struct FrambufferCreateInfo
    {
        public ReadOnlyMemory<IImage> colorAttachments;
        public IImage? depthStencilAttachment;
    }

    public interface IFramebuffer : IDisposable
    {
        IRenderingContext GetContext();
    }
}
