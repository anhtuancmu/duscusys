using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Discussions;
using Discussions.DbModel;
using Discussions.DbModel.model;
using DiscussionsClientRT;
using System.Linq;
using Reporter;

namespace DiscSvc.Reporting
{
    public struct ReportingActivitiesTasks
    {
        public Task<Dictionary<int, byte[]>> ScreenshotsTask;
        public Task<ReportCollector> ReportTask;
    }

    public class ReportingPhotonClient : IDisposable
    {
        private Topic _topic;
        private Discussion _disc;
        private Session _session;
        private TaskCompletionSource<Dictionary<int, byte[]>> _remoteScreenshotTCS;
        private TaskCompletionSource<ReportCollector> _hardReportTCS;
        
        private ClientRT _clienRt;

        private bool _servicingPhotonClient = true;

        public ReportingActivitiesTasks StartReportingActivities(Topic topic, Discussion disc, 
                                                                 Session session, DiscCtx ctx)
        {
            _topic = topic;
            _disc = disc;
            _session = session;

            var moder = ctx.Person.Single(p => p.Name.StartsWith("moder"));
            _clienRt = new ClientRT(disc.Id,
                                     ConfigManager.ServiceServer,                 
                                     moder.Name,
                                     moder.Id,
                                     DeviceType.Wpf);
            
            _clienRt.onJoin += OnJoined;

            _hardReportTCS = new TaskCompletionSource<ReportCollector>();
            _remoteScreenshotTCS = new TaskCompletionSource<Dictionary<int, byte[]>>();

            Task.Factory.StartNew(async () =>
                {
                    while (_servicingPhotonClient)
                    {
                        _clienRt.Service();
                        await Utils.Delay(40);                        
                    }
                });

            return new ReportingActivitiesTasks
            {
                ReportTask = _hardReportTCS.Task,
                ScreenshotsTask = _remoteScreenshotTCS.Task
            };
        }
        
        private void OnJoined()
        {
            _clienRt.onJoin -= OnJoined;

            _clienRt.onScreenshotResponse += onScreenshotResponse;
            _clienRt.SendScreenshotRequest(_topic.Id, _disc.Id);

            var reportParameters = new Reporter.ReportParameters(
                                _session.Person.Select(p => p.Id).Distinct().ToList(),
                                _session, _topic, _disc);
            new ReportCollector(null, onReportGenerated, reportParameters, null, _clienRt);
        }

        private void onScreenshotResponse(Dictionary<int, byte[]> resp)
        {
            _clienRt.onScreenshotResponse -= onScreenshotResponse;

            _remoteScreenshotTCS.SetResult(resp);
            _remoteScreenshotTCS = null;
        }

        public void onReportGenerated(ReportCollector sender, object args)
        {
            _hardReportTCS.SetResult(sender);
            _hardReportTCS = null;
        }

        public void Dispose()
        {
            _servicingPhotonClient = false;       

            if (_clienRt != null)
            {
                _clienRt.SendLiveRequest();
                _clienRt.Stop();
                _clienRt = null;
            }
        }        
    }
}