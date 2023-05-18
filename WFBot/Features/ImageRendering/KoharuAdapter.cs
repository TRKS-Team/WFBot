using System.Buffers;
using Humanizer;
using SharpVk;
using SkiaSharp;
using WFBot.Koharu;
using WFBot.Utils;
using WFBot.WebUI;

namespace WFBot.Features.ImageRendering;

public class StringPainter : Painter<string>
{
    public override IDrawingCommand Draw(string data)
    {
        return SimpleImageRendering(data);
    }
}

public static class KoharuAdapter
{
    static bool UseGPU = false;

    public static byte[] SimpleStringRendering(string s)
    {
        return new StringPainter().Draw(s).BuildImage();
    }


    public static byte[] BuildImage(this IDrawingCommand command)
    {
        var profiler = new ImageRenderProfiler();

        var vulkan = context ??= CreateVulkan();

        command = command.ApplyWFBotInfoTag(AsyncContext.GetCommandIdentifier());
        var list = new List<DrawingContentWithPosition>();
        profiler.Segment("Make commands");

        command.AcquireAllDrawingCommands(list);
        profiler.Segment("Get all drawing commands");
        
        using var surface = UseGPU ? SKSurface.Create(vulkan, false, new SKImageInfo(command.Size.Width, command.Size.Height, SKColorType.Rgba8888))
                : SKSurface.Create(new SKImageInfo(command.Size.Width, command.Size.Height, SKColorType.Rgba8888));
        using var canvas = surface.Canvas;
        foreach (var (drawingContent, position) in list)
        {
            if (drawingContent.DrawingPriority == DrawingPriority.Background)
            {
                drawingContent.DrawCore(canvas, position);
            }
        }

        foreach (var (drawingContent, position) in list)
        {
            if (drawingContent.DrawingPriority == DrawingPriority.Foreground)
            {
                drawingContent.DrawCore(canvas, position);
            }
        }
        profiler.Segment("Draw call");
        using var skImage = surface.Snapshot();
        profiler.Segment("Snapshot");

        // const double ratio = 0.7;
        // var resizedWidth = (int)(skImage.Width * ratio);
        // var resizedHeight = (int)(skImage.Height * ratio);
        // using var surface2 = UseGPU ? SKSurface.Create(vulkan, false, new SKImageInfo(resizedWidth, resizedHeight, SKColorType.Rgba8888)) :
        //     SKSurface.Create(new SKImageInfo(resizedWidth, resizedHeight, SKColorType.Rgba8888));
        // using var paint = new SKPaint();
        // paint.IsAntialias = true;
        // paint.FilterQuality = SKFilterQuality.High;
        //     
        // surface2.Canvas.DrawImage(skImage, new SKRectI(0, 0, resizedWidth, resizedHeight),
        //     paint);
        // surface2.Canvas.Flush();
        //
        // using var newImg = surface2.Snapshot();
        profiler.Segment("Resize");
        using var gl1 = skImage.Encode(SKEncodedImageFormat.Jpeg, 100);
        var gl1Span = gl1.Span;
        var gl = ArrayPool<byte>.Shared.Rent(gl1Span.Length);
        gl1Span.CopyTo(gl);
        Console.WriteLine($"图片大小: {gl.Length.Bytes().Kilobytes:F1}KB");
        profiler.Segment("Encode");

        return gl;
    }

    static GRContext CreateVulkan()
    {
        if (!UseGPU) return null;
        try
        {
            var grVkBackendContext = new GRSharpVkBackendContext();

            Instance? _instance;
            Device? _device;

            _instance = Instance.Create(Array.Empty<string>(), Array.Empty<string>(), applicationInfo: new ApplicationInfo()
            {
            });
            grVkBackendContext.VkInstance = _instance;

            var physicalDevices = _instance.EnumeratePhysicalDevices();
            PhysicalDevice? physicalDevice = null;
            string? deviceName = null;

            Console.WriteLine($"All GPU(s):");
            for (var i = 0; i < physicalDevices.Length; ++i)
            {
                var pd = physicalDevices[i];
                var property = pd.GetProperties();
                Console.WriteLine($"GPU {i}: {property.DeviceName}");

                // 只采用VirtualCpu独立显卡和核显
                if (property.DeviceType != PhysicalDeviceType.IntegratedGpu &&
                    property.DeviceType != PhysicalDeviceType.DiscreteGpu &&
                    property.DeviceType != PhysicalDeviceType.IntegratedGpu)
                    continue;

                // 默认不使用虚拟Gpu
                if (property.DeviceType == PhysicalDeviceType.VirtualGpu &&
                    physicalDevice != null)
                    continue;
                physicalDevice = pd;
                deviceName = property.DeviceName;
            }
            Console.WriteLine($"Selected GPU: {deviceName}");
            if (physicalDevice == null)
                throw new Exception("Unable to find physical device");

            grVkBackendContext.VkPhysicalDevice = physicalDevice;

            var queueFamilyProperties = physicalDevice.GetQueueFamilyProperties();

            var families = queueFamilyProperties
                .Select((properties, index) => new { properties, index })
                .Where(pair => pair.properties.QueueFlags.HasFlag(QueueFlags.Graphics)).ToArray();

            var graphicsFamily = families
                .FirstOrDefault()?.index;

            if (graphicsFamily == null)
                throw new Exception("Unable to find graphics queue");

            var queueInfos = new[]
            {
                new DeviceQueueCreateInfo { QueueFamilyIndex = (uint)graphicsFamily.Value, QueuePriorities = new[] { 1f } }
            };


            _device = physicalDevice.CreateDevice(queueInfos, null, null);

            if (_device == null)
                throw new Exception("Failed to create device");

            grVkBackendContext.VkDevice = _device;


            var graphicsQueue = _device.GetQueue((uint)graphicsFamily.Value, 0);

            if (graphicsQueue == null)
                throw new Exception("Failed to get queue");

            grVkBackendContext.VkQueue = graphicsQueue;
            grVkBackendContext.GraphicsQueueIndex = (uint)graphicsFamily.Value;

            grVkBackendContext.GetProcedureAddress = (name, ins, dev) =>
            {
                IntPtr ptr;
                if (dev != null)
                    ptr = dev.GetProcedureAddress(name);
                else if (ins != null)
                    ptr = ins.GetProcedureAddress(name);
                else
                    ptr = _instance.GetProcedureAddress(name);

                if (ptr == IntPtr.Zero)
                    Console.WriteLine($"{name} not found");
                return ptr;
            };

            var grContext = GRContext.CreateVulkan(grVkBackendContext, null);
            return grContext;
        }
        catch (Exception e)
        {
            return null;
        }
    }
    static GRContext context;

}