namespace Engine.Rendering
{
    public readonly record struct ShaderCreateInfo(ReadOnlyMemory<byte> Code);

    public interface IShader : IDisposable
    {
        IRenderingContext GetContext();
    }
}
