using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions;
using Discussions.DbModel;
using Discussions.rt;
using Discussions.stats;

namespace Discussions
{
    public class CtxSingleton
    {
        static StatsTrackingDbCtx ctx = null;

        public static UISharedRTClient sharedClient = null;

        public static StatsTrackingDbCtx Get()
        {
            if(ctx==null)
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
            if(ctx!=null)
            {
                ctx = null;
                SessionInfo.UpdateToNewCtx();
            }
        }
    }
}
