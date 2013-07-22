using System.Threading.Tasks;

namespace DiscSvc.Reporting
{
    public class Utils
    {
        public static Task Delay(int ms)
        {
            var tcs = new TaskCompletionSource<object>();
            var timer = new System.Timers.Timer(ms) { AutoReset = false };
            timer.Elapsed += delegate
            {
                timer.Dispose();
                tcs.SetResult(null);
            };
            timer.Start();
            return tcs.Task;
        }
    }
}