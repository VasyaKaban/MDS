namespace Engine.Rendering
{
    public enum ImageViewType
    {
        ImageView1D,
        ImageView2D,
        ImageView3D,
        CubeMap,
        ImageView1DArray,
        ImageView2DArray,
        CubeMapArray
    }

    public enum ComponentSwizzle
    {
        Identity,
        Zero,
        One,
        Red,
        Green,
        Blue,
        Alpha
    }

    public struct ComponentMapping
    {
        public ComponentSwizzle red;
        public ComponentSwizzle green;
        public ComponentSwizzle blue;
        public ComponentSwizzle alpha;
    }

    public struct ImageSubresourceRange
    {
        public uint baseMipLevel;
        public uint levelCount;
        public uint baseArrayLayer;
        public uint layerCount;
    }

    public struct ImageViewCreateInfo
    {
        public IImage image;
        public ImageViewType viewType;
        public ComponentMapping components;
        public ImageSubresourceRange subresourceRange;
    }


    public interface IImageView : IDisposable
    {
        IRenderingContext GetContext();
        IImage GetImage();
        void GetCreateInfo(out ImageViewCreateInfo info);
    }
}
