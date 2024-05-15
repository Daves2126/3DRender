namespace _3DRender.Commons;
using Silk.NET.Maths;

public interface IGraphicsRenderer
{
    unsafe void OnLoad();
    unsafe void OnRender(double obj);
    void OnUpdate(double obj);
    void OnFramebufferResize(Vector2D<int> newSize);
    void OnClose();
    void AddFigure(Figure figure);
}