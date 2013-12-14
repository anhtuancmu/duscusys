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
                   
                    await Utils.Delay(_rnd.Next(200));
                   
                    lbv.Close();
                }
               
                await Utils.Delay(_rnd.Next(200));
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
            var browser = await lbv.LaunchRandomSource(_rnd);
            if (browser == null)
                return;

            await Utils.Delay(_rnd.Next(100));

            await browser.BotScrollDownAsync();

            await Utils.Delay(100 + _rnd.Next(800));

            browser.Close();
        }

        public void Dispose()
        {
            _enabled = false;
        }
    }
}