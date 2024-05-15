namespace _3DRender.OpenGL;

using _3DRender.Commons;
using Silk.NET.Maths;
using System.Numerics;
using System;
using Silk.NET.OpenGL;
using Silk.NET.Windowing;

public class OpenGLRenderer(IWindow window) : IGraphicsRenderer
{
    private GL Gl;
    private uint Vao;
    private uint Shader;
    public List<Figure> Figures { get; } = [];
    private readonly IWindow _window = window;
    private readonly string VertexShaderSource = @"
        #version 330 core
        layout (location = 0) in vec4 vPos;
    
        void main()
        {
            gl_Position = vec4(vPos.x, vPos.y, vPos.z, 1.0);
        }
    ";

    private readonly string FragmentShaderSource = @"
        #version 330 core
        out vec4 FragColor;
        uniform vec4 ourColor;

        void main()
        {
            FragColor = ourColor;
        }
        ";

    public unsafe void OnLoad()
    {
        Gl = GL.GetApi(_window);
        Vao = Gl.GenVertexArray();
        Gl.BindVertexArray(Vao);

        uint vertexShader = Gl.CreateShader(ShaderType.VertexShader);
        Gl.ShaderSource(vertexShader, VertexShaderSource);
        Gl.CompileShader(vertexShader);
        string infoLog = Gl.GetShaderInfoLog(vertexShader);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            Console.WriteLine($"Error compilando vertex shader {infoLog}");
        }

        uint fragmentShader = Gl.CreateShader(ShaderType.FragmentShader);
        Gl.ShaderSource(fragmentShader, FragmentShaderSource);
        Gl.CompileShader(fragmentShader);
        infoLog = Gl.GetShaderInfoLog(fragmentShader);
        if (!string.IsNullOrWhiteSpace(infoLog))
        {
            Console.WriteLine($"Error compilando fragment shader {infoLog}");
        }

        Shader = Gl.CreateProgram();
        Gl.AttachShader(Shader, vertexShader);
        Gl.AttachShader(Shader, fragmentShader);
        Gl.LinkProgram(Shader);

        Gl.GetProgram(Shader, GLEnum.LinkStatus, out var status);
        if (status == 0)
        {
            Console.WriteLine($"Error de enlace de shader {Gl.GetProgramInfoLog(Shader)}");
        }

        Gl.DetachShader(Shader, vertexShader);
        Gl.DetachShader(Shader, fragmentShader);
        Gl.DeleteShader(vertexShader);
        Gl.DeleteShader(fragmentShader);

        foreach (var figure in Figures)
        {
            figure.Vbo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, figure.Vbo);
            fixed (void* v = &figure.Vertices[0])
            {
                Gl.BufferData(BufferTargetARB.ArrayBuffer, (nuint)(figure.Vertices.Length * sizeof(Vector3)), v, BufferUsageARB.StaticDraw);
            }

            figure.Ebo = Gl.GenBuffer();
            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, figure.Ebo);
            fixed (void* i = &figure.Indices[0])
            {
                Gl.BufferData(BufferTargetARB.ElementArrayBuffer, (nuint)(figure.Indices.Length * sizeof(uint)), i, BufferUsageARB.StaticDraw);
            }
        }
    }

    public unsafe void OnRender(double obj)
    {
        Gl.Clear((uint)ClearBufferMask.ColorBufferBit);
        foreach (var figure in Figures)
        {
            Gl.UseProgram(Shader);
            Gl.Uniform4(Gl.GetUniformLocation(Shader, "ourColor"), figure.Color.X, figure.Color.Y, figure.Color.Z, 1.0f);

            Gl.BindBuffer(BufferTargetARB.ArrayBuffer, figure.Vbo);
            Gl.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 3 * sizeof(float), null);
            Gl.EnableVertexAttribArray(0);

            Gl.BindBuffer(BufferTargetARB.ElementArrayBuffer, figure.Ebo);
            Gl.DrawElements(PrimitiveType.Triangles, (uint)figure.Indices.Length, DrawElementsType.UnsignedInt, null);
        }
    }

    public void OnUpdate(double obj)
    {
    }

    public void OnFramebufferResize(Vector2D<int> newSize)
    {
        Gl.Viewport(newSize);
    }

    public void OnClose()
    {
        foreach (var figure in Figures)
        {
            Gl.DeleteBuffer(figure.Vbo);
            Gl.DeleteBuffer(figure.Ebo);
        }
        Gl.DeleteVertexArray(Vao);
        Gl.DeleteProgram(Shader);
    }

    public void AddFigure(Figure figure)
    {
        Figures.Add(figure);
    }
}
