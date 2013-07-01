﻿using System.Timers;
using System.Web;
using Discussions;
using Discussions.DbModel;

namespace DiscSvc.Reporting
{
    public class ReportingHandler : IHttpHandler
    {
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

        public async void ProcessRequest(HttpContext context)
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
            var ctx = new DiscCtx(ConfigManager.ConnStr);
            var reportParams = queryParams.Materialize(ctx);
            if (reportParams == null)
            {
                context.Response.StatusCode = 400;
                return;
            }

            var screenshotClient = new ReportingPhotonClient();
            var reportingTasks = screenshotClient.StartReportingActivities(reportParams.Topic,
                                                                           reportParams.Discussion,
                                                                           reportParams.Session);
            var complexReportTask = reportingTasks.Item2;

            //compute and set report parameters 
            var report = new Report
                {
                    QueryParams = queryParams,
                    ReportParams = reportParams,
                    Participants = Helpers.ParticipantsTuples(reportParams.Topic, reportParams.Session),
                    ComplexReport  = await complexReportTask,
                    ReportUrl = context.Request.Url.ToString()
                };

            var screenshotTask = reportingTasks.Item1;
            report.ReceiveScreenshots(await screenshotTask, context);

            var cleanupTimer = new Timer(10*60*1000); //10 minutes
            cleanupTimer.Elapsed += (sender, args) =>
                {
                    cleanupTimer.Dispose();
                    report.Dispose();
                    screenshotClient.Dispose();
                };   
           cleanupTimer.Start();

           context.Response.Write(report.TransformText());
        }

        #endregion
    }
}
