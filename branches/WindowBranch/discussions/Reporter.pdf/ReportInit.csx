using System.Linq;
using System.Threading.Tasks;

App app = new Discussions.App();
app.InitializeComponent();
Task.Factory.StartNew(() => app.Run());


//var pdfTopic = CtxSingleton.Get().Topic.FirstOrDefault(t => t.Id == 11);

//(new PdfReportDriver()).Run(SessionInfo.Get().person.Session, pdfTopic, SessionInfo.Get().discussion, SessionInfo.Get()