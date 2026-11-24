namespace Engine.Rendering
{
    [Flags]
    public enum BufferUsageFlags
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
        public ulong size;
        public BufferUsageFlags usage;
        public BufferCPUMapAccessFlags cpuMapAccess;
    }

    public struct BufferCopy
    {
        public ulong srcOffset;
        public uint dstOffset;
        public uint size;
    }

    public struct BufferInitialData
    {
        public ReadOnlyMemory<byte> data;
        public ReadOnlyMemory<BufferCopy> regions;
    }

    public interface IBuffer : IDisposable
    {
        IRenderingContext GetContext();
        void GetCreateInfo(out BufferCreateInfo info);
    }
}
