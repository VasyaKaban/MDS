namespace Engine.Rendering
{
    public enum Filter
    {
        Nearest,
        Linear
    }

    public enum SamplerAddressMode
    {
        Repeat,
        MirroredRepeat,
        ClampToEdge,
        ClampToBorder,
        MirrorClampToEdge
    }

    public enum CompareOperation
    {
        Never,
        Less,
        Equal,
        LessOrEqual,
        Greater,
        NotEqual,
        GreaterOrEqual,
        Always
    }

    public enum StaticBorderColor
    {
        FloatTransparentBlack,
        IntTransparentBlack,
        FloatOpaqueBlack,
        IntOpaqueBlack,
        FloatOpaqueWhite,
        IntOpaqueWhite
    }

    public readonly record struct SamplerCreateInfo(
        Filter MagFilter,
        Filter MinFilter,
        Filter MipmapFilter,
        SamplerAddressMode AddressModeU,
        SamplerAddressMode AddressModeV,
        SamplerAddressMode AddressModeW,
        float MipLodBias,
        bool AnisotropyEnable,
        float MaxAnisotropy,
        bool CompareEnable,
        CompareOperation CompareOperation,
        float MinLod,
        float MaxLod,
        StaticBorderColor BorderColor
    );

    public interface ISampler : IDisposable
    {
        IRenderingContext GetContext();
        SamplerCreateInfo GetCreateInfo();
    }
}
