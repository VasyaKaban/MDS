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

    public readonly record struct VertexInputBindingDescription(
        uint Binding,
        uint Stride,
        VertexInputRate InputRate
    );

    public readonly record struct VertexInputAttributeDescription(
        uint Location,
        uint Binding,
        Format Format,
        uint Offset
    );

    public readonly record struct PipelineVertexStateCreateInfo(
        ReadOnlyMemory<VertexInputBindingDescription> VertexBindingDescriptions,
        ReadOnlyMemory<VertexInputAttributeDescription> VertexAttributeDescriptions
    );

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

    public readonly record struct PipelineInputAssemblyStateCreateInfo(
        PrimitiveTopology Topology,
        bool StripTopologyPrimitiveRestartEnable
    );

    public readonly record struct PipelineTessellationStateCreateInfo(uint PatchControlPoints);

    public readonly record struct Viewport(
        float X,
        float Y,
        float Width,
        float Height,
        float MinDepth,
        float MaxDepth
    );

    public readonly record struct Rect2D(
        Offset2D Offset,
        Extent2D Extent
    );

    public readonly record struct PipelineViewportStateCreateInfo(
        uint ViewportCount,
        ReadOnlyMemory<Viewport> Viewports,
        uint ScissorCount,
        ReadOnlyMemory<Rect2D> Scissors
    );

    public enum PolygonMode
    {
        Fill,
        Line,
        Point
    }

    [Flags]
    public enum CullMode
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

    public readonly record struct PipelineRasterizationStateCreateInfo(
        bool DepthClampEnable,
        bool RasterizerDiscardEnable,
        PolygonMode PolygonMode,
        CullMode CullModeMask,
        FrontFace FrontFace,
        bool DepthBiasEnable,
        float DepthBiasConstantFactor,
        float DepthBiasClamp,
        float DepthBiasSlopeFactor,
        float LineWidth,
        ReadOnlyMemory<Format> ColorAttachmentFormats,
        Format? DepthStencilAttachmentFormat
    );

    public readonly record struct PipelineMultisampleStateCreateInfo(
        SampleCount RasterizationSamples,
        bool SampleShadingEnable,
        float MinSampleShading,
        ReadOnlyMemory<uint> SampleMask,
        bool AlphaToCoverageEnable,
        bool AlphaToOneEnable
    );

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

    public readonly record struct StencilOperationState(
        StencilOperation FailOperation,
        StencilOperation PassOperation,
        StencilOperation DepthFailOperation,
        CompareOperation CompareOperation,
        uint CompareMask,
        uint WriteMask,
        uint Reference
    );

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

    public readonly record struct PipelineDepthStencilStateCreateInfo(
        bool DepthTestEnable,
        bool DepthWriteEnable,
        CompareOperation DepthCompareOperation,
        bool DepthBoundsTestEnable,
        bool StencilTestEnable,
        StencilOperationState Front,
        StencilOperationState Back,
        float MinDepthBounds,
        float MaxDepthBounds
    );

    public enum BlendOperation
    {
        Add,
        Subtract,
        ReverseSubtract,
        Min,
        Max
    }

    [Flags]
    public enum ColorComponent
    {
        None = 0,
        Red = 1 << 0,
        Green = 1 << 1,
        Blue = 1 << 2,
        Alpha = 1 << 3
    }

    public readonly record struct PipelineColorBlendAttachmentState(
        bool BlendEnable,
        BlendFactor SourceColorBlendFactor,
        BlendFactor DestinationColorBlendFactor,
        BlendOperation ColorBlendOperation,
        BlendFactor SourceAlphaBlendFactor,
        BlendFactor DestinationAlphaBlendFactor,
        BlendOperation AlphaBlendOperation,
        ColorComponent ColorWriteMask
    );

    public readonly record struct PipelineColorBlendStateCreateInfo(
        bool LogicOperationnable,
        LogicOperation LogicOperation,
        ReadOnlyMemory<PipelineColorBlendAttachmentState> Attachments,
        float RedBlendContstant,
        float GreenBlendContstant,
        float BlueBlendContstant,
        float AlphaBlendContstant
    );

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

    public readonly record struct GraphicsPipelineCreateInfo(
        IShader VertexShader,
        IShader? TesselationControlShader,
        IShader? TesselationEvaluationShader,
        IShader? GeometryShader,
        IShader? FragmentShader,

        PipelineVertexStateCreateInfo VertexInputState,
        PipelineInputAssemblyStateCreateInfo InputAssemblyState,
        PipelineTessellationStateCreateInfo? TessellationState,
        PipelineViewportStateCreateInfo? ViewportState,
        PipelineRasterizationStateCreateInfo RasterizationState,
        PipelineMultisampleStateCreateInfo? MultisampleState,
        PipelineDepthStencilStateCreateInfo? DepthStencilState,
        PipelineColorBlendAttachmentState? ColorBlendState,
        ReadOnlyMemory<DynamicState> DynamicStates
    );

    public readonly record struct ComputePipelineCreateInfo(IShader ComputeShader);

    public interface IPipeline : IDisposable
    {
        IRenderingContext GetContext();
        PipelineType GetPipelineType();
    }
}
