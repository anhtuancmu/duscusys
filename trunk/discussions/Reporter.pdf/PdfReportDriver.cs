using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discussions;
using Discussions.DbModel;

namespace Reporter.pdf
{
    class PdfReportDriver
    {
        public async Task Run()
        {
            var ctx = new DiscCtx(ConfigManager.ConnStr);
            var disc = ctx.Discussion.First();
            var topic = disc.Topic.First();
            var session = ctx.Session.FirstOrDefault();
            var pers = session.Person.First();

            //start hard report 
            var reportParameters = new ReportParameters(session.Person.Select(p=>p.Id).ToList(),
                                                        session, topic, disc);
            var tcs = new TaskCompletionSource<ReportCollector>();
            new ReportCollector(null, reportGenerated, reportParameters, tcs);

            var pdfAsm = new Reporter.pdf.PdfAssembler2(disc, topic, pers, session, @"C:\projects\TDS\pdfasm.pdf", tcs.Task);
            await pdfAsm.Run();
        }

        public void reportGenerated(ReportCollector sender, object args)
        {
            ((TaskCompletionSource<ReportCollector>)args).SetResult(sender);
        } 
    }
}
