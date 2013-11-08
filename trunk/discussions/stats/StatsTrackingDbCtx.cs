using System.Data.Entity;
using System.Linq;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;

namespace Discussions.stats
{
    public class StatsTrackingDbCtx : DiscCtx
    {
        private readonly UISharedRTClient _sharedClient;

        public StatsTrackingDbCtx(string connStr, UISharedRTClient sharedClient) :
            base(connStr)
        {
            _sharedClient = sharedClient;
        }

        public override int SaveChanges()
        {
            var si = SessionInfo.Get();
            if (_sharedClient != null && si.currentTopicId != -1 && si.discussion != null)
            {
                //added
                foreach (var entry in ChangeTracker.Entries<ArgPoint>().Where(e=>e.State==EntityState.Added))
                {
                    entry.Entity.ChangesPending = false;
                    _sharedClient.clienRt.SendStatsEvent(StEvent.BadgeCreated,
                                                        si.person.Id, si.discussion.Id, si.currentTopicId,
                                                        DeviceType.Wpf);
                }

                //edited
                foreach (var entry in ChangeTracker.Entries<ArgPoint>().Where(e => e.State == EntityState.Modified))
                {
                    entry.Entity.ChangesPending = false;
                    _sharedClient.clienRt.SendStatsEvent(StEvent.BadgeEdited,
                                                        SessionInfo.Get().person.Id,
                                                        si.discussion.Id,
                                                        si.currentTopicId,
                                                        DeviceType.Wpf);
                }

                //sources/comments/media modified
                foreach (var entry in ChangeTracker.Entries<ArgPoint>().Where(e => e.State == EntityState.Unchanged))
                {
                    var ap = entry.Entity;
                    if (ap.ChangesPending)
                    {
                        ap.ChangesPending = false;
                        _sharedClient.clienRt.SendStatsEvent(StEvent.BadgeEdited,
                                                            si.person.Id, si.discussion.Id, si.currentTopicId,
                                                            DeviceType.Wpf);
                    }
                }
            }

            return base.SaveChanges();
        }
    }
}