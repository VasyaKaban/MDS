namespace Engine.Rendering
{
    [Flags]
    public enum BufferUsage
    {
        None = 0,
        TransferSource = 1 << 0,
        TransferDestination = 1 << 1,
        UniformBuffer = 1 << 2,
        StorageBuffer = 1 << 3,
        IndexBuffer = 1 << 4,
        VertexBuffer = 1 << 5,
        IndirectBuffer = 1 << 6
    }

    [Flags]
    public enum BufferCPUMapAccess
    {
        None = 0,
        MapRead = 1 << 0,
        MapWrite = 1 << 1,
        MapCoherent = 1 << 2
    }

    /*
     Device local  -> BufferCPUMapAccessFlags.None
     host visible  -> !BufferCPUMapAccessFlags.None
     host coherent -> BufferCPUMapAccessFlags.MapCoherent
     host cached   -> BufferCPUMapAccessFlags.MapRead
     */

    public readonly record struct BufferCreateInfo(
         ulong Size,
         BufferUsage Usage,
         BufferCPUMapAccess CpuMapAccess
    );

    public readonly record struct BufferCopy(
        ulong SourceOffset,
        uint DestinationOffset,
        uint Size
    );

    public readonly record struct BufferInitialData(
         ReadOnlyMemory<byte> Data,
         ReadOnlyMemory<BufferCopy> Regions
    );

    public interface IBuffer : IDisposable
    {
        IRenderingContext GetContext();
        BufferCreateInfo GetCreateInfo();
    }
}
