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

    public readonly record struct ComponentMapping(
        ComponentSwizzle Red,
        ComponentSwizzle Green,
        ComponentSwizzle Blue,
        ComponentSwizzle Alpha
    );

    public readonly record struct ImageSubresourceRange(
        uint BaseMipLevel,
        uint LevelCount,
        uint BaseArrayLayer,
        uint LayerCount
    );

    public readonly record struct ImageViewCreateInfo(
        IImage Image,
        ImageViewType ViewType,
        ComponentMapping Components,
        ImageSubresourceRange SubresourceRange
    );


    public interface IImageView : IDisposable
    {
        IRenderingContext GetContext();
        IImage GetImage();
        ImageViewCreateInfo GetCreateInfo();
    }
}
