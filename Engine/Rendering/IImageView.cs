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
        ComponentSwizzle red;
        ComponentSwizzle green;
        ComponentSwizzle blue;
        ComponentSwizzle alpha;
    }

    public struct ImageSubresourceRange
    {
        uint baseMipLevel;
        uint levelCount;
        uint baseArrayLayer;
        uint layerCount;
    }

    public struct ImageViewCreateInfo
    {
        IImage image;
        ImageViewType viewType;
        ComponentMapping components;
        ImageSubresourceRange subresourceRange;
    }


    public interface IImageView : IDisposable
    {
        IRenderingContext GetContext();
        IImage GetImage();
        void GetCreateInfo(out ImageViewCreateInfo info);
    }
}
