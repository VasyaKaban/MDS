namespace Engine.Rendering
{
    public struct ShaderCreateInfo
    {
        public ReadOnlyMemory<byte> code;
    }

    public interface IShader : IDisposable
    {
        IRenderingContext GetContext();
    }
}
