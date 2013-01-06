using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.DbModel;
using Discussions.rt;
using Discussions.stats;

namespace Discussions
{
    //requirement to keep private and public boards opened breaks assumption that only single
    //form owns data context in the same time. this creates conflicts between forms. 
    //this second singleton is used for private board to resolve conflicts
    public class Ctx2
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