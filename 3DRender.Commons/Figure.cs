namespace _3DRender.Commons;

using System.Numerics;

public class Figure(float[] vertices, uint[] indices, Vector3 color)
{
    public float[] Vertices { get; set; } = vertices;
    public uint[] Indices { get; set; } = indices;
    public Vector3 Color { get; set; } = color;
    public uint Vbo { get; set; }
    public uint Ebo { get; set; }
}
