using Engine.Windowing;

namespace Engine.Rendering
{
    /*
     RenderingContextCreateInfo:
        bool createDebugMessnger
        void DebugMessngerPFN;

     */

    public interface IRenderingContext : IDisposable
    {
        IWindow GetWindow();

        IBuffer CreateBuffer(in BufferCreateInfo info, in BufferInitialData? initialData);
        IImage CreateImage(in ImageCreateInfo info, in ImageInitialData? initialData);
        IImageView CreateImageView(in ImageViewCreateInfo info);
        ISampler CreateSampler(in SamplerCreateInfo info);
        IShader CreateShader(in ShaderCreateInfo info);
        //pipeline
        //framebuffer
        //command pool
        //command buffer(via pool)
        //+swapchain(flush + fence)
        //+queries(formats)
        //+properties
    }
}
