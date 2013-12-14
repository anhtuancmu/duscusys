using System;
using System.Linq;
using System.Threading.Tasks;
using Discussions.DbModel;
using Discussions.rt;
using Discussions.RTModel.Model;
using Discussions.view;

namespace Discussions.bots
{
    public class EnlargeOpenSourceCloseBot : IDisposable
    {
        private readonly PublicCenter _publicCenter;
        private readonly Random _rnd;
        private bool _enabled;

        public EnlargeOpenSourceCloseBot(PublicCenter center)
        {
            _publicCenter = center;
            _rnd = new Random();
            _enabled = true;

            RunAsync().GetAwaiter().OnCompleted(()=>{});
        }

        async Task RunAsync()
        {
            while (_enabled)
            {
                LargeBadgeView lbv = OpenRandomBadge();
                if (lbv != null)
                {
                    await OpenSourceAsync((ArgPoint)lbv.DataContext, _rnd);

                    await Utils.Delay(_rnd.Next(1000));
                   
                    lbv.Close();
                }
               
                await Utils.Delay(_rnd.Next(1000));
            }
        }

        static async Task OpenSourceAsync(ArgPoint ap, Random rnd)
        {
            if (!ap.Attachment.Any())
                return;

            var src = ap.Description.Source.FirstOrDefault();
            if (src != null)
            {
                UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(
                    SyncMsgType.SourceView, src.Id, true);

                await Utils.Delay(rnd.Next(1000));

                UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(
                    SyncMsgType.SourceView, src.Id, false);
            }
        }

        LargeBadgeView OpenRandomBadge()
        {
            var badgeToOpen = _publicCenter.GetRandomBadge();
            if (badgeToOpen == null)
                return null;

            badgeToOpen.badgeDoubleTap(null, null);

            return _publicCenter.GetLargeView();
        }

        public void Dispose()
        {
            _enabled = false;
        }
    }
}