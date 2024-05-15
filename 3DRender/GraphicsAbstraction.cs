namespace _3DRender;

using _3DRender.Commons;
using _3DRender.Direct3D;
using _3DRender.OpenGL;
using Silk.NET.Windowing;
using System.Runtime.InteropServices;

public class GraphicsAbstraction
{
    private IGraphicsRenderer _renderer;
    private readonly IWindow _window;

    public GraphicsAbstraction(IWindow window)
    {
        _window = window;
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            _window.Title = string.Concat(_window.Title, " Using Direct3D Api");
            _renderer = new Direct3DRenderer(window);
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            _window.Title = string.Concat(_window.Title, " Using OpenGl Api");
            _renderer = new OpenGLRenderer(window);
        }
        else
        {
            _window.Title = string.Concat(_window.Title, " Using OpenGl Api");
            _renderer = new OpenGLRenderer(window);
        }
    }

    public void SetGraphicsApiRenderer(IGraphicsRenderer renderer)
    {
        _renderer = renderer;
    }

    public void Initialize()
    {
        _window.Load += _renderer.OnLoad;
        _window.Render += _renderer.OnRender;
        _window.Update += _renderer.OnUpdate;
        _window.FramebufferResize += _renderer.OnFramebufferResize;
        _window.Closing += _renderer.OnClose;
    }

    public void AddFigure(Figure figure)
    {
        _renderer.AddFigure(figure);
    }
}