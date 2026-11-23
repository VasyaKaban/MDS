namespace Engine.Rendering
{
    [Flags]
    public enum BufferUsageFlags
    {
        None = 0,
        TransferSrc = 1 << 0,
        TransferDst = 1 << 1,
        UniformBuffer = 1 << 2,
        StorageBuffer = 1 << 3,
        IndexBuffer = 1 << 4,
        VertexBuffer = 1 << 5,
        IndirectBuffer = 1 << 6
    }

    [Flags]
    public enum BufferCPUMapAccessFlags
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

    public struct BufferCreateInfo
    {
        ulong size;
        BufferUsageFlags usage;
        BufferCPUMapAccessFlags cpuMapAccess;
    }

    public struct BufferCopy
    {
        ulong srcOffset;
        uint dstOffset;
        uint size;
    }

    public struct BufferInitialData
    {
        ReadOnlyMemory<byte> data;
        ReadOnlyMemory<BufferCopy> regions;
    }

    public interface IBuffer : IDisposable
    {
        IRenderingContext GetContext();
        void GetCreateInfo(out BufferCreateInfo info);
    }
}
