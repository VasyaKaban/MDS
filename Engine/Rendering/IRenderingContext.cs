using Engine.Windowing;

namespace Engine.Rendering
{
    [Flags]
    public enum DebugMessageSeverityFlags
    {
        None = 0,
        Verbose = 1 << 0,
        Info = 1 << 1,
        Warning = 1 << 2,
        Error = 1 << 3
    }

    [Flags]
    public enum DebugMessageTypeFlags
    {
        None = 0,
        General = 1 << 0,
        Validation = 1 << 1,
        Performance = 1 << 2
    }

    public enum PresentMode
    {
        Immediate,
        Mailbox,
        FirstInFirstOut,
        FirstInFirstOutRelaxed
    }

    public struct SwapchainCreateInfo
    {
        Format imageFormat;
        PresentMode presentMode;
    }

    delegate void DebugMessengerDelegate(DebugMessageSeverityFlags severity, DebugMessageTypeFlags type, string message);
    delegate SwapchainCreateInfo SwapchainConfigureDelegate(ReadOnlySpan<Format> formats, ReadOnlySpan<PresentMode> presentModes);

    public struct RenderingContextCreateInfo
    {
        SwapchainConfigureDelegate swapchainConfigure;
        DebugMessengerDelegate? debugMessenger;
        DebugMessageSeverityFlags severityMask;
        DebugMessageTypeFlags typeMask;
    }

    [Flags]
    public enum FormatFeatureFlags
    {
        None = 0,
        SampledImage = 1 << 0,
        StorageImage = 1 << 1,
        StorageImageAtomic = 1 << 2,
        VertexBuffer = 1 << 3,
        ColorAttachment = 1 << 4,
        ColorAttachmentBlend = 1 << 5,
        DepthStencilAttachment = 1 << 6,
        SampledImageFilterLinear = 1 << 7,
    }

    public struct ImageFormatProperties
    {
        public Extent3D maxExtent;
        public uint maxMipLevels;
        public uint maxArrayLayers;
        public SampleCountFlags sampleCounts;
    }

    public enum RenderingContextFeature
    {
        FullDrawIndexUint32,
        ImageCubeArray,
        IndependentBlend,
        GeometryShader,
        TessellationShader,
        SampleRateShading,
        DualSourceBlend,
        LogicOperation,
        MultiDrawIndirect,
        DrawIndirectFirstInstance,
        DepthClamp,
        DepthBiasClamp,
        FillModeNonSolid,
        DepthBounds,
        WideLines,
        LargePoints,
        AlphaToOne,
        MultiViewport,
        SamplerAnisotropy,
        VertexPipelineStoresAndAtomics,
        FragmentStoresAndAtomics,
        ShaderTessellationAndGeometryPointSize,
        ShaderImageGatherExtended,
        ShaderStorageImageMultisample,
        ShaderStorageImageReadWithoutFormat,
        ShaderStorageImageWriteWithoutFormat,
        ShaderUniformBufferArrayDynamicIndexing,
        ShaderSampledImageArrayDynamicIndexing,
        ShaderStorageBufferArrayDynamicIndexing,
        ShaderStorageImageArrayDynamicIndexing,
        ShaderClipDistance,
        ShaderCullDistance,
        ShaderFloat64,
        ShaderInt64,
        ShaderInt16,
        ShaderResourceMinLod
    }

    public struct RenderingContextProperties
    {
        uint apiMajorVersion;
        uint apiMinorVersion;
        string deviceName;
    }

    public interface IRenderingContext : IDisposable
    {
        IWindow GetWindow();

        //Objects
        IBuffer CreateBuffer(in BufferCreateInfo info, in BufferInitialData? initialData);
        IImage CreateImage(in ImageCreateInfo info, in ImageInitialData? initialData);
        IImageView CreateImageView(in ImageViewCreateInfo info);
        ISampler CreateSampler(in SamplerCreateInfo info);
        IShader CreateShader(in ShaderCreateInfo info);
        ICommandPool CreateCommandPool(in CommandPoolCreateInfo info);
        IFramebuffer CreateFramebuffer(in FrambufferCreateInfo info);
        IPipeline CreateGraphicsPipeline(in GraphicsPipelineCreateInfo info);
        IPipeline CreateComputePipeline(in ComputePipelineCreateInfo info);

        //Swapchain
        PresentMode[] GetSupportedPresentModes();
        Format GetSwapchainImageFormat();
        IFramebuffer GetCurrentSwapchainFramebuffer();
        void SwapBuffers();

        //Submit
        void Submit(Span<ICommandBuffer> commandBuffers, ref IFence? fence);
        void Wait();

        //Properties/Features
        //TODO: Limits?
        bool IsFeatureSupported(RenderingContextFeature feature);
        void Getproperties(out RenderingContextProperties properties);

        //Query
        FormatFeatureFlags QueryBufferFormatSupport(Format format);
        FormatFeatureFlags QueryImageFormatSupport(Format format, ImageType imageType, ImageCreateFlags flags);
        void QueryImageFormatProperties(
            Format format,
            ImageType type,
            ImageUsageFlags usage,
            ImageCreateFlags flags,
            out ImageFormatProperties properties);
    }
}
