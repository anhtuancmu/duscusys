using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Discussions;
using Discussions.DbModel;
using Discussions.model;
using Discussions.RTModel.Model;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using PdfSharp.Pdf;

namespace Reporter.pdf
{
    class PdfAssembler2
    {
        Discussion _discussion;
        Topic _topic;
        string _PdfPathName;
        Document _document;
        Person _person;
        Session _session;
        Task<ReportCollector> _hardReportTask;
        
        public PdfAssembler2(Discussion discussion, Topic topic, Person person, 
                             Session session, string PdfPathName, Task<ReportCollector> hardReportTask)
        {
            _discussion = discussion;
            _topic = topic;
            _PdfPathName = PdfPathName;
            _person = person;
            _session = session;
            _hardReportTask = hardReportTask;
        }

        public async Task Run()
        {
            _document = new Document();

            SetStyles();

            CoverPage();

            BasicInfo();

            await DiscussionBackground();

            DiscussionBgMediaTable();

            DiscussionBgSourcesTable();

            AllArgPoints();
     
            ClusterInformation(await _hardReportTask);

            LinkInformation(_hardReportTask.Result);

            Summary(_hardReportTask.Result);

            PdfDocumentRenderer pdfRenderer = new PdfDocumentRenderer(true, PdfFontEmbedding.Always);
            pdfRenderer.Document = _document;
            pdfRenderer.RenderDocument();

            pdfRenderer.PdfDocument.Save(_PdfPathName);

            Process.Start(_PdfPathName);
        }

        void CoverPage()
        {
            Section section = _document.AddSection();

            //blue background
            PdfTools2.AddPageBg(section, _document, 0x2C, 0xA1, 0xCF);          

            var p0 = PdfTools2.SectionHeader(section.AddParagraph("Tohoku University"));
            p0.Format.SpaceBefore = Unit.FromPoint(10);
            p0.Format.Alignment = ParagraphAlignment.Center;
            p0.Format.Font.Color = Colors.White;

            var p = section.AddParagraph("Discussion Support System");
            p.Format.Alignment = ParagraphAlignment.Center;
            p.Format.Font.Color = Colors.White;
            p.Format.Font.Size = 40;
            p.Format.SpaceBefore = Unit.FromPoint(250);

            var p2 = section.AddParagraph("Discussion report");
            p2.Format.Alignment = ParagraphAlignment.Center;
            p2.Format.Font.Color = Colors.White;
            p2.Format.Font.Size = 30;
            p2.Format.SpaceBefore = Unit.FromPoint(100);

            var p3 = PdfTools2.SectionHeader(section.AddParagraph(DateTime.Now.Date.ToShortDateString()));           
            p3.Format.Alignment = ParagraphAlignment.Center;
            p3.Format.Font.Color = Colors.White;
            p3.Format.SpaceBefore = Unit.FromPoint(280);
        }

        void BasicInfo()
        {
            var s = _document.AddSection();

            PdfTools2.SectionHeader(s.AddParagraph("Basic information"));

            var t = PdfTools2.TableDefaults(s.AddTable());

            var c0 = t.AddColumn(0.5 * ContentWidth());
            var c1 = t.AddColumn(0.5 * ContentWidth());

            var r0 = t.AddRow();
            r0.Cells[0].AddParagraph("Discussion");
            r0.Cells[1].AddParagraph(_discussion.Subject);

            var r1 = t.AddRow();
            r1.Cells[0].AddParagraph("Topic");
            r1.Cells[1].AddParagraph(_topic.Name);

            //session
            if (_person.Session != null)
            {
                var r2 = t.AddRow();
                r2.Cells[0].AddParagraph("Session");
                r2.Cells[1].AddParagraph(_person.Session.Name);

                var r3 = t.AddRow();
                r3.Cells[0].AddParagraph("Date and time");
                r3.Cells[1].AddParagraph(_person.Session.EstimatedDateTime.ToString());
            }
            else
            {
                s.AddParagraph("Session: no session for " + _person.Name);
            }

            var r4 = t.AddRow();
            r4.Cells[0].AddParagraph("Total time (one topic)");
            var cumulativeDuration = TimeSpan.FromSeconds(_topic.CumulativeDuration).ToString();
            r4.Cells[1].AddParagraph(cumulativeDuration);

            //session participants 
            if (_person.Session != null)
            {
                s.AddParagraph();
                s.AddParagraph("Participants");

                var partTbl = s.AddTable();
                partTbl.Borders.Color = Colors.Transparent;
                partTbl.Borders.Width = Unit.FromPoint(1);                

                var participants = DaoUtils.Participants(_topic, _person.Session);

                c0 = partTbl.AddColumn(0.5 * ContentWidth());
                c1 = partTbl.AddColumn(0.5 * ContentWidth());

                Row row = null;                
                var enumerator = participants.GetEnumerator();
                for (int i = 0; i < participants.Count(); i++)
                {
                    if (i % 2 == 0)
                        row = partTbl.AddRow();

                    enumerator.MoveNext();
                    var p = row.Cells[i % 2].AddParagraph(enumerator.Current.Name);
                    p.Format.Shading.Color = new MigraDoc.DocumentObjectModel.Color((uint)enumerator.Current.Color);                   
                }
            }
        }

        void SetStyles()
        {
            var normal = _document.Styles["Normal"];
            normal.ParagraphFormat.Font = new MigraDoc.DocumentObjectModel.Font("Segoe UI Light", Unit.FromPoint(14));
            normal.ParagraphFormat.Font.Color = Colors.Black;

            _document.DefaultPageSetup.RightMargin = 20;
            _document.DefaultPageSetup.LeftMargin = 20;
            _document.DefaultPageSetup.TopMargin = 20;
            _document.DefaultPageSetup.BottomMargin = 20;

            ///   s.ParagraphFormat.Shading.Color = Color.FromRgbColor(0xff, new Color(34,100,200));                            
        }

        Unit ContentWidth()
        {
            return _document.DefaultPageSetup.PageWidth -
                           (_document.DefaultPageSetup.LeftMargin + _document.DefaultPageSetup.RightMargin);
        }

        Task DiscussionBackground()
        {
            var bgUrl = string.Format("http://{0}/discsvc/bgpage?id={1}", ConfigManager.ServiceServer, _discussion.Id);

            var tcs = new TaskCompletionSource<int>();
            new WebScreenshoter(
                bgUrl,
                pathName =>
                {
                    Section section = _document.AddSection();
                    PdfTools2.SectionHeader(section.AddParagraph("Discussion Background"));

                    var slices = PdfTools2.SliceImage(pathName);
                    foreach (var slice in slices)
                    {
                        section = _document.AddSection();
                        var img = section.AddImage(slice);
                        img.Width = PdfTools2.PAGE_WIDTH;
                        img.RelativeHorizontal = RelativeHorizontal.Page;
                        img.RelativeVertical = RelativeVertical.Page;
                    }

                    tcs.SetResult(0);//ok, done
                },
              -1000
            );
            return tcs.Task;
        }

        void DiscussionBgMediaTable()
        {
            var s = _document.AddSection();

            s.AddParagraph("Media of discussion background").SectionHeader();
            MediaTable(s, _discussion.Attachment);            
        }

        void MediaTable(Section s, IEnumerable<Attachment> media, int tableColor=-1)
        {
            Table t = null;
            if(tableColor==-1)
                t = s.AddTable().TableDefaults();
            else
                t = s.AddTable().TableDefaults(tableColor);

            var colWidth = 0.5 * ContentWidth();
            var c0 = t.AddColumn(colWidth);
            var c1 = t.AddColumn(colWidth);

            var r0 = t.AddRow();
            r0.Cells[0].AddParagraph("Number");
            r0.Cells[1].AddParagraph("Media");

            double maxImgWidth = colWidth - 10; 
            int number = 1;
            foreach (var att in media)
            {
                var row = t.AddRow();
                row.BottomPadding = 5;
                row.TopPadding = 5;       

                //number                
                var header = row.Cells[0].AddParagraph();
                header.AddText(string.Format("{0}. {1}", number, att.Link));

                number++;                

                //thumb               
                var pathName = Utils2.RandomFilePath(".png");
                var bmpSrc = AttachmentManager.GetAttachmentBitmap3(att);            
                AttachmentManager.SaveBitmapSource(AttachmentManager.GetAttachmentBitmap3(att), pathName);

                var para = row.Cells[1].AddImage(pathName);
                if (att.Format == (int)AttachmentFormat.Pdf)//pdf thumb is low resolution
                    para.Height = maxImgWidth;
                else
                    para.Width = maxImgWidth;
                para.RelativeHorizontal = RelativeHorizontal.Column;
                para.RelativeVertical= RelativeVertical.Paragraph;
            }
        }

        void DiscussionBgSourcesTable()
        {
            var s = _document.AddSection();

            s.AddParagraph("Sources of discussion background").SectionHeader();
            SourcesTable(s, _discussion.Background.Source);
        }

        void SourcesTable(Section s, IEnumerable<Source> sources, int tableColor=-1)
        {
            Table t;
            if(tableColor==-1)
                t = s.AddTable().TableDefaults();
            else
                t = s.AddTable().TableDefaults(tableColor);

            var colWidth = ContentWidth();
            var c0 = t.AddColumn(colWidth);

            t.AddRow().Cells[0].AddParagraph("Sources");

            int number = 1;
            foreach (var src in sources)
            {
                var row = t.AddRow();
                row.BottomPadding = 5;
                row.TopPadding = 5;
       
                var para = row.Cells[0].AddParagraph();
                para.AddText(string.Format("{0}. {1}", number, src.Text));

                number++;
            }
        }

        void AllArgPoints()
        {
            var s = _document.AddSection();

            PdfTools2.SectionHeader(s.AddParagraph("Argument points"));

            foreach (var pers in _session.Person)
            {
                var para = s.AddParagraph("Argument points of " + pers.Name);                                
                
                var argPointsOf = DaoUtils.ArgPointsOf(pers, _discussion);
                if (argPointsOf.Count() > 0)
                {
                    foreach (var ap in argPointsOf)
                    {
                        if (!ap.SharedToPublic)
                            continue;

                        ArgPointNode(s, ap);
                        s.AddParagraph("\n\n");
                    }
                }
                else
                {
                    s.AddParagraph("<No arguments>\n\n");                    
                }                
            }
        }

        void ArgPointNode(Section s, ArgPoint ap)
        {
            //arg.point header table  
            var t = s.AddTable().TableDefaults(ap.Person.Color);      

            var c0 = t.AddColumn(0.2 * ContentWidth());
            var c1 = t.AddColumn(0.8 * ContentWidth());

            var r1 = t.AddRow();
            r1.Format.Font.Bold = true;
            r1.Cells[0].AddParagraph("Point #" + ap.OrderNumber);
            r1.Cells[1].AddParagraph(ap.Point);

            var r0 = t.AddRow();
            r0.Cells[0].AddParagraph("Author");
            r0.Cells[1].AddParagraph(ap.Person.Name);

            //description
            var descr = s.AddParagraph(ap.Description.Text);
            descr.Format.Shading.Color = new MigraDoc.DocumentObjectModel.Color((uint)ap.Person.Color);
            descr.Format.LeftIndent = Unit.FromPoint(-3); //align with table 
            descr.Format.RightIndent = Unit.FromPoint(5);
            descr.Format.SpaceBefore = Unit.FromPoint(1);

            //point's media
            MediaTable(s, ap.Attachment, ap.Person.Color);

            //point's sources
            SourcesTable(s, ap.Description.Source, ap.Person.Color);
 
            //point's comments
            CommentsTable(s, ap.Comment, ap.Person.Color);
        }

        void CommentsTable(Section s, IEnumerable<Comment> comments, int color)
        {
            var t2 = s.AddTable().TableDefaults(color);
            t2.AddColumn(0.2 * ContentWidth());
            t2.AddColumn(0.8 * ContentWidth());

            var hdrRow = t2.AddRow();
            hdrRow.Cells[0].AddParagraph("Author");
            hdrRow.Cells[1].AddParagraph("Comment");

            foreach (var comment in comments)
            {
                if (comment.Person == null || comment.Text == "New comment")
                    continue;
                
                var row = t2.AddRow();
                row.Cells[0].AddParagraph(comment.Person.Name);
                row.Cells[0].Shading.Color = new MigraDoc.DocumentObjectModel.Color((uint)comment.Person.Color);
                row.Cells[1].AddParagraph(comment.Text);
            }
        }

        void ClusterInformation(ReportCollector hardReport)
        {            
            var s = _document.AddSection();

            PdfTools2.SectionHeader(s.AddParagraph("Cluster information"));

            if (hardReport.ClusterReports.Count > 0)
            {
                foreach (var clusterReport in hardReport.ClusterReports)
                {
                    ClusterTable(s, clusterReport);
                    s.AddParagraph();
                }
            }
            else
            {
                s.AddParagraph("<No clusters>\n\n");
            }
        }

        void ClusterTableLine(Table t, string clustTitle, int clustId, bool indent, Person pers)
        {
            Paragraph p;
            Row r;
            if (string.IsNullOrEmpty(clustTitle))
            {
                r = t.AddRow();
                p = r.Cells[0].AddParagraph(string.Format("<Cluster_{0}>", clustId));
            }
            else
            {
                r = t.AddRow();
                p = r.Cells[0].AddParagraph(string.Format("Cluster \"{0}\"", clustTitle));
            }

            if (pers!=null)
                r.Shading.Color = pers.PersonToColor();
            if (indent)
                p.LeftParaIndent(20);
        }

        void ArgPointTableLine(Table t, ArgPoint point, bool indent)
        {
            var r = t.AddRow();
            Paragraph p = r.Cells[0].AddParagraph(string.Format("Arg.point#{0}:  {1}", point.OrderNumber, point.Point));
            r.Cells[0].Shading.Color = new MigraDoc.DocumentObjectModel.Color((uint)point.Person.Color);
            if (indent)
                p.LeftParaIndent(20);
        }

        void ClusterTable(Section s, ClusterReport clustReport)
        {            
            var t = s.AddTable().TableDefaults(); 
            t.AddColumn(ContentWidth());
               
            var hdrRow = t.AddRow();
            hdrRow.Shading.Color = clustReport.initialOwner.PersonToColor();
            hdrRow.Cells[0].AddParagraph("Cluster");

            ClusterTableLine(t, clustReport.clusterTitle, clustReport.clusterId, false, clustReport.initialOwner);

            foreach (var point in clustReport.points)           
                ArgPointTableLine(t, point, true);                                          
        }

        void LinkInformation(ReportCollector hardReport)
        {
            var s = _document.AddSection();

            PdfTools2.SectionHeader(s.AddParagraph("Link information"));

            if (hardReport.LinkReports.Count > 0)
            {
                foreach (var linkReport in hardReport.LinkReports)
                {
                    LinkTable(s, linkReport);
                    s.AddParagraph();
                }
            }
            else
            {
                s.AddParagraph("<No links>\n\n");
            }
        }

        void LinkTable(Section s, LinkReportResponse link)
        {
            var t = s.AddTable().TableDefaults();
            t.AddColumn(ContentWidth());

            var hdrRow = t.AddRow();
            hdrRow.Cells[0].Shading.Color = link.initOwner.PersonToColor();
            hdrRow.Cells[0].AddParagraph("Link");

            if (!string.IsNullOrEmpty(link.Caption))
            {                
                var r = t.AddRow();
                r.Cells[0].Shading.Color = link.initOwner.PersonToColor();
                r.Cells[0].AddParagraph(string.Format("Link \"{0}\"", link.Caption));
            }

            if (link.EndpointArgPoint1)            
                ArgPointTableLine(t, link.ArgPoint1, true);                           
            else
                ClusterTableLine(t, link.ClusterCaption1, link.IdOfCluster1, true, null);

            if (link.EndpointArgPoint2)
                ArgPointTableLine(t, link.ArgPoint2, true);
            else
                ClusterTableLine(t, link.ClusterCaption2, link.IdOfCluster2, true, null);  
        }

        void Summary(ReportCollector hardReport)
        {
            var s = _document.AddSection();

            PdfTools2.SectionHeader(s.AddParagraph("Summary information"));

            SummaryTable(s, hardReport);
        }

        void SummaryTable(Section s, ReportCollector hardReport)
        {
            var t = s.AddTable().TableDefaults();            
            t.AddColumn(0.5 * ContentWidth());
            t.AddColumn(0.5 * ContentWidth());

            var r = t.AddRow();
            r.Cells[0].AddParagraph("Arg.points");
            r.Cells[1].AddParagraph(hardReport.TotalArgPointReport.numPoints.ToString());

            var r1 = t.AddRow();
            r1.Cells[0].AddParagraph("Attachments");
            r1.Cells[1].AddParagraph(hardReport.TotalArgPointReport.numMediaAttachments.ToString());

            var r2 = t.AddRow();
            r2.Cells[0].AddParagraph("Sources (total events) ");
            r2.Cells[1].AddParagraph(hardReport.EventTotals.TotalSourceAdded.ToString());

            var r3 = t.AddRow();
            r3.Cells[0].AddParagraph("Clusters (total events)");
            r3.Cells[1].AddParagraph(hardReport.EventTotals.TotalClusterCreated.ToString());
            
            var r4 = t.AddRow();
            r4.Cells[0].AddParagraph("Links (total events)");
            r4.Cells[1].AddParagraph(hardReport.EventTotals.TotalLinkCreated.ToString());

            var r5 = t.AddRow();
            r5.Cells[0].AddParagraph("Comments");
            r5.Cells[1].AddParagraph(hardReport.TotalArgPointReport.numComments.ToString());
        }
    }
}
