using System;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using Discussions.DbModel;
using Discussions.rt;
using Discussions.stats;

namespace Discussions.ctx
{
    public class PrivateCenterCtx
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
            int nAttempts = 0;
            bool saveFailed;
            ArgPoint conflictedArgPoint = null;
            do   
            {        
                saveFailed = false;
                try
                {
                    Get().SaveChanges();
                }
                catch (System.Data.Entity.Core.OptimisticConcurrencyException ex)        
                {           
                    saveFailed = true;            

                    Comment[] addedConflictedComments =
                        ex.StateEntries.Where(se => se.State == EntityState.Modified && 
                                                    (se.Entity as DbModel.Comment)!=null)
                            .Select(se => se.Entity)
                            .Cast<Comment>()
                            .ToArray();

                    if (ex.StateEntries.Any() && addedConflictedComments.Length == 0)
                    {
                        //if placeholder comment is in the conflict
                        Get().Refresh(RefreshMode.StoreWins, 
                                     ex.StateEntries.Where(se => se.Entity != null).Select(se => se.Entity));                                               
                    }

                    //detach conflicted comments to prevent their modification by the Refresh() call
                    foreach (var addedConflictedComment in addedConflictedComments)
                    {
                        conflictedArgPoint = addedConflictedComment.ArgPoint;
                        Get().Detach(addedConflictedComment);
                    }

                    if (conflictedArgPoint != null)
                    {
                        Get().Refresh(RefreshMode.StoreWins, conflictedArgPoint);

                        foreach (var comment in addedConflictedComments)
                        {
                            Get().AddToComment(comment);
                        }
                    }
                    if (conflictedArgPoint != null)
                        DaoUtils.EnsureCommentPlaceholderExists(conflictedArgPoint);
                }
            } while (saveFailed && ++nAttempts < 6);
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