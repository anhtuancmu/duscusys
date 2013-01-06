using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discussions;
using Discussions.DbModel;
using Discussions.rt;

namespace Reporter.pdf
{
    public class PdfReportDriver
    {
        public void Run(Session session, Topic topic, Discussion discussion, Person person)
        {
            //tests
            //var ctx = new DiscCtx(ConfigManager.ConnStr);
            //var discussion = ctx.Discussion.First();
            //var topic = discussion.Topic.First();
            //var session = ctx.Session.FirstOrDefault();
            //var pers = session.Person.First();

            //start hard report 
            var reportParameters = new ReportParameters(session.Person.Select(p => p.Id).ToList(),
                                                        session, topic, discussion);
            var tcs = new TaskCompletionSource<ReportCollector>();
            new ReportCollector(null, reportGenerated, reportParameters, tcs, UISharedRTClient.Instance.clienRt);

            var pdfAsm = new Reporter.pdf.PdfAssembler2(discussion, topic, person, session,
                                                        Utils.RandomFilePath(".pdf"), tcs.Task,
                                                        RemoteFinalSceneScreenshot(topic.Id, discussion.Id));
            pdfAsm.Run();
        }

        public void reportGenerated(ReportCollector sender, object args)
        {
            ((TaskCompletionSource<ReportCollector>) args).SetResult(sender);
        }

        public Task<Dictionary<int, string>> FinalSceneScreenshot(int topicId, int discId)
        {
            //close opened public center to prevent d-editor conflicts 
            if (DiscWindows.Get().discDashboard != null)
            {
                DiscWindows.Get().discDashboard.Close();
                DiscWindows.Get().discDashboard = null;
            }

            PublicCenter pubCenter = new PublicCenter(UISharedRTClient.Instance,
                                                      () => { },
                                                      topicId, discId
                );

            pubCenter.Show();
            pubCenter.Hide();

            Task<Dictionary<int, string>> t = pubCenter.FinalSceneScreenshots();
            t.GetAwaiter().OnCompleted(() =>
                {
                    pubCenter.Close();
                    pubCenter = null;
                });

            return t;
        }

        private TaskCompletionSource<Dictionary<int, byte[]>> remoteScreenshotTCS;

        public Task<Dictionary<int, byte[]>> RemoteFinalSceneScreenshot(int topicId, int discId)
        {
            UISharedRTClient.Instance.clienRt.onScreenshotResponse += onScreenshotResponse;
            UISharedRTClient.Instance.clienRt.SendScreenshotRequest(topicId, discId);


            remoteScreenshotTCS = new TaskCompletionSource<Dictionary<int, byte[]>>();
            return remoteScreenshotTCS.Task;
        }

        private void onScreenshotResponse(Dictionary<int, byte[]> resp)
        {
            UISharedRTClient.Instance.clienRt.onScreenshotResponse -= onScreenshotResponse;

            remoteScreenshotTCS.SetResult(resp);
            remoteScreenshotTCS = null;
        }
    }
}