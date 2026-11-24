using System;
using System.Collections.Generic;
using System.Text;

namespace Engine.Rendering
{
    [Flags]
    public enum CommandBufferUsageFlags
    {
        None = 0,
        OneTimeSubmit = 1 << 0
    }

    public struct CommandBufferBeginInfo
    {
        public CommandBufferUsageFlags flags;
    }

    public struct DrawIndirectCommand
    {
        public uint vertexCount;
        public uint instanceCount;
        public uint firstVertex;
        public uint firstInstance;
    }

    public struct DrawIndexedIndirectCommand
    {
        public uint indexCount;
        public uint instanceCount;
        public uint firstIndex;
        public int vertexOffset;
        public uint firstInstance;
    }

    struct DispatchIndirectCommand
    {
        public uint x;
        public uint y;
        public uint z;
    }

    public enum PushConstantType
    {
        Bool,
        Int,
        UInt,
        Float,
        Float64,
        Int64
    }

    public enum PushConstantExtent
    {
        Scalar,
        Vector2,
        Vector3,
        Vector4,
        Matrix2x2,
        Matrix2x3,
        Matrix2x4,
        Matrix3x2,
        Matrix3x3,
        Matrix3x4,
        Matrix4x2,
        Matrix4x3,
        Matrix4x4
    }

    public struct PushConstantRangeDesc
    {
        public PushConstantType type;
        public PushConstantExtent extent;
        public uint binding;
        public uint count;
        public ulong dataOffset;
    }

    public ref struct PushConstantDesc
    {
        public ReadOnlySpan<PushConstantRangeDesc> ranges;
        public ReadOnlySpan<byte> data;
    }

    public struct SampledImageDesc
    {
        public IImageView imageView;
        public ISampler sampler;
    }

    public struct BufferDesc
    {
        public IBuffer buffer;
        public ulong offset;
        public ulong size;
    }

    public struct ClearColorFloat
    {
        public float red;
        public float green;
        public float blue;
        public float alpha;
    }

    public struct ClearColorInt
    {
        public int red;
        public int green;
        public int blue;
        public int alpha;
    }

    public struct ClearColorUInt
    {
        public uint red;
        public uint green;
        public uint blue;
        public uint alpha;
    }

    public union ClearColorValue(ClearColorFloat, ClearColorInt, ClearColorUInt);

    public struct ClearDepthStencilValue
    {
        public float depth;
        public uint stencil;
    }

    public enum AttachmentLoadOperation
    {
        Load,
        Clear,
        DoNotCare
    }

    public enum AttachmentStoreOperation
    {
        Store,
        DoNotCare
    }

    public union ClearValue(ClearColorValue, ClearDepthStencilValue);

    public struct RenderingAttachmentInfo
    {
        public AttachmentLoadOperation loadOperation;
        public AttachmentStoreOperation storeOperation;
        public ClearValue clearValue;
    }

    public ref struct RenderingInfo
    {
        public IFramebuffer framebuffer;
        public ReadOnlySpan<RenderingAttachmentInfo> colorAttachments;
        public RenderingAttachmentInfo? depthStencilAttachment;
    }

    public struct VertexBufferDesc
    {
        public IBuffer buffer;
        public ulong offset;
    }

    public enum IndexType
    {
        UInt16,
        UInt32
    }

    [Flags]
    public enum StencilFaceFlags
    {
        None = 0,
        Front = 1 << 0,
        Back = 1 << 1
    }

    [Flags]
    public enum PipelineStageFlags
    {
        None = 0,
        DrawIndirect = 1 << 0,
        VertexInput = 1 << 1,
        VertexShader = 1 << 2,
        TessellationControlShader = 1 << 3,
        TessellationEvaluationShader = 1 << 4,
        GeometryShader = 1 << 5,
        FragmentShader = 1 << 6,
        EarlyFragmentTests = 1 << 7,
        LateFragmentTests = 1 << 8,
        ColorAttachmentOutput = 1 << 9,
        ComputeShader = 1 << 10,
        AllTransfer = 1 << 11,
        Host = 1 << 12,
        AllGraphics = 1 << 13,
        AllCommands = 1 << 14,
        Copy = 1 << 15,
        Clear = 1 << 16,
        IndexInput = 1 << 17,
        VertexAttributeInput =  1 << 18,
        PreRasterizationShaders = 1 << 19
    }

    [Flags]
    public enum AccessFlags
    {
        None = 0,
        IndirectCommandRead = 1 << 0,
        IndexRead = 1 << 1,
        VertexAttributeRead = 1 << 2,
        UniformRead = 1 << 3,
        ShaderRead = 1 << 4,
        ShaderWrite = 1 << 5,
        ColorAttachmentRead = 1 << 6,
        ColorAttachmentWrite= 1 << 7,
        DepthStencilAttachmentRead = 1 << 8,
        DepthStencilAttachmentWrite = 1 << 9,
        TransferRead = 1 << 10,
        TransferWrite = 1 << 11,
        HostRead = 1 << 12,
        HostWrite = 1 << 13,
        MemoryRead = 1 << 14,
        MemoryWrite = 1 << 15,
        ShaderSampledRead = 1 << 16,
        ShaderStorageRead = 1 << 17,
        ShaderStorageWrite = 1 << 18
    }

    public struct MemoryBarrier
    {
        public PipelineStageFlags srcStageMask;
        public AccessFlags srcAccessMask;
        public PipelineStageFlags dstStageMask;
        public AccessFlags dstAccessMask;
    }

    public interface ICommandBuffer : IDisposable
    {
        void Begin(in CommandBufferBeginInfo info);
        void End();

        //State
        void BeginRendering(in RenderingInfo info);
        void EndRendering();
        void BindPipeline(IPipeline pipeline);
        void BindVertexBuffers(ReadOnlySpan<VertexBufferDesc> descriptions, uint baseLocation);
        void BindIndexBuffer(IBuffer buffer, ulong offset, IndexType indexType);
        void SetViewport(uint firstViewport, ReadOnlySpan<Viewport> viewports);
        void SetScissor(uint firstScissor, ReadOnlySpan<Rect2D> scissors);
        void SetLineWidth(float lineWidth);
        void SetDepthBias(float depthBiasConstantFactor, float depthBiasClamp, float depthBiasSlopeFactor);
        void SetBlendConstants(float redBlendContstant, float greenBlendContstant, float blueBlendContstant, float alphaBlendContstant);
        void SetDepthBounds(float minDepthBounds, float maxDepthBounds);
        void SetStencilOperationValues(StencilFaceFlags faceMask, uint compareMask, uint writeMask, uint reference);

        //PipelineBarrier
        void SetMemoryBarrier(in MemoryBarrier barrier);

        //Graphics
        void Draw(uint vertexCount, uint instanceCount, uint firstVertex, uint firstInstance);
        void DrawIndexed(uint indexCount, uint instanceCount, uint firstIndex, int vertexOffset, uint firstInstance);
        void DrawIndirect(IBuffer buffer, ulong offset, uint drawCount, uint stride);
        void DrawIndexedIndirect(IBuffer buffer, ulong offset, uint drawCount, uint stride);

        //Compute
        void Dispatch(uint groupCountX, uint groupCountY, uint groupCountZ);
        void DispatchIndirect(IBuffer buffer, ulong offset);

        //Transfer
        void CopyBuffer(IBuffer source, IBuffer destination, ReadOnlySpan<BufferCopy> regions);
        void CopyBufferToImage(IBuffer source, IImage destination, ReadOnlySpan<BufferImageCopy> regions);
        void CopyImageToBuffer(IImage source, IBuffer destination, ReadOnlySpan<BufferImageCopy> regions);
        void ClearColorImage(IImage image, in ClearColorValue color, ReadOnlySpan<ImageSubresourceRange> ranges);
        void ClearDepthStencilImage(IImage image, in ClearDepthStencilValue depthStencil, ReadOnlySpan<ImageSubresourceRange> ranges);
        void FillBuffer(IBuffer buffer, ulong offset, ulong size, uint dataWord);

        //Descriptors
        void BindSampledImages(ReadOnlySpan<SampledImageDesc> descriptions, uint baseLocation);
        void BindStorageImages(ReadOnlySpan<IImageView> descriptions, uint baseLocation);
        void BindUniformBuffers(ReadOnlySpan<BufferDesc> descriptions, uint baseLocation);
        void BindStorageBuffers(ReadOnlySpan<BufferDesc> descriptions, uint baseLocation);

        //PushConstants
        void SetPushConstants(in PushConstantDesc desc);
    }
}
