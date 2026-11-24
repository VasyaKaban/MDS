using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Engine.Rendering
{
    public enum PipelineType
    {
        Graphics,
        Compute
    }

    public enum VertexInputRate
    {
        Vertex,
        Instance
    }

    public struct VertexInputBindingDescription
    {
        public uint binding;
        public uint stride;
        public VertexInputRate inputRate;
    }

    public struct VertexInputAttributeDescription
    {
        public uint location;
        public uint binding;
        public Format format;
        public uint offset;
    }

    public struct PipelineVertexStateCreateInfo
    {
        public ReadOnlyMemory<VertexInputBindingDescription> vertexBindingDescriptions;
        public ReadOnlyMemory<VertexInputAttributeDescription> vertexAttributeDescriptions;
    }

    public enum PrimitiveTopology
    {
        PointList,
        LineList,
        LineStrip,
        TriangleList,
        TriangleStrip,
        TriangleFan,
        LineListWithAdjacency,
        LineStripWithAdjacency,
        TriangleListWithAdjacency,
        TriangleStripWithAdjacency,
        PatchList
    }

    public struct PipelineInputAssemblyStateCreateInfo
    {
        public PrimitiveTopology topology;
        public bool stripTopologyPrimitiveRestartEnable;
    }

    public struct PipelineTessellationStateCreateInfo
    {
        public uint patchControlPoints;
    }

    public struct Viewport
    {
        public float x;
        public float y;
        public float width;
        public float height;
        public float minDepth;
        public float maxDepth;
    }

    public struct Rect2D
    {
        public Offset2D offset;
        public Extent2D extent;
    }

    public struct PipelineViewportStateCreateInfo
    {
        public uint viewportCount;
        public ReadOnlyMemory<Viewport> viewports;
        public uint scissorCount;
        public ReadOnlyMemory<Rect2D> scissors;
    }

    public enum PolygonMode
    {
        Fill,
        Line,
        Point
    }

    [Flags]
    public enum CullModeFlags
    {
        None = 0,
        Front = 1 << 0,
        Back = 1 << 1
    }

    public enum FrontFace
    {
        CounterClockwise,
        Clockwise
    }

    public struct PipelineRasterizationStateCreateInfo
    {
        public bool depthClampEnable;
        public bool rasterizerDiscardEnable;
        public PolygonMode polygonMode;
        public CullModeFlags cullMode;
        public FrontFace frontFace;
        public bool depthBiasEnable;
        public float depthBiasConstantFactor;
        public float depthBiasClamp;
        public float depthBiasSlopeFactor;
        public float lineWidth;
        public ReadOnlyMemory<Format> colorAttachmentFormats;
        public Format? depthStencilAttachmentFormat;
    }

    public struct PipelineMultisampleStateCreateInfo
    {
        public SampleCountFlags rasterizationSamples;
        public bool sampleShadingEnable;
        public float minSampleShading;
        public ReadOnlyMemory<uint> sampleMask;
        public bool alphaToCoverageEnable;
        public bool alphaToOneEnable;
    }

    public enum StencilOperation
    {
        Keep,
        Zero,
        Replace,
        IncrementAndClamp,
        DecrementAndClamp,
        Invert,
        IncrementAndWrap,
        DecrementAndWrap
    }

    public struct StencilOperationState
    {
        public StencilOperation failOperation;
        public StencilOperation passOperation;
        public StencilOperation depthFailOperation;
        public CompareOperation compareOperation;
        public uint compareMask;
        public uint writeMask;
        public uint reference;
    }

    public enum LogicOperation
    {
        Clear,
        And,
        AndReverse,
        Copy,
        AndInverted,
        NoOperation,
        Xor,
        Or,
        NotOr,
        Equivalent,
        Invert,
        OrReverse,
        CopyInverted,
        OpInverted,
        NotAnd,
        Set
    }

    public enum BlendFactor
    {
        Zero,
        One,
        SourceColor,
        OneMinusSourceColor,
        DestinationColor,
        OneMinusDestinationColor,
        SourceAlpha,
        OneMinusSourceAlpha,
        DestinationAlpha,
        OneMinusDestinationAlpha,
        ConstantColor,
        OneMinusConstantColor,
        ConstantAlpha,
        OneMinusConstantAlpha,
        SourceAlphaSaturage,
        Source1Color,
        OneMinusSource1Color,
        Source1Alpha,
        OneMinusSource1Alpha,
    }

    public struct PipelineDepthStencilStateCreateInfo
    {
        public bool depthTestEnable;
        public bool depthWriteEnable;
        public CompareOperation depthCompareOperation;
        public bool depthBoundsTestEnable;
        public bool stencilTestEnable;
        public StencilOperationState front;
        public StencilOperationState back;
        public float minDepthBounds;
        public float maxDepthBounds;
    }

    public enum BlendOperation
    {
        Add,
        Subtract,
        ReverseSubtract,
        Min,
        Max
    }

    [Flags]
    public enum ColorComponentFlags
    {
        None = 0,
        Red = 1 << 0,
        Green = 1 << 1,
        Blue = 1 << 2,
        Alpha = 1 << 3
    }

    public struct PipelineColorBlendAttachmentState
    {
        public bool blendEnable;
        public BlendFactor srcColorBlendFactor;
        public BlendFactor dstColorBlendFactor;
        public BlendOperation colorBlendOperation;
        public BlendFactor srcAlphaBlendFactor;
        public BlendFactor dstAlphaBlendFactor;
        public BlendOperation alphaBlendOperation;
        public ColorComponentFlags colorWriteMask;
    }

    public struct PipelineColorBlendStateCreateInfo
    {
        public bool logicOperationnable;
        public LogicOperation logicOperation;
        public ReadOnlyMemory<PipelineColorBlendAttachmentState> attachments;
        public float redBlendContstant;
        public float greenBlendContstant;
        public float blueBlendContstant;
        public float alphaBlendContstant;
    }

    public enum DynamicState
    {
        Viewport,
        Scissor,
        LineWidth,
        DepthBias,
        BlendConstants,
        DepthBounds,
        StencilOperationValues
    }

    public struct GraphicsPipelineCreateInfo
    {
        public IShader vertedShader;
        public IShader? tesselationControlShader;
        public IShader? tesselationEvaluationShader;
        public IShader? geometryShader;
        public IShader? fragmentShader;

        public PipelineVertexStateCreateInfo vertexInputState;
        public PipelineInputAssemblyStateCreateInfo inputAssemblyState;
        public PipelineTessellationStateCreateInfo? tessellationState;
        public PipelineViewportStateCreateInfo? viewportState;
        public PipelineRasterizationStateCreateInfo rasterizationState;
        public PipelineMultisampleStateCreateInfo? multisampleState;
        public PipelineDepthStencilStateCreateInfo? depthStencilState;
        public PipelineColorBlendAttachmentState? colorBlendState;
        public ReadOnlyMemory<DynamicState> dynamicStates;
    }

    public struct ComputePipelineCreateInfo
    {
        public IShader computeShader;
    }

    public interface IPipeline : IDisposable
    {
        IRenderingContext GetContext();
        PipelineType GetPipelineType();
    }
}
