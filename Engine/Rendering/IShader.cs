namespace Engine.Rendering
{
    public enum ShaderStage
    {
        Vertex,
        TessellationControl,
        TessellationEvaluation,
        Geometry,
        Fragment,
        Compute
    }

    public struct ShaderCreateInfo
    {
        ShaderStage stage;
        ReadOnlyMemory<byte> code;
    }

    public interface IShader : IDisposable
    {
        IRenderingContext GetContext();
    }
}
