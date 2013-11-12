using System;
using Discussions.rt;
using Discussions.stats;

namespace Discussions.ctx
{
    public class PublicBoardCtx
    {
        private static StatsTrackingDbCtx ctx = null;

        public static UISharedRTClient sharedClient = null;

        public static StatsTrackingDbCtx Get()
        {
            if (ctx == null)
            {
                if (sharedClient == null)
                    throw new Exception();

                ctx = new StatsTrackingDbCtx(Discussions.ConfigManager.ConnStr, sharedClient);
            }

            return ctx;
        }

        public static void SaveChangesIgnoreConflicts()
        {
            try
            {
                Get().SaveChanges();
            }
            catch (Exception)
            {
            }
        }

        public static void DropContext()
        {
            if (ctx != null)
            {
                ctx = null;
                SessionInfo.UpdateToNewCtx();
            }
        }
    }
}