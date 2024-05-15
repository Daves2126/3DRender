namespace _3DRender.Direct3D;

using Silk.NET.Core.Native;
using Silk.NET.Direct3D11;

public class BufferSet
{
    public ComPtr<ID3D11Buffer> VertexBuffer { get; }
    public ComPtr<ID3D11Buffer> IndexBuffer { get; }
    public uint IndexCount { get; }

    public BufferSet(ComPtr<ID3D11Buffer> vertexBuffer, ComPtr<ID3D11Buffer> indexBuffer, uint indexCount)
    {
        VertexBuffer = vertexBuffer;
        IndexBuffer = indexBuffer;
        IndexCount = indexCount;
    }
}
