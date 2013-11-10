//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.IO;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Windows;
//using Discussions;
//using Discussions.DbModel;
//using PDFjet.NET;
//using Color = PDFjet.NET.Color;
//using Font = PDFjet.NET.Font;
//using Paragraph = PDFjet.NET.Paragraph;

//namespace Reporter.pdf
//{
//    public class PdfAssembler3
//    {
//        private Discussion _discussion;
//        private Topic _topic;
//        private string _PdfPathName;
//        private PDF _document;
//        private Person _person;
//        private Session _session;
//        private Task<ReportCollector> _hardReportTask;
//        private Task<Dictionary<int, byte[]>> _finalScene;
//        private List<string> _images = new List<string>();

//        private FileStream _fontStream;

//        private Font _mainFont;

//        public PdfAssembler3(Discussion discussion, Topic topic, Person person,
//                             Session session, string PdfPathName, Task<ReportCollector> hardReportTask,
//                             Task<Dictionary<int, byte[]>> finalScene)
//        {
//            _discussion = discussion;
//            _topic = topic;
//            _PdfPathName = PdfPathName;
//            _person = person;
//            _session = session;
//            _hardReportTask = hardReportTask;
//            _finalScene = finalScene;

//            if (topic == null)
//            {
//                MessageBox.Show("Null topic");
//            }
//            if (discussion == null)
//            {
//                MessageBox.Show("Null discussion");
//            }
//        }

//        public async Task<List<string>> RunAsync()
//        {
//          //  try
//            {
//                var fos = new FileStream(_PdfPathName, FileMode.Create);
//                _fontStream = new FileStream(System.IO.Path.Combine(Utils.ExeDir(), @"Assets\Meiryo.ttf"), FileMode.Open, FileAccess.Read);

//                var bos = new BufferedStream(fos);
//                _document = new PDF(bos);

//                SetStyles();

//                CoverPage();

//                TableOfContents();

//                BasicInfo();

//                await DiscussionBackground();

//                DiscussionBgMediaTable();

//                //DiscussionBgSourcesTable();

//                //await FinalScene();

//                //Summary(_hardReportTask.Result);

//                //AllArgPoints();

//                //ClusterInformation(await _hardReportTask);

//                //LinkInformation(_hardReportTask.Result);          

//                _document.Flush();
//                bos.Close();

//                Process.Start(_PdfPathName);
//            }
//            //catch (Exception e)
//            //{
//            //    MessageBox.Show(e.StackTrace);
//            //}

//            return _images;
//        }

//        private void CoverPage()
//        {
//            var page = new Page(_document, A4.PORTRAIT);

//            //background
//            page.SetBrushColor(PdfTools3.CoverPageBg);
//            page.FillRect(0, 0, page.GetWidth(), page.GetHeight());

//            //page.SetBrushColor(Color.white);
//            //page.FillRect(4 * PdfTools3.LEFT_MARGIN, 450, PdfTools3.ContentWidth(page.GetWidth()) - 6 * PdfTools3.LEFT_MARGIN, 1);

//            _mainFont = new Font(_document, _fontStream, CodePage.UNICODE, Embed.YES);

//            {
//                _mainFont.SetSize(20);

//                var column = PdfTools3.AdjustCol(_mainFont, page.GetWidth(), page.GetHeight());

//                var p1 = new Paragraph().Centered();
//                p1.Add(new TextLine(_mainFont, "Tohoku University").WithColor(Color.white));
//                column.AddParagraph(p1);
//                column.SetPosition(0, 200);
//                column.DrawOn(page);
//            }

//            {
//                _mainFont.SetSize(30);
//                var p2 = new Paragraph().Centered();
//                p2.Add(new TextLine(_mainFont, "Discussion Support System").WithColor(Color.white));
//                var column = PdfTools3.AdjustCol(_mainFont, page.GetWidth(), page.GetHeight());
//                column.AddParagraph(p2);
//                column.SetPosition(0, 270);
//                column.DrawOn(page);
//            }

//            {
//                _mainFont.SetSize(20);
//                var column = PdfTools3.AdjustCol(_mainFont, page.GetWidth(), page.GetHeight());

//                var p22 = new Paragraph().Centered();
//                p22.Add(new TextLine(_mainFont, "Topic report").WithColor(Color.white));
//                column.AddParagraph(p22);

//                column.AddParagraph(PdfTools3.EmptyPara(_mainFont, 1));

//                column.SetPosition(0, 350);
//                column.DrawOn(page);
//            }

//            //footer
//            {
//                _mainFont.SetSize(14);

//                var column2 = PdfTools3.AdjustCol(_mainFont, page.GetWidth(), page.GetHeight());
//                column2.SetPosition(0, page.GetHeight() - 50);
//                var p3 = new Paragraph().Centered();
//                var footer = new TextLine(_mainFont, DateTime.Now.Date.ToShortDateString()).WithColor(Color.white);
//                p3.Add(footer);
//                column2.AddParagraph(p3);
//                column2.DrawOn(page);
//            }
//        }

//        private void BasicInfo()
//        {
//            var page = new Page(_document, A4.PORTRAIT);

//            //insert section header
//            PdfTools3.MakeSectionHeader("Basic information",_mainFont).DrawOn(page);

//            //restore font size after header
//            _mainFont.SetSize(PdfTools3.DEFAULT_FONT_SIZE);
//            var table = new Table();
//            table.SetPosition(PdfTools3.LEFT_MARGIN, PdfTools3.TOP_MARGIN + 10);

//            var cells = new List<List<Cell>>();

//            var row = new List<Cell> {"Discussion".ToCell(_mainFont), _discussion.Subject.ToCell(_mainFont)};
//            cells.Add(row);

//            row = new List<Cell> {"Topic".ToCell(_mainFont), _topic.Name.ToCell(_mainFont)};
//            cells.Add(row);


//            //session
//            if (_person.Session != null)
//            {
//                row = new List<Cell> {"Session".ToCell(_mainFont), _person.Session.Name.ToCell(_mainFont)};
//                cells.Add(row);

//                row = new List<Cell>
//                    {
//                        "Date and time".ToCell(_mainFont),
//                        _person.Session.EstimatedDateTime.ToString().ToCell(_mainFont)
//                    };
//                cells.Add(row);
//            }
//            else
//            {
//                row = new List<Cell> {("Session: no session for " + _person.Name).ToCell(_mainFont)};
//                cells.Add(row);
//            }

//            var r4 = new List<Cell>
//                {
//                    "Total time (one topic)".ToCell(_mainFont),
//                    TimeSpan.FromSeconds(_topic.CumulativeDuration).ToString().ToCell(_mainFont)
//                };
//            cells.Add(r4);

//            table.SetData(cells);
//            table.TableDefaults(page.GetWidth(), 2);
//            table.DrawOn(page);

//            //session participants 
//            if (_person.Session != null)
//            {
//                var subtitle = new TextLine(_mainFont, "Participants");
//                subtitle.SetPosition(PdfTools3.LEFT_MARGIN, 240);
//                subtitle.DrawOn(page);

//                var partTbl = new Table();                
//                cells = new List<List<Cell>>();

//                var participants = DaoUtils.Participants(_topic, _person.Session);

//                row = new List<Cell>();
//                foreach(var p in participants)
//                {
//                    var cell = p.Name.ToCell(_mainFont);
//                    cell.SetBgColor(p.Color);
//                    row.Add(cell);

//                    //next line
//                    if (row.Count == 2)
//                    {
//                        cells.Add(row);
//                        row = new List<Cell>();
//                    }
//                }
//                if (row.Count > 0)
//                    cells.Add(row);

//                partTbl.SetData(cells);
//                partTbl.TableDefaults(page.GetWidth(), 2);
//                partTbl.SetPosition(PdfTools3.LEFT_MARGIN, 250);
//                partTbl.DrawOn(page);
//            }
//        }

//        private void SetStyles()
//        {
//            //var normal = _document.Styles["Normal"];
//            //normal.ParagraphFormat.Font = new MigraDoc.DocumentObjectModel.Font("Segoe UI Light", Unit.FromPoint(14));
//            //normal.ParagraphFormat.Font.Color = Colors.Black;

//            //_document.DefaultPageSetup.RightMargin = 20;
//            //_document.DefaultPageSetup.LeftMargin = 20;
//            //_document.DefaultPageSetup.TopMargin = 20;
//            //_document.DefaultPageSetup.BottomMargin = 20;

//            //var style = _document.Styles.AddStyle("TOC", "Normal");
//            //style.ParagraphFormat.AddTabStop("16cm", TabAlignment.Right, TabLeader.Dots);
//            //style.ParagraphFormat.Font.Color = Colors.Blue;                          
//        }

//        private Task DiscussionBackground()
//        {
//            var bgUrl = string.Format("http://{0}/discsvc/bgpage?id={1}", ConfigManager.ServiceServer, _discussion.Id);

//            var tcs = new TaskCompletionSource<int>();
//            new WebScreenshoter(
//                bgUrl,
//                pathName =>
//                    {
//                        var slices = PdfTools3.SliceImage(pathName);
//                        foreach (var slice in slices)
//                        {
//                            var page = new Page(_document, A4.PORTRAIT);
//                            var image1 = new Image(_document, new FileStream(slice, FileMode.Open), ImageType.PNG); 
//                            image1.ScaleBy(page.GetWidth()/image1.GetWidth());
//                            image1.DrawOn(page);
//                        }

//                        tcs.SetResult(0); //ok, done
//                    },
//                1300
//                );
//            return tcs.Task;
//        }

//        private void DiscussionBgMediaTable()
//        {
//            var page = new Page(_document, A4.PORTRAIT);

//            PdfTools3.MakeSectionHeader("Media of discussion background", _mainFont).DrawOn(page);

//           // MediaTable(s, _discussion.Attachment);            
//        }

//        void MediaTable(Page page, IEnumerable<Attachment> media, int tableColor = -1)
//        {
//            //var t = new Table();
//            //var cells = new List<List<Cell>>();
        
//            //if (tableColor == -1)
//            //    t = s.AddTable().TableDefaults();
//            //else
//            //    t = s.AddTable().TableDefaults(tableColor);

//            //var colWidth = 0.5 * ContentWidth();
//            //var c0 = t.AddColumn(colWidth);
//            //var c1 = t.AddColumn(colWidth);

//            //var r0 = t.AddRow();
//            //r0.Cells[0].AddParagraph().AddBold("Number");
//            //r0.Cells[1].AddParagraph().AddBold("Media");

//            //double maxImgWidth = colWidth - 10;
//            //int number = 1;
//            //foreach (var att in media)
//            //{
//            //    var row = t.AddRow();
//            //    row.BottomPadding = 5;
//            //    row.TopPadding = 5;

//            //    //number                
//            //    var header = row.Cells[0].AddParagraph();
//            //    header.AddText(string.Format("{0}. {1}", number, att.Link));

//            //    number++;

//            //    //thumb               
//            //    var pathName = Utils2.RandomFilePath(".png");
//            //    var bmpSrc = AttachmentManager.GetAttachmentBitmap3(att);
//            //    AttachmentManager.SaveBitmapSource(AttachmentManager.GetAttachmentBitmap3(att), pathName);

//            //    var para = row.Cells[1].AddImage(pathName);
//            //    if (att.Format == (int)AttachmentFormat.Pdf)//pdf thumb is low resolution
//            //        para.Height = maxImgWidth;
//            //    else
//            //        para.Width = maxImgWidth;
//            //    para.RelativeHorizontal = RelativeHorizontal.Column;
//            //    para.RelativeVertical = RelativeVertical.Paragraph;
//            //}
//        }

//        //void DiscussionBgSourcesTable()
//        //{
//        //    //var s = _document.AddSection();
//        //    var s = _document.LastSection;
//        //    s.AddParagraph();
//        //    s.AddParagraph("Sources of discussion background").SectionHeader();
//        //    SourcesTable(s, _discussion.Background.Source);
//        //}

//        //void SourcesTable(Section s, IEnumerable<Source> sources, int tableColor=-1)
//        //{
//        //    Table t;
//        //    if(tableColor==-1)
//        //        t = s.AddTable().TableDefaults();
//        //    else
//        //        t = s.AddTable().TableDefaults(tableColor);

//        //    var colWidth = ContentWidth();
//        //    var c0 = t.AddColumn(colWidth);

//        //    t.AddRow().Cells[0].AddParagraph().AddBold("Sources");

//        //    int number = 1;
//        //    foreach (var src in sources)
//        //    {
//        //        var row = t.AddRow();
//        //        row.BottomPadding = 5;
//        //        row.TopPadding = 5;

//        //        var para = row.Cells[0].AddParagraph();
//        //        para.AddText(string.Format("{0}. {1}", number, src.Text));

//        //        number++;
//        //    }
//        //}

//        //void AllArgPoints()
//        //{
//        //    var s = _document.AddSection();

//        //    PdfTools2.SectionHeader(s.AddParagraph("Argument points"));

//        //    foreach (var pers in _session.Person)
//        //    {
//        //        if (pers == null)
//        //        {
//        //            MessageBox.Show("skipping a null person in session");
//        //            continue;
//        //        }

//        //        bool personValid = true;
//        //        ArgPoint invalidAp = null;
//        //        foreach(var ap in pers.ArgPoint)
//        //            if (ap.Topic == null)
//        //            {
//        //                personValid = false;
//        //                invalidAp = ap;
//        //            }

//        //        if (!personValid)
//        //        {
//        //            MessageBox.Show(string.Format("{0}'s arg.point \"{1}\" has null (undefined) topic. Skipping the arg.point in report", pers.Name, invalidAp.Point));
//        //            continue;                    
//        //        }

//        //        var para = s.AddParagraph().AddBold("Argument points of " + pers.Name);                             

//        //        var argPointsOf = DaoUtils.ArgPointsOf(pers, _discussion, _topic);
//        //        if (argPointsOf.Count() > 0)
//        //        {
//        //            foreach (var ap in argPointsOf)
//        //            {
//        //                if (!ap.SharedToPublic)
//        //                    continue;

//        //                ArgPointNode(s, ap);
//        //                s.AddParagraph("\n\n");
//        //            }
//        //        }
//        //        else
//        //        {
//        //            s.AddParagraph("<No arguments>\n\n");                    
//        //        }                
//        //    }
//        //}

//        //void ArgPointNode(Section s, ArgPoint ap)
//        //{
//        //    //arg.point header table  
//        //    var t = s.AddTable().TableDefaults(ap.Person.Color);      

//        //    var c0 = t.AddColumn(0.2 * ContentWidth());
//        //    var c1 = t.AddColumn(0.8 * ContentWidth());

//        //    var r1 = t.AddRow();
//        //    r1.Format.Font.Bold = true;
//        //    r1.Cells[0].AddParagraph().AddBold("Point #" + ap.OrderNumber);
//        //    r1.Cells[1].AddParagraph(ap.Point);

//        //    //var r0 = t.AddRow();
//        //    //r0.Cells[0].AddParagraph("Author");
//        //    //r0.Cells[1].AddParagraph(ap.Person.Name);

//        //    //description
//        //    var descr = s.AddParagraph(ap.Description.Text);
//        //    descr.Format.Shading.Color = new MigraDoc.DocumentObjectModel.Color((uint)ap.Person.Color);
//        //    descr.AlignWithTable();

//        //    //point's media
//        //    MediaTable(s, ap.Attachment, ap.Person.Color);

//        //    //point's sources
//        //    SourcesTable(s, ap.Description.Source, ap.Person.Color);

//        //    //point's comments
//        //    CommentsTable(s, ap.Comment, ap.Person.Color);
//        //}

//        //void CommentsTable(Section s, IEnumerable<Comment> comments, int color)
//        //{
//        //    var t2 = s.AddTable().TableDefaults(color);
//        //    t2.AddColumn(0.2 * ContentWidth());
//        //    t2.AddColumn(0.8 * ContentWidth());

//        //    var hdrRow = t2.AddRow();
//        //    hdrRow.Cells[0].AddParagraph().AddBold("Author");
//        //    hdrRow.Cells[1].AddParagraph().AddBold("Comment");

//        //    foreach (var comment in comments)
//        //    {
//        //        if (comment.Person == null || comment.Text == "New comment")
//        //            continue;

//        //        var row = t2.AddRow();
//        //        row.Cells[0].AddParagraph(comment.Person.Name);
//        //        row.Cells[0].Shading.Color = new MigraDoc.DocumentObjectModel.Color((uint)comment.Person.Color);
//        //        row.Cells[1].AddParagraph(comment.Text);
//        //    }
//        //}

//        //async void ClusterInformation(ReportCollector hardReport)
//        //{            
//        //    var s = _document.AddSection();

//        //    PdfTools2.SectionHeader(s.AddParagraph("Cluster information"));

//        //    if (hardReport.ClusterReports.Count > 0)
//        //    {
//        //        var screenshots = await _finalScene;
//        //        foreach (var clusterReport in hardReport.ClusterReports)
//        //        {                  
//        //            ClusterTable(s, clusterReport, screenshots[clusterReport.clusterShId]);                   
//        //            s.AddParagraph();
//        //        }
//        //    }
//        //    else
//        //    {
//        //        s.AddParagraph("<No clusters>\n\n");
//        //    }
//        //}

//        //void ClusterTableLine(Table t, string clustTitle, int clustId, bool indent, Person pers)
//        //{
//        //    Paragraph p;
//        //    Row r;
//        //    if (string.IsNullOrEmpty(clustTitle))
//        //    {
//        //        r = t.AddRow();
//        //        p = r.Cells[0].AddParagraph(string.Format("<Cluster_{0}>", clustId));
//        //    }
//        //    else
//        //    {
//        //        r = t.AddRow();
//        //        p = r.Cells[0].AddParagraph(string.Format("Cluster \"{0}\"", clustTitle));
//        //    }

//        //    if (pers!=null)
//        //        r.Shading.Color = pers.PersonToColor();
//        //    if (indent)
//        //        p.LeftParaIndent(20);
//        //}

//        //void ArgPointTableLine(Table t, ArgPoint point, bool indent)
//        //{
//        //    var r = t.AddRow();
//        //    Paragraph p = r.Cells[0].AddParagraph(string.Format("Arg.point#{0}:  {1}", point.OrderNumber, point.Point));
//        //    r.Cells[0].Shading.Color = new MigraDoc.DocumentObjectModel.Color((uint)point.Person.Color);
//        //    if (indent)
//        //        p.LeftParaIndent(20);
//        //}

//        //void ClusterTable(Section s, ClusterReport clustReport, byte[] image)
//        //{            
//        //    var t = s.AddTable().TableDefaults(); 
//        //    t.AddColumn(ContentWidth());

//        //    var hdrRow = t.AddRow();
//        //    hdrRow.Shading.Color = clustReport.initialOwner.PersonToColor();
//        //    hdrRow.Cells[0].AddParagraph().AddBold("Cluster");

//        //    ClusterTableLine(t, clustReport.clusterTitle, clustReport.clusterId, false, clustReport.initialOwner);

//        //    foreach (var point in clustReport.points)           
//        //        ArgPointTableLine(t, point, true);

//        //    AddLinkOrClusterImg(s, image);                 
//        //}

//        //async void LinkInformation(ReportCollector hardReport)
//        //{
//        //    var s = _document.AddSection();  

//        //    PdfTools2.SectionHeader(s.AddParagraph("Link information"));

//        //    if (hardReport.LinkReports.Count > 0)
//        //    {
//        //        var screenshots = await _finalScene;

//        //        foreach (var linkReport in hardReport.LinkReports)
//        //        {
//        //            LinkTable(s, linkReport, screenshots[linkReport.linkShId]);
//        //            s.AddParagraph();
//        //        }
//        //    }
//        //    else
//        //    {
//        //        s.AddParagraph("<No links>\n\n");
//        //    }
//        //}

//        //void LinkTable(Section s, LinkReportResponse link, byte[] image)
//        //{            
//        //    var t = s.AddTable().TableDefaults();
//        //    t.AddColumn(ContentWidth());

//        //    var hdrRow = t.AddRow();
//        //    hdrRow.Cells[0].Shading.Color = link.initOwner.PersonToColor();
//        //    hdrRow.Cells[0].AddParagraph().AddBold("Link");

//        //    if (!string.IsNullOrEmpty(link.Caption))
//        //    {                
//        //        var r = t.AddRow();
//        //        r.Cells[0].Shading.Color = link.initOwner.PersonToColor();
//        //        r.Cells[0].AddParagraph(string.Format("Link \"{0}\"", link.Caption));
//        //    }

//        //    if (link.EndpointArgPoint1)            
//        //        ArgPointTableLine(t, link.ArgPoint1, true);                           
//        //    else
//        //        ClusterTableLine(t, link.ClusterCaption1, link.IdOfCluster1, true, null);

//        //    if (link.EndpointArgPoint2)
//        //        ArgPointTableLine(t, link.ArgPoint2, true);
//        //    else
//        //        ClusterTableLine(t, link.ClusterCaption2, link.IdOfCluster2, true, null);

//        //    //AddLinkOrClusterImg(s, pathname);           
//        //}

//        //void AddLinkOrClusterImg(Section s, byte[] image)
//        //{
//        //    var imgPara = s.AddParagraph();           
//        //    imgPara.AlignWithTable();
//        //    var pathName = PdfTools2.ImageBytesToFile(image, _images);
//        //    var img = imgPara.AddImage(pathName);

//        //    var bmp = new Bitmap(pathName);      
//        //    int w = bmp.Width;
//        //    int h = bmp.Height;
//        //    bmp.Dispose();

//        //    var maxHeight =  0.3 * _document.DefaultPageSetup.PageHeight;
//        //    if (h > maxHeight)
//        //    {
//        //        img.Height = maxHeight;
//        //        w = (int)maxHeight * w / h;
//        //        h = (int)maxHeight;               
//        //    }

//        //    var maxWidth = 0.5 * ContentWidth();
//        //    if (w > maxWidth)
//        //    {
//        //        img.Width = maxWidth;
//        //        img.Height = maxWidth * h / w;
//        //    }
//        //}

//        //void Summary(ReportCollector hardReport)
//        //{
//        //    //var s = _document.AddSection();
//        //    var s = _document.LastSection;
//        //    s.AddParagraph();
//        //    PdfTools2.SectionHeader(s.AddParagraph("Summary information"));

//        //    SummaryTable(s, hardReport);
//        //}

//        //void SummaryTable(Section s, ReportCollector hardReport)
//        //{
//        //    var t = s.AddTable().TableDefaults();            
//        //    t.AddColumn(0.5 * ContentWidth());
//        //    t.AddColumn(0.5 * ContentWidth());

//        //    var r = t.AddRow();
//        //    r.Cells[0].AddParagraph("Arg.points");
//        //    r.Cells[1].AddParagraph(hardReport.TotalArgPointReport.numPoints.ToString());

//        //    var r1 = t.AddRow();
//        //    r1.Cells[0].AddParagraph("Attachments");
//        //    r1.Cells[1].AddParagraph(hardReport.TotalArgPointReport.numMediaAttachments.ToString());

//        //    var r2 = t.AddRow();
//        //    r2.Cells[0].AddParagraph("Sources (total events) ");           
//        //    r2.Cells[1].AddParagraph(hardReport.EventTotals.TotalSourceAdded.ToString());

//        //    var r3 = t.AddRow();
//        //    r3.Cells[0].AddParagraph("Clusters");
//        //    r3.Cells[1].AddParagraph(hardReport.ClusterReports.Count.ToString());

//        //    var r4 = t.AddRow();
//        //    r4.Cells[0].AddParagraph("Links");
//        //    r4.Cells[1].AddParagraph(hardReport.LinkReports.Count.ToString());

//        //    var r5 = t.AddRow();
//        //    r5.Cells[0].AddParagraph("Comments");
//        //    r5.Cells[1].AddParagraph(hardReport.TotalArgPointReport.numComments.ToString());
//        //}

//        //async Task FinalScene()
//        //{
//        //    //var s = _document.AddSection();
//        //    var s = _document.LastSection;
//        //    s.AddParagraph();
//        //    PdfTools2.SectionHeader(s.AddParagraph("Final Public Dashboard"));
//        //    var screenshots = await _finalScene;
//        //    var imgPara = s.AddParagraph();
//        //    var pathName = PdfTools2.ImageBytesToFile(screenshots[-1], _images);            
//        //    var img = imgPara.AddImage(pathName);              
//        //    img.Width = ContentWidth();
//        //}       

//        /// <summary>
//        /// Defines the cover page.
//        /// </summary>
//        public void TableOfContents()
//        {
//            //var section = _document.LastSection;             
//            //Paragraph paragraph = PdfTools2.SectionHeader(section.AddParagraph("Table of Contents"));

//            //paragraph = section.AddParagraph();
//            //paragraph.Style = "TOC"; 
//            //Hyperlink hyperlink = paragraph.AddHyperlink("Basic information", HyperlinkType.Bookmark);         
//            //hyperlink.AddText("Basic information\t");
//            //hyperlink.AddPageRefField("BasicInfo");

//            //paragraph = section.AddParagraph();
//            //paragraph.Style = "TOC";
//            //Hyperlink hyperlink2 = paragraph.AddHyperlink("Discussion background media", HyperlinkType.Bookmark);
//            //hyperlink2.AddText("Basic information\t");
//            //hyperlink2.AddPageRefField("discbgmedia");            
//        }
//    }
//}