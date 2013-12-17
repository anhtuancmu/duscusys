using System.Threading.Tasks;
using DistributedEditor;

namespace Discussions.bots
{
    public static class BotUtils
    {
        public static async Task LaserMovementAsync(LaserPointerWndCtx ctx)
        {

            for (int i = 0; i < 100; i += 8)
            {
                ctx.BotHandleMove(new System.Windows.Point(700 + i, 100 + 3 * i));
                await Utils.DelayAsync(20);
            }
            for (int i = 0; i < 100; i += 8)
            {
                ctx.BotHandleMove(new System.Windows.Point(800 - i, 400 - 3 * i));
                await Utils.DelayAsync(20);
            }
        }
    }
}