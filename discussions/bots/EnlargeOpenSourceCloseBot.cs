using System;
using System.Threading.Tasks;
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
                    await OpenRandomSourceAsync(lbv);
                   
                    await Utils.DelayAsync(100+_rnd.Next(200));
                   
                    lbv.Close();
                }
               
                await Utils.DelayAsync(200+_rnd.Next(200));
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

        async Task OpenRandomSourceAsync(LargeBadgeView lbv)
        {
            var browser = await lbv.BotLaunchRandomSource(_rnd);
            if (browser == null)
                return;

            await Utils.DelayAsync(_rnd.Next(1000));

            await browser.BotScrollRandomAsync(_rnd);

            await Utils.DelayAsync(1000 + _rnd.Next(1800));

            browser.Close();
        }

        public void Dispose()
        {
            _enabled = false;
        }
    }
}