using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Discussions;
using Discussions.DbModel;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;

namespace Reporter.pdf
{
    public class PdfAssembler
    {
        private Discussion _discussion;
        private Topic _topic;
        private string _PdfPathName;
        private Document _document;
        private Person _person;

        public PdfAssembler(Discussion discussion, Topic topic, Person person, string PdfPathName)
        {
            this._discussion = discussion;
            this._topic = topic;
            this._PdfPathName = PdfPathName;
            this._person = person;
        }

        public async Task Run()
        {
            _document = new Document();
            try
            {
                PdfTools.SwitchBgColor(_document, new BaseColor(0x2C, 0xA1, 0xCF));
                PdfWriter.GetInstance(_document, new FileStream(_PdfPathName, FileMode.Create));
            }
            catch (Exception e)
            {
                MessageDlg.Show("File I/O error " + e.Message);
                return;
            }

            _document.Open();
            await Assemble();
            _document.Close();

            Process.Start(_PdfPathName);
        }

        private async Task Assemble()
        {
            //Cover page 
            PdfTools.EmptyParagraph(4, _document);
            PdfTools.CenteredParagraph("Tohoku University Discussion Support System", PdfTools.HeaderFont, _document);
            PdfTools.EmptyParagraph(6, _document);
            PdfTools.LeftParagraph("  Discussion report", PdfTools.SubHeaderFont, _document);

            //Title page
            PdfTools.SwitchBgColor(_document, BaseColor.WHITE);
            _document.NewPage();

            PdfTools.LeftParagraph("Basic information", PdfTools.SubHeaderFont2, _document);
            _document.Add(TitleTable());

            //session participants 
            if (_person.Session != null)
            {
                PdfTools.EmptyLine(_document);
                PdfTools.SingleLine("Participants", _document);
                _document.Add(ParticipantsTable());

                PdfTools.EmptyLine(_document);
            }

            //discussion background          
            await DiscussionBackground();

            //InsertLine();

            ////subject
            //document.Add(makeHeader(discussion.Subject, true));

            //InsertLine();

            ////background
            //Paragraph p = new Paragraph();
            /////p.Add(TextRefsAggregater.PlainifyRichText(discussion.Background));
            //p.Add(new Chunk("\n"));
            //document.Add(p);

            //InsertLine();

            ////sources
            //backgroundSources();
            //document.NewPage();

            ////agreement blocks
            //List<ArgPoint> agreed = new List<ArgPoint>();
            //List<ArgPoint> disagreed = new List<ArgPoint>();
            //List<ArgPoint> unsolved = new List<ArgPoint>();
            //addBlockOfAgreement("Agreed", agreed);
            //addBlockOfAgreement("Disagreed", disagreed);
            //addBlockOfAgreement("Unsolved", unsolved);
        }

        private PdfPTable TitleTable()
        {
            var aTable = PdfTools.TableDefaults(new PdfPTable(2));

            aTable.AddCell(PdfTools.ColoredTxtCell("Discussion"));
            aTable.AddCell(PdfTools.ColoredTxtCell(_discussion.Subject));

            aTable.AddCell(PdfTools.ColoredTxtCell("Topic"));
            aTable.AddCell(PdfTools.ColoredTxtCell(_topic.Name));

            //session
            if (_person.Session != null)
            {
                aTable.AddCell(PdfTools.ColoredTxtCell("Session"));
                aTable.AddCell(PdfTools.ColoredTxtCell(_person.Session.Name));

                aTable.AddCell(PdfTools.ColoredTxtCell("Date and time"));
                aTable.AddCell(PdfTools.ColoredTxtCell(_person.Session.EstimatedDateTime.ToString()));
            }
            else
            {
                PdfTools.CenteredParagraph("Session: no session for " + _person.Name, PdfTools.SubHeaderFont2, _document);
            }

            aTable.AddCell(PdfTools.ColoredTxtCell("Total time (one topic)"));
            var cumulativeDuration = TimeSpan.FromSeconds(_topic.CumulativeDuration).ToString();
            aTable.AddCell(PdfTools.ColoredTxtCell(cumulativeDuration));

            return aTable;
        }

        private PdfPTable ParticipantsTable()
        {
            //var aTable = PdfTools.TableDefaults(new PdfPTable(2));
            //var participants = DaoUtils.Participants(_topic, _person.Session);

            //foreach (var pers in participants)
            //{
            //    aTable.AddCell(PdfTools.ColoredTxtCell(pers.Name, new BaseColor(pers.Color)));
            //}

            //return aTable;
            return null;
        }

        private Task DiscussionBackground()
        {
            var bgUrl = string.Format("http://{0}/discsvc/bgpage?id={1}", ConfigManager.ServiceServer, _discussion.Id);

            var tcs = new TaskCompletionSource<int>();
            new WebScreenshoter(
                bgUrl,
                pathName =>
                    {
                        var img = iTextSharp.text.Image.GetInstance(pathName);

                        _document.SetPageSize(new iTextSharp.text.Rectangle(0, 0, img.Width, img.Height));
                        _document.SetMargins(0, -20, -15, 0);
                        img.Alignment = Element.ALIGN_MIDDLE;
                        img.IndentationLeft = 0;
                        img.BorderWidth = 0;
                        img.IndentationRight = 0;
                        //img.Right = 0;
                        //img.SetDpi(300, 300);// = PageSize.A4.Width + 30;
                        //img.SetAbsolutePosition(PageSize.A4.Width, img.AbsoluteY);         
                        //img.SetAbsolutePosition(0,img.AbsoluteY);
                        //img.ScaleToFit(PageSize.A4.Width, img.Height);
                        _document.Add(img);

                        PdfTools.LeftParagraph("Basic information 4", PdfTools.SubHeaderFont2, _document);
                        PdfTools.LeftParagraph("Basic information 5", PdfTools.SubHeaderFont2, _document);

                        tcs.SetResult(0); //ok, done
                    },
                -100
                );
            return tcs.Task;
        }
    }
}