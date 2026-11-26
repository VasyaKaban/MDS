using Engine.Windowing;

namespace Engine.Rendering
{
    [Flags]
    public enum DebugMessageSeverity
    {
        None = 0,
        Verbose = 1 << 0,
        Info = 1 << 1,
        Warning = 1 << 2,
        Error = 1 << 3
    }

    [Flags]
    public enum DebugMessageType
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

    public readonly record struct SwapchainCreateInfo(
        Format ImageFormat,
        PresentMode PresentMode
    );

    public delegate void DebugMessengerDelegate(DebugMessageSeverity severity, DebugMessageType type, string message);
    public delegate SwapchainCreateInfo SwapchainConfigureDelegate(ReadOnlySpan<Format> formats, ReadOnlySpan<PresentMode> presentModes);

    public readonly record struct RenderingContextCreateInfo(
        SwapchainConfigureDelegate SwapchainConfigure,
        DebugMessengerDelegate? DebugMessenger,
        DebugMessageSeverity DebugMessengerSeverityMask,
        DebugMessageType DebugMessengerTypeMask
    );

    [Flags]
    public enum FormatFeature
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
        MipMapGeneration = 1 << 8
    }

    public readonly record struct ImageFormatProperties(
        Extent3D MaxExtent,
        uint MaxMipLevels,
        uint MaxArrayLayers,
        SampleCount SampleCounts
    );

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

    //TODO: check availability of each limit
    public enum RenderingContextIntLimit
    {
        MinTexelOffset,
        MinTexelGatherOffset,
    }

    public enum RenderingContextUIntLimit
    {
        MaxImageDimension1D,
        MaxImageDimension2D,
        MaxImageDimension3D,
        MaxImageDimensionCube,
        MaxImageArrayLayers,
        MaxTexelBufferElements,
        MaxUniformBufferRange,
        MaxStorageBufferRange,
        MaxMemoryAllocationCount,
        MaxPushConstantsSize,
        MaxSamplerAllocationCount,
        MaxBoundDescriptorSets,
        MaxPerStageDescriptorSamplers,
        MaxPerStageDescriptorUniformBuffers,
        MaxPerStageDescriptorStorageBuffers,
        MaxPerStageDescriptorSampledImages,
        MaxPerStageDescriptorStorageImages,
        MaxPerStageDescriptorInputAttachments,
        MaxPerStageResources,
        MaxDescriptorSetSamplers,
        MaxDescriptorSetUniformBuffers,
        MaxDescriptorSetUniformBuffersDynamic,
        MaxDescriptorSetStorageBuffers,
        MaxDescriptorSetStorageBuffersDynamic,
        MaxDescriptorSetSampledImages,
        MaxDescriptorSetStorageImages,
        MaxDescriptorSetInputAttachments,
        MaxVertexInputAttributes,
        MaxVertexInputBindings,
        MaxVertexInputAttributeOffset,
        MaxVertexInputBindingStride,
        MaxVertexOutputComponents,
        MaxTessellationGenerationLevel,
        MaxTessellationPatchSize,
        MaxTessellationControlPerVertexInputComponents,
        MaxTessellationControlPerVertexOutputComponents,
        MaxTessellationControlPerPatchOutputComponents,
        MaxTessellationControlTotalOutputComponents,
        MaxTessellationEvaluationInputComponents,
        MaxTessellationEvaluationOutputComponents,
        MaxGeometryShaderInvocations,
        MaxGeometryInputComponents,
        MaxGeometryOutputComponents,
        MaxGeometryOutputVertices,
        MaxGeometryTotalOutputComponents,
        MaxFragmentInputComponents,
        MaxFragmentOutputAttachments,
        MaxFragmentDualSrcAttachments,
        MaxFragmentCombinedOutputResources,
        MaxComputeSharedMemorySize,
        MaxComputeWorkGroupCountX,
        MaxComputeWorkGroupCountY,
        MaxComputeWorkGroupCountZ,
        MaxComputeWorkGroupInvocations,
        MaxComputeWorkGroupSizeX,
        MaxComputeWorkGroupSizeY,
        MaxComputeWorkGroupSizeZ,
        SubPixelPrecisionBits,
        SubTexelPrecisionBits,
        MipmapPrecisionBits,
        MaxDrawIndexedIndexValue,
        MaxDrawIndirectCount,
        MaxViewports,
        MaxViewportWidth,
        MaxViewportHeight,
        ViewportSubPixelBits,
        MaxTexelOffset,
        MaxTexelGatherOffset,
        SubPixelInterpolationOffsetBits,
        MaxFramebufferWidth,
        MaxFramebufferHeight,
        MaxFramebufferLayers,
        MaxColorAttachments,
        MaxSampleMaskWords,
        MaxClipDistances,
        MaxCullDistances,
        MaxCombinedClipAndCullDistances,
        DiscreteQueuePriorities,
    }

    public enum RenderingContextFloatLimit
    {
        MaxSamplerLodBias,
        MaxSamplerAnisotropy,
        ViewportBoundsRangeStart,
        ViewportBoundsRangeEnd,
        MinInterpolationOffset,
        MaxInterpolationOffset,
        TimestampPeriod,
        PointSizeRangeStart,
        PointSizeRangeEnd,
        LineWidthRangeStart,
        LineWidthRangeEnd,
        PointSizeGranularity,
        LineWidthGranularity,
    }

    public enum RenderingContextUlongLimit
    {
        BufferImageGranularity,
        SparseAddressSpaceSize,
        MinMemoryMapAlignment,
        MinTexelBufferOffsetAlignment,
        MinUniformBufferOffsetAlignment,
        MinStorageBufferOffsetAlignment,
        OptimalBufferCopyOffsetAlignment,
        OptimalBufferCopyRowPitchAlignment,
        NonCoherentAtomSize
    }

    public enum RenderingContextSampleCountLimit
    {
        FramebufferColorSampleCounts,
        FramebufferDepthSampleCounts,
        FramebufferStencilSampleCounts,
        FramebufferNoAttachmentsSampleCounts,
        SampledImageColorSampleCounts,
        SampledImageIntegerSampleCounts,
        SampledImageDepthSampleCounts,
        SampledImageStencilSampleCounts,
        StorageImageSampleCounts,
    }

    public enum RenderingContextBoolLimit
    {
        TimestampComputeAndGraphics,
        StrictLines,
        StandardSampleLocations,
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
        IFence CreateFence();

        //Swapchain
        PresentMode[] GetSupportedPresentModes();
        Format GetSwapchainImageFormat();
        void SetPresentMode(PresentMode presentMode);
        IFramebuffer GetCurrentSwapchainFramebuffer();
        void SwapBuffers();

        //Submit
        void Submit(Span<ICommandBuffer> commandBuffers, out IFence? fence);
        void Wait();

        //Properties/Features
        bool IsFeatureSupported(RenderingContextFeature feature);
        string GetDeviceName();
        int GetLimit(RenderingContextIntLimit limit);
        uint GetLimit(RenderingContextUIntLimit limit);
        float GetLimit(RenderingContextFloatLimit limit);
        ulong GetLimit(RenderingContextUlongLimit limit);
        SampleCount GetLimit(RenderingContextSampleCountLimit limit);
        bool GetLimit(RenderingContextBoolLimit limit);

        //Query
        FormatFeature QueryBufferFormatSupport(Format format);
        FormatFeature QueryImageFormatSupport(Format format, ImageType imageType, ImageCreateFlags flags);
        ImageFormatProperties QueryImageFormatProperties(
            Format format,
            ImageType type,
            ImageUsage usage,
            ImageCreateFlags flags);
    }
}
