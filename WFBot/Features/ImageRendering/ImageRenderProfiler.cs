//#define PROFILE
#define PROFILE
using System.Diagnostics;
namespace WFBot.Features.ImageRendering
{
    public class ImageRenderProfiler : IDisposable 
    {
        Stopwatch sw;
        Stopwatch swBase;

        public ImageRenderProfiler()
        {
            sw = Stopwatch.StartNew();
            swBase = Stopwatch.StartNew();
        }

        public void Segment(string s)
        {
#if PROFILE
            Console.WriteLine($"Profiler: {s} {sw.Elapsed.TotalMilliseconds:N0}ms");
            sw.Restart();
#endif
        }

        public void Dispose()
        {
#if PROFILE
            Console.WriteLine($"Profiler: 完成渲染 {swBase.Elapsed.TotalSeconds:F2}s");
#endif
        }
    }
}
