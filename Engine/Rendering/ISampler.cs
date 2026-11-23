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

    public struct SamplerCreateInfo
    {
        Filter magFilter;
        Filter minFilter;
        Filter mipmapFilter;
        SamplerAddressMode addressModeU;
        SamplerAddressMode addressModeV;
        SamplerAddressMode addressModeW;
        float mipLodBias;
        bool anisotropyEnable;
        float maxAnisotropy;
        bool compareEnable;
        CompareOperation compareOperation;
        float minLod;
        float maxLod;
        StaticBorderColor borderColor;
    }

    public interface ISampler : IDisposable
    {
        IRenderingContext GetContext();
        void GetCreateInfo(out SamplerCreateInfo info);
    }
}
