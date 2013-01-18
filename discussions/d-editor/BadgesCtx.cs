using System;
using Discussions.DbModel;

namespace Discussions
{
    public class BadgesCtx
    {
        private static DiscCtx ctx = null;

        public static DiscCtx Get()
        {
            if (ctx == null)
            {
                ctx = new DiscCtx(Discussions.ConfigManager.ConnStr);
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
            }
        }
    }
}