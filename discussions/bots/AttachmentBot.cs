using System;
using System.Linq;
using System.Threading.Tasks;
using Discussions.DbModel;
using Discussions.DbModel.model;
using Discussions.rt;
using Discussions.RTModel.Model;

namespace Discussions.bots
{
    public class AttachmentBot
    {
        private readonly Topic _topic;
        private readonly Random _rnd;

        public AttachmentBot()
        {
            _topic = SessionInfo.Get().discussion.Topic.First();
            _rnd = new Random();
        }

        public async Task RunAsync()
        {
            while (true)
            {
                await SendOpenExplanationAsync();
            }
        }

        private async Task SendOpenExplanationAsync()
        {
            var ap = _topic.ArgPoint.ElementAt(_rnd.Next(_topic.ArgPoint.Count));

            if (!ap.Attachment.Any())
                return;

            var attachment = ap.Attachment.ElementAt(_rnd.Next(ap.Attachment.Count));
            SyncMsgType syncMsgType;
            switch ((AttachmentFormat) attachment.Format)
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

            await Utils.Delay(_rnd.Next(1000));

            UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(
                syncMsgType, attachment.Id, false);
        }
    }
}
