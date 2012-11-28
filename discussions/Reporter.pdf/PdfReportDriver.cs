using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discussions;
using Discussions.DbModel;
using Discussions.rt;

namespace Reporter.pdf
{
    class PdfReportDriver
    {
        public async Task Run(Session session, Topic topic, Discussion discussion, Person person)
        {
            //tests
            //var ctx = new DiscCtx(ConfigManager.ConnStr);
            //var discussion = ctx.Discussion.First();
            //var topic = discussion.Topic.First();
            //var session = ctx.Session.FirstOrDefault();
            //var pers = session.Person.First();

            //start hard report 
            var reportParameters = new ReportParameters(session.Person.Select(p=>p.Id).ToList(),
                                                        session, topic, discussion);
            var tcs = new TaskCompletionSource<ReportCollector>();
            new ReportCollector(null, reportGenerated, reportParameters, tcs);
            
            var pdfAsm = new Reporter.pdf.PdfAssembler2(discussion, topic, person, session,
                                                        Utils.RandomFilePath(".pdf"), tcs.Task,
                                                        FinalSceneScreenshot(topic.Id, discussion.Id));
            await pdfAsm.Run();
        }
        
        public void reportGenerated(ReportCollector sender, object args)
        {
            ((TaskCompletionSource<ReportCollector>)args).SetResult(sender);
        }

        public Task<string> FinalSceneScreenshot(int topicId, int discId)
        {    
            //close opened public center to prevent d-editor conflicts 
            if(DiscWindows.Get().discDashboard!=null)
            {
                DiscWindows.Get().discDashboard.Close();
                DiscWindows.Get().discDashboard = null;
            }               
            
            PublicCenter pubCenter = new PublicCenter(UISharedRTClient.Instance,
                                                      () => {}
                                                      );
        
            pubCenter.Show();
            pubCenter.Hide();
     
            Task<string> t = pubCenter.FinalSceneScreenshot(topicId, discId);
            t.GetAwaiter().OnCompleted(() =>
                {
                    pubCenter.Close();
                    pubCenter = null;
                });

            return t;
        }
    }
}
