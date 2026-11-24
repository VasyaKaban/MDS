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
        public Filter magFilter;
        public Filter minFilter;
        public Filter mipmapFilter;
        public SamplerAddressMode addressModeU;
        public SamplerAddressMode addressModeV;
        public SamplerAddressMode addressModeW;
        public float mipLodBias;
        public bool anisotropyEnable;
        public float maxAnisotropy;
        public bool compareEnable;
        public CompareOperation compareOperation;
        public float minLod;
        public float maxLod;
        public StaticBorderColor borderColor;
    }

    public interface ISampler : IDisposable
    {
        IRenderingContext GetContext();
        void GetCreateInfo(out SamplerCreateInfo info);
    }
}
