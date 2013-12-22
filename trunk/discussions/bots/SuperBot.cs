﻿using System;
using System.Threading.Tasks;
using Discussions.pdf_reader;
using Discussions.view;

namespace Discussions.bots
{
    public class SuperBot
    {
        private readonly PublicCenter _publicCenter;
        private readonly Random _rnd;
        private bool _enabled;

        public SuperBot(PublicCenter center)
        {
            _publicCenter = center;
            _rnd = new Random();
            _enabled = true;

            ExplanationModeMediator.Inst.ExplanationModeEnabled = true;

            RunAsync().GetAwaiter().OnCompleted(() => { });
        }

        async Task RunAsync()
        {
            while (_enabled)
            {
                LargeBadgeView lbv = OpenRandomBadge();
                if (lbv != null)
                {
                    await Utils.DelayAsync(1000);

                    if(_rnd.Next(10)<5)
                        await WorkWithAttachmentAsync(lbv, _rnd);
                    else
                        await OpenRandomSourceAsync(lbv);

                    await Utils.DelayAsync(300 + _rnd.Next(500));

                    lbv.Close();
                }

                await Utils.DelayAsync(_rnd.Next(1000));
            }
        }

        static async Task WorkWithAttachmentAsync(LargeBadgeView lbv, Random rnd)
        {
            Tuple<WebkitBrowserWindow, ImageWindow, ReaderWindow2> result =
                await lbv.BotLaunchRandomAttachmentAsync(rnd);

            WebkitBrowserWindow browserWindow = result.Item1;
            if (browserWindow != null)
            {
                await Utils.DelayAsync(500);
                browserWindow.Deinit();
                return;
            }

            ImageWindow imgWindow = result.Item2;
            if (imgWindow != null)
            {
                await Utils.DelayAsync(300);

                ExplanationModeMediator.Inst.LasersEnabled = true;

                await imgWindow.BotManipulationsAsync();

                await Utils.DelayAsync(400);

                ExplanationModeMediator.Inst.LasersEnabled = false;
                await Utils.DelayAsync(1000);

                imgWindow.Deinit();
                return;
            }

            ReaderWindow2 pdf = result.Item3;
            if (pdf != null)
            {
                await Utils.DelayAsync(1000);

                await pdf.BotScrollAsync(rnd);

                await Utils.DelayAsync(600);

                await pdf.BotLaserActivityAsync();
                
                await Utils.DelayAsync(400);

                pdf.Deinit();
                return;
            }
        }

        async Task OpenRandomSourceAsync(LargeBadgeView lbv)
        {
            var browser = await lbv.BotLaunchRandomSource(_rnd);
            if (browser == null)
                return;

            await Utils.DelayAsync(1000);

            await browser.BotScrollRandomAsync(_rnd);

            await Utils.DelayAsync(300);

            await browser.BotLaserActivityAsync();

            await Utils.DelayAsync(1000 + _rnd.Next(1800));

            browser.Close();
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