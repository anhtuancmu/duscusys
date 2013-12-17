using System;
using System.Linq;
using System.Threading.Tasks;
using Discussions.DbModel;
using Discussions.rt;
using Discussions.RTModel.Model;

namespace Discussions.bots
{
    public class SourceBot : IDisposable
    {
        private readonly Topic _topic;
        private readonly Random _rnd;
        private bool _enabled;

        public SourceBot()
        {
            _topic = SessionInfo.Get().discussion.Topic.First();
            _rnd = new Random();

            _enabled = true;
            RunAsync().GetAwaiter().OnCompleted(()=>{});
        }

        async Task RunAsync()
        {
            while (_enabled)
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

                await Utils.DelayAsync(_rnd.Next(1000));

                UISharedRTClient.Instance.clienRt.SendExplanationModeSyncRequest(
                    SyncMsgType.SourceView, src.Id, false);
            }
        }

        public void Dispose()
        {
            _enabled = false;
        }
    }
}
