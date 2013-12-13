using System;
using System.Linq;
using System.Threading.Tasks;
using Discussions.DbModel;
using Discussions.rt;
using Discussions.RTModel.Model;

namespace Discussions.bots
{
    public class SourceBot
    {
        private readonly Topic _topic;
        private readonly Random _rnd;

        public SourceBot()
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

            var src = ap.Description.Source.FirstOrDefault();
            if (src != null)
            {
                UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(
                    SyncMsgType.SourceView, src.Id, true);

                await Utils.Delay(_rnd.Next(1000));

                UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(
                    SyncMsgType.SourceView, src.Id, false);
            }
        }
    }
}
