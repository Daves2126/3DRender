using Silk.NET.Windowing;
using Silk.NET.Maths;
using System.Numerics;
using _3DRender.Commons;
using _3DRender;
using System.Runtime.InteropServices;

OSPlatform currentOS = GetCurrentOperatingSystem();
Console.WriteLine($"Current Operating System: {currentOS}");

IWindow window;
var options = WindowOptions.Default;
options.Size = new Vector2D<int>(800, 600);
options.Title = "Bridge Pattern Example";
window = Window.Create(options);
var graphics = new GraphicsAbstraction(window);
graphics.Initialize();

var smallSquareVertices = new float[]
{
            0.5f, 0.5f, 0.0f,
            0.5f, -0.5f, 0.0f,
            -0.5f, -0.5f, 0.0f,
            -0.5f, 0.5f, 0.5f,
};
var smallSquareIndices = new uint[]
{
            0, 1, 3, 1, 2, 3
};
graphics.AddFigure(new Figure(smallSquareVertices, smallSquareIndices, new Vector3(1.0f, 0.5f, 0.2f)));

var smallSquareVertices1 = new float[]
{
            -0.75f, 0.75f, 0.0f,
            -0.75f, -0.75f, 0.0f,
            -1.0f, -0.75f, 0.0f,
            -1.0f, 0.75f, 0.0f,
};
var smallSquareIndices1 = new uint[]
{
            0, 1, 3, 1, 2, 3
};
graphics.AddFigure(new Figure(smallSquareVertices1, smallSquareIndices1, new Vector3(0.2f, 0.5f, 1.0f)));

window.Run();
window.Dispose();

static OSPlatform GetCurrentOperatingSystem()
{
    if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    {
        return OSPlatform.Windows;
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    {
        return OSPlatform.Linux;
    }
    else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
    {
        return OSPlatform.OSX;
    }
    else
    {
        return OSPlatform.Create("Other");
    }
}