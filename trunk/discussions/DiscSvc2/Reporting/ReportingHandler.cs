using System.Timers;
using System.Web;
using Discussions;
using Discussions.DbModel;

namespace DiscSvc.Reporting
{
    public class ReportingHandler : IHttpHandler
    {
        ReportingPhotonClient _screenshotClient;
        Report _report;
        Timer _cleanupTimer;
        DiscCtx _ctx;

        /// <summary>
        /// You will need to configure this handler in the Web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            //validate query parameters
            QueryParams queryParams;
            try
            {
                queryParams = new QueryParams(context.Request);
            }
            catch
            {
                context.Response.StatusCode = 400; //bad request
                return;
            }

            //convert query parameters to entities            
            _ctx = new DiscCtx(ConfigManager.ConnStr);
            var reportParams = queryParams.Materialize(_ctx);
            if (reportParams == null)
            {
                context.Response.StatusCode = 400;
                return;
            }

            _screenshotClient = new ReportingPhotonClient();
            var reportingTasks = _screenshotClient.StartReportingActivities(
                reportParams.Topic,
                reportParams.Discussion,
                reportParams.Session,
                _ctx);

            //blocking wait
            //var r1 = reportingTasks.ScreenshotsTask.Result;  
            //var r2 = reportingTasks.ReportTask.Result;  

            //compute and set report parameters 
            _report = new Report
            {
                QueryParams = queryParams,
                ReportParams = reportParams,
                Participants = Helpers.ParticipantsTuples(reportParams.Topic, reportParams.Session),
                ComplexReport = reportingTasks.ReportTask.Result,
                ReportUrl = context.Request.Url.ToString(),
                BaseUrl = Helpers.BaseUrl(context.Request)
            };

            _report.ReceiveScreenshots(reportingTasks.ScreenshotsTask.Result, context);

            _cleanupTimer = new Timer(5 * 60 * 1000); //5 minutes
            _cleanupTimer.Elapsed += CleanupTimerOnElapsed;
            _cleanupTimer.Start();

            context.Response.Write(_report.TransformText());
        }
        #endregion
        private void CleanupTimerOnElapsed(object sender, ElapsedEventArgs elapsedEventArgs)
        {
            _cleanupTimer.Elapsed -= CleanupTimerOnElapsed;
            if (_report != null)
            {
                _report.Dispose();
                _report = null;
            }
            if (_screenshotClient != null)
            {
                _screenshotClient.Dispose();
                _screenshotClient = null;
            }
            if (_ctx != null)
            {
                _ctx.Dispose();
                _ctx = null;
            }

            _cleanupTimer.Dispose();
            _cleanupTimer = null;
        }

      
    }
}
