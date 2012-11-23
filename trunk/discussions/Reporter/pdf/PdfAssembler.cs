using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using Discussions.DbModel;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;

namespace Reporter.pdf
{
    public class PdfAssembler
    {
        Discussion _discussion;
        Topic _topic;
        string _PdfPathName;
        Document _document;
        Person _person;

        public PdfAssembler(Discussion discussion, Topic topic, Person person, string PdfPathName)
        {
            this._discussion = discussion;
            this._topic = topic;
            this._PdfPathName = PdfPathName;
            this._person = person;
         
            _document = new Document();
            try
            {
                PdfTools.SwitchBgColor(_document, new BaseColor(0x2C, 0xA1, 0xCF));
                PdfWriter.GetInstance(_document, new FileStream(PdfPathName, FileMode.Create));
            }
            catch (Exception e)
            {
                MessageBox.Show("File I/O error " + e.Message);
                return;
            }

            _document.Open();
            Assemble();

            UrlToPdf(@"http://localhost/discsvc/bgpage?id=1");
          
            _document.Close();

            Process.Start(PdfPathName);
        }

        void Assemble()
        {
            //Cover page 
            PdfTools.EmptyParagraph(4, _document);
            PdfTools.CenteredParagraph("Tohoku University Discussion Support System", PdfTools.HeaderFont, _document);
            PdfTools.EmptyParagraph(6, _document);
            PdfTools.LeftParagraph("  Discussion report", PdfTools.SubHeaderFont, _document);

            //Title page
            PdfTools.SwitchBgColor(_document, BaseColor.WHITE);
            _document.NewPage();

            _document.Add(TitleTable());

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

        PdfPTable TitleTable()
        {
            var aTable = new PdfPTable(3);

            aTable.AddCell(PdfTools.ColoredTxtCell("Discussion"));
            aTable.AddCell(PdfTools.ColoredTxtCell("Topic"));

            aTable.AddCell(PdfTools.ColoredTxtCell(_discussion.Subject));
            aTable.AddCell(PdfTools.ColoredTxtCell(_topic.Name));

            //session
            if (_person.Session != null)
            {
                aTable.AddCell("Session: " + _person.Session.Name);
                aTable.AddCell(_person.Session.EstimatedDateTime.ToString() + _person.Session.Name);

                //session participants 
                aTable.AddCell("Session: " + _person.Session.Name);

                PdfTools.EmptyLine(_document);

                _document.Add(SessionParticipants());
                
                PdfTools.EmptyLine(_document);
            }
            else
            {
                PdfTools.CenteredParagraph("Session: no session for " + _person.Name, PdfTools.SubHeaderFont2, _document);
            }

            return aTable;
        }

        PdfPTable SessionParticipants()
        {                                                            
            var aTable = new PdfPTable(2);            
            var participants = DaoUtils.Participants(_topic, _person.Session);

            foreach(var pers in participants)
            {
                aTable.AddCell(PdfTools.ColoredTxtCell(pers.Name, new BaseColor(pers.Color)));
            }

            return aTable;
        }

        private void UrlToPdf(string url)
        {
            using (var wc = new WebClient())
            {
                HtmlToPDF(wc.DownloadString(url));
            }
        }

        private void HtmlToPDF(string html)
        {
            TextReader reader = new StringReader(html);

            // step 1: creation of a document-object
            Document document = new Document(PageSize.A4, 30, 30, 30, 30);

            // step 2:
            // we create a writer that listens to the document
            // and directs a XML-stream to a file
            PdfWriter writer = PdfWriter.GetInstance(document, new FileStream(@"C:\projects\TDS\htmlPdf.pdf", FileMode.Create));

            // step 3: we create a worker parse the document
            HTMLWorker worker = new HTMLWorker(document);

            // step 4: we open document and start the worker on the document
            document.Open();
            worker.StartDocument();

            // step 5: parse the html into the document
            try
            {
                worker.Parse(reader);
            }
            catch (Exception e)
            {

            }

            // step 6: close the document and the worker
            worker.EndDocument();
            worker.Close();
            document.Close();
        }
    }
}
