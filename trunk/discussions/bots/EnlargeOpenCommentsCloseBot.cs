using System;
using System.Threading.Tasks;
using Discussions.view;

namespace Discussions.bots
{
    public class EnlargeOpenCommentsCloseBot : IDisposable
    {
        private readonly PublicCenter _publicCenter;
        private readonly Random _rnd;
        private bool _enabled;


        public EnlargeOpenCommentsCloseBot(PublicCenter center)
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
                await Utils.DelayAsync(1000);
                if (lbv != null)
                {
                    var numCommentActivities = _rnd.Next(5) + 1;
                    for (int i = 1; i <= numCommentActivities; ++i)
                    {
                        if (!_enabled)
                            return;
                        lbv.BotGenerateCommentChange();
                        await Utils.DelayAsync(200+_rnd.Next(500));
                    }

                    await Utils.DelayAsync(200);

                    lbv.Close();
                }
               
                await Utils.DelayAsync(_rnd.Next(1000));
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