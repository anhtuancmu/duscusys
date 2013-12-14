using System;
using System.Linq;
using System.Threading.Tasks;
using Discussions.DbModel;
using Discussions.DbModel.model;
using Discussions.rt;
using Discussions.RTModel.Model;
using Discussions.view;

namespace Discussions.bots
{
    public class EnlargeOpenAttachmentCloseBot : IDisposable
    {
        private readonly PublicCenter _publicCenter;
        private readonly Random _rnd;
        private bool _enabled;

        public EnlargeOpenAttachmentCloseBot(PublicCenter center)
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
                    await OpenAttachmentAsync((ArgPoint)lbv.DataContext, _rnd);

                    await Utils.Delay(_rnd.Next(1000));
                   
                    lbv.Close();
                }
               
                await Utils.Delay(_rnd.Next(1000));
            }
        }

        public static async Task OpenAttachmentAsync(ArgPoint ap, Random rnd)
        {
            if (!ap.Attachment.Any())
                return;

            var attachment = ap.Attachment.ElementAt(rnd.Next(ap.Attachment.Count));
            SyncMsgType syncMsgType;
            switch ((AttachmentFormat)attachment.Format)
            {
                case AttachmentFormat.None:
                    throw new NotImplementedException();
                    break;
                case AttachmentFormat.Jpg:
                    syncMsgType = SyncMsgType.ImageView;
                    break;
                case AttachmentFormat.Png:
                    syncMsgType = SyncMsgType.ImageView;
                    break;
                case AttachmentFormat.Bmp:
                    syncMsgType = SyncMsgType.ImageView;
                    break;
                case AttachmentFormat.Pdf:
                    syncMsgType = SyncMsgType.PdfView;
                    break;
                case AttachmentFormat.Youtube:
                    syncMsgType = SyncMsgType.YoutubeView;
                    break;
                case AttachmentFormat.GeneralWebLink:
                    syncMsgType = SyncMsgType.SourceView;
                    break;
                case AttachmentFormat.PngScreenshot:
                    syncMsgType = SyncMsgType.ImageView;
                    break;
                case AttachmentFormat.WordDocSet:
                    throw new NotImplementedException();
                    break;
                case AttachmentFormat.ExcelDocSet:
                    throw new NotImplementedException();
                    break;
                case AttachmentFormat.PowerPointDocSet:
                    throw new NotImplementedException();
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }


            UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(
                syncMsgType, attachment.Id, true);

            await Utils.Delay(rnd.Next(1000));

            UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(
                syncMsgType, attachment.Id, false);
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