using System.Data;
using System.Data.Objects;
using Discussions.DbModel;
using Discussions.DbModel.model;
using Discussions.rt;

namespace Discussions.stats
{
    public class StatsTrackingDbCtx : DiscCtx
    {
        private UISharedRTClient sharedClient = null;

        public StatsTrackingDbCtx(string connStr, UISharedRTClient sharedClient) :
            base(connStr)
        {
            this.sharedClient = sharedClient;
        }

        public override int SaveChanges(SaveOptions options)
        {
            var si = SessionInfo.Get();
            if (sharedClient != null && si.currentTopicId != -1 && si.discussion != null)
            {
                //added
                foreach (ObjectStateEntry entry in ObjectStateManager.GetObjectStateEntries(EntityState.Added))
                {
                    if (entry.Entity is ArgPoint)
                    {
                        ((ArgPoint) entry.Entity).ChangesPending = false;
                        sharedClient.clienRt.SendStatsEvent(StEvent.BadgeCreated,
                                                            si.person.Id, si.discussion.Id, si.currentTopicId,
                                                            DeviceType.Wpf);
                    }
                }

                //edited
                foreach (ObjectStateEntry entry in ObjectStateManager.GetObjectStateEntries(EntityState.Modified))
                {
                    if (entry.Entity is ArgPoint)
                    {
                        ((ArgPoint) entry.Entity).ChangesPending = false;
                        sharedClient.clienRt.SendStatsEvent(StEvent.BadgeEdited,
                                                            SessionInfo.Get().person.Id,
                                                            si.discussion.Id,
                                                            si.currentTopicId,
                                                            DeviceType.Wpf);
                    }
                }

                //sources/comments/media modified
                foreach (ObjectStateEntry entry in ObjectStateManager.GetObjectStateEntries(EntityState.Unchanged))
                {
                    if (entry.Entity is ArgPoint)
                    {
                        var ap = (ArgPoint) entry.Entity;
                        if (ap.ChangesPending)
                        {
                            ap.ChangesPending = false;
                            sharedClient.clienRt.SendStatsEvent(StEvent.BadgeEdited,
                                                                si.person.Id, si.discussion.Id, si.currentTopicId,
                                                                DeviceType.Wpf);
                        }
                    }
                }
            }

            return base.SaveChanges(options);
        }
    }
}