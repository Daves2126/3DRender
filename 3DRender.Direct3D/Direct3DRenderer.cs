namespace _3DRender.Direct3D;

using _3DRender.Commons;
using Silk.NET.Core.Native;
using Silk.NET.Direct3D.Compilers;
using Silk.NET.Direct3D11;
using Silk.NET.DXGI;
using Silk.NET.Input;
using Silk.NET.Maths;
using Silk.NET.Windowing;
using System.Runtime.CompilerServices;
using System.Text;

public class Direct3DRenderer : IGraphicsRenderer
{
    private readonly IWindow _window;
    DXGI dxgi = null!;
    D3D11 d3d11 = null!;
    D3DCompiler compiler = null!;
    ComPtr<IDXGIFactory2> factory = default;
    ComPtr<IDXGISwapChain1> swapchain = default;
    ComPtr<ID3D11Device> device = default;
    ComPtr<ID3D11DeviceContext> deviceContext = default;
    ComPtr<ID3D11VertexShader> vertexShader = default;
    ComPtr<ID3D11PixelShader> pixelShader = default;
    ComPtr<ID3D11InputLayout> inputLayout = default;
    private readonly List<Figure> _figures = [];
    private readonly List<BufferSet> _bufferList = [];
    private readonly string shaderSource = @"
        struct vs_in {
            float3 position_local : POS;
        };

        struct vs_out {
            float4 position_clip : SV_POSITION;
        };

        vs_out vs_main(vs_in input) {
            vs_out output = (vs_out)0;
            output.position_clip = float4(input.position_local, 1.0);
            return output;
        }

        float4 ps_main(vs_out input) : SV_TARGET {
            return float4( 1.0, 0.5, 0.2, 1.0 );
        }
    ";
    public Direct3DRenderer(IWindow window)
    {
        _window = window;
    }


    ~Direct3DRenderer()
    {
        factory.Dispose();
        swapchain.Dispose();
        device.Dispose();
        deviceContext.Dispose();
        vertexShader.Dispose();
        pixelShader.Dispose();
        inputLayout.Dispose();
        compiler.Dispose();
        d3d11.Dispose();
        dxgi.Dispose();
    }

    public unsafe void OnLoad()
    {
        const bool forceDxvk = false;
        dxgi = DXGI.GetApi(_window, forceDxvk);
        d3d11 = D3D11.GetApi(_window, forceDxvk);
        compiler = D3DCompiler.GetApi();
        var input = _window.CreateInput();
        foreach (var keyboard in input.Keyboards)
        {
            keyboard.KeyDown += OnKeyDown;
        }
        SilkMarshal.ThrowHResult
        (
            d3d11.CreateDevice
            (
                default(ComPtr<IDXGIAdapter>),
                D3DDriverType.Hardware,
                Software: default,
                (uint)CreateDeviceFlag.Debug,
                null,
                0,
                D3D11.SdkVersion,
                ref device,
                null,
                ref deviceContext
            )
        );

        // Create our swapchain.
        var swapChainDesc = new SwapChainDesc1
        {
            BufferCount = 2, // double buffered
            Format = Format.FormatB8G8R8A8Unorm,
            BufferUsage = DXGI.UsageRenderTargetOutput,
            SwapEffect = SwapEffect.FlipDiscard,
            SampleDesc = new SampleDesc(1, 0)
        };

        // Create our DXGI factory to allow us to create a swapchain. 
        factory = dxgi.CreateDXGIFactory<IDXGIFactory2>();

        // Create the swapchain.
        SilkMarshal.ThrowHResult
        (
            factory.CreateSwapChainForHwnd
            (
                device,
                _window.Native!.DXHandle!.Value,
                in swapChainDesc,
                null,
                ref Unsafe.NullRef<IDXGIOutput>(),
                ref swapchain
            )
        );

        foreach (var figure in _figures)
        {
            ComPtr<ID3D11Buffer> vertexBuffer = default;
            ComPtr<ID3D11Buffer> indexBuffer = default;

            var bufferDesc = new BufferDesc
            {
                ByteWidth = (uint)(figure.Vertices.Length * sizeof(float)),
                Usage = Usage.Default,
                BindFlags = (uint)BindFlag.VertexBuffer
            };

            fixed (float* vertexData = figure.Vertices)
            {
                var subResourceData = new SubresourceData
                {
                    PSysMem = vertexData
                };

                SilkMarshal.ThrowHResult(device.CreateBuffer(in bufferDesc, in subResourceData, ref vertexBuffer));
            }

            bufferDesc = new BufferDesc
            {
                ByteWidth = (uint)(figure.Indices.Length * sizeof(uint)),
                Usage = Usage.Default,
                BindFlags = (uint)BindFlag.IndexBuffer
            };

            fixed (uint* indexData = figure.Indices)
            {
                var subResourceData = new SubresourceData
                {
                    PSysMem = indexData
                };

                SilkMarshal.ThrowHResult(device.CreateBuffer(in bufferDesc, in subResourceData, ref indexBuffer));
            }

            var shaderBytes = Encoding.ASCII.GetBytes(shaderSource);

            ComPtr<ID3D10Blob> vertexCode = default;
            ComPtr<ID3D10Blob> vertexErrors = default;
            HResult hr = compiler.Compile
            (
                in shaderBytes[0],
                (nuint)shaderBytes.Length,
                nameof(shaderSource),
                null,
                ref Unsafe.NullRef<ID3DInclude>(),
                "vs_main",
                "vs_5_0",
                0,
                0,
                ref vertexCode,
                ref vertexErrors
            );

            if (hr.IsFailure)
            {
                if (vertexErrors.Handle is not null)
                {
                    Console.WriteLine(SilkMarshal.PtrToString((nint)vertexErrors.GetBufferPointer()));
                }

                hr.Throw();
            }

            ComPtr<ID3D10Blob> pixelCode = default;
            ComPtr<ID3D10Blob> pixelErrors = default;
            hr = compiler.Compile
            (
                in shaderBytes[0],
                (nuint)shaderBytes.Length,
                nameof(shaderSource),
                null,
                ref Unsafe.NullRef<ID3DInclude>(),
                "ps_main",
                "ps_5_0",
                0,
                0,
                ref pixelCode,
                ref pixelErrors
            );

            if (hr.IsFailure)
            {
                if (pixelErrors.Handle is not null)
                {
                    Console.WriteLine(SilkMarshal.PtrToString((nint)pixelErrors.GetBufferPointer()));
                }

                hr.Throw();
            }

            SilkMarshal.ThrowHResult
            (
                device.CreateVertexShader
                (
                    vertexCode.GetBufferPointer(),
                    vertexCode.GetBufferSize(),
                    ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                    ref vertexShader
                )
            );

            SilkMarshal.ThrowHResult
            (
                device.CreatePixelShader
                (
                    pixelCode.GetBufferPointer(),
                    pixelCode.GetBufferSize(),
                    ref Unsafe.NullRef<ID3D11ClassLinkage>(),
                    ref pixelShader
                )
            );

            fixed (byte* name = SilkMarshal.StringToMemory("POS"))
            fixed (byte* colorName = SilkMarshal.StringToMemory("COLOR"))
            {
                var inputElement = new InputElementDesc
                {
                    SemanticName = name,
                    SemanticIndex = 0,
                    Format = Format.FormatR32G32B32Float,
                    InputSlot = 0,
                    AlignedByteOffset = 0,
                    InputSlotClass = InputClassification.PerVertexData,
                    InstanceDataStepRate = 0
                };

                SilkMarshal.ThrowHResult
                (
                    device.CreateInputLayout
                    (
                        in inputElement,
                        1,
                        vertexCode.GetBufferPointer(),
                        vertexCode.GetBufferSize(),
                        ref inputLayout
                )
                );
            }

            vertexCode.Dispose();
            vertexErrors.Dispose();
            pixelCode.Dispose();
            pixelErrors.Dispose();

            // Add the created buffers to the buffer list
            _bufferList.Add(new BufferSet(vertexBuffer, indexBuffer, (uint)figure.Indices.Length));
        }
    }

    public unsafe void OnRender(double obj)
    {
        // Obtain the framebuffer for the swapchain's backbuffer.
        using var framebuffer = swapchain.GetBuffer<ID3D11Texture2D>(0);

        // Create a view over the render target.
        ComPtr<ID3D11RenderTargetView> renderTargetView = default;
        SilkMarshal.ThrowHResult(device.CreateRenderTargetView(framebuffer, null, ref renderTargetView));

        // Clear the render target to be all black ahead of rendering.
        var backgroundColour = new[] { 0.0f, 0.0f, 0.0f, 1.0f };
        deviceContext.ClearRenderTargetView(renderTargetView, ref backgroundColour[0]);

        // Update the rasterizer state with the current viewport.
        var viewport = new Viewport(0, 0, _window.FramebufferSize.X, _window.FramebufferSize.Y, 0, 1);
        deviceContext.RSSetViewports(1, in viewport);

        // Tell the output merger about our render target view.
        deviceContext.OMSetRenderTargets(1, ref renderTargetView, ref Unsafe.NullRef<ID3D11DepthStencilView>());

        // Update the input assembler to use our shader input layout.
        deviceContext.IASetPrimitiveTopology(D3DPrimitiveTopology.D3DPrimitiveTopologyTrianglelist);
        deviceContext.IASetInputLayout(inputLayout);

        // Bind our shaders.
        deviceContext.VSSetShader(vertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
        deviceContext.PSSetShader(pixelShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);

        // Iterate through the list of figures and render each one.
        var vertexStride = 3U * sizeof(float);
        var vertexOffset = 0U;

        foreach (var bufferSet in _bufferList)
        {
            var vertexBuffer = bufferSet.VertexBuffer;
            deviceContext.IASetVertexBuffers(0, 1, ref vertexBuffer, in vertexStride, in vertexOffset);
            deviceContext.IASetIndexBuffer(bufferSet.IndexBuffer, Format.FormatR32Uint, 0);

            // Bind our shaders.
            deviceContext.VSSetShader(vertexShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);
            deviceContext.PSSetShader(pixelShader, ref Unsafe.NullRef<ComPtr<ID3D11ClassInstance>>(), 0);

            // Draw the indexed primitives.
            deviceContext.DrawIndexed(bufferSet.IndexCount, 0, 0);
        }

        // Present the drawn image.
        swapchain.Present(1, 0);

        // Clean up any resources created in this method.
        renderTargetView.Dispose();
    }


    public void OnUpdate(double obj)
    {

    }

    public void OnFramebufferResize(Vector2D<int> newSize)
    {
        SilkMarshal.ThrowHResult
        (
            swapchain.ResizeBuffers(0, (uint)newSize.X, (uint)newSize.Y, Format.FormatB8G8R8A8Unorm, 0)
        );
    }

    public void OnClose()
    {

    }
    void OnKeyDown(IKeyboard keyboard, Key key, int scancode)
    {
        // Check to close the window on escape.
        if (key == Key.Escape)
        {
            _window.Close();
        }
    }

    public void AddFigure(Figure figure)
    {
        _figures.Add(figure);
    }
}
