//#define PROFILE
#define PROFILE
using System.Diagnostics;
namespace WFBot.Features.ImageRendering
{
    public class ImageRenderProfiler : IDisposable 
    {
        Stopwatch sw;
        Stopwatch swBase;
        bool fake;

        public ImageRenderProfiler(bool fake = false)
        {
            this.fake = fake;
            if (fake) return;
            
            sw = Stopwatch.StartNew();
            swBase = Stopwatch.StartNew();
        }

        public void Segment(string s)
        {
            if (fake) return;
#if PROFILE
            Console.WriteLine($"Profiler: {s} {sw.Elapsed.TotalMilliseconds:N0}ms");
            sw.Restart();
#endif
        }

        public void Dispose()
        {
            if (fake) return;
#if PROFILE
            Console.WriteLine($"Profiler: 完成渲染 {swBase.Elapsed.TotalSeconds:F2}s");
#endif
        }
    }
}
