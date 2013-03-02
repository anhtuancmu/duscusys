using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.IO;
using Discussions.model;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Discussions.DbModel;
using System.Windows.Forms;

namespace Discussions
{
    public class ReportGenerator
    {
        private Font HeaderFont = FontFactory.GetFont(FontFactory.HELVETICA, 16, Font.NORMAL);
        private Font BoldHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA, 16, Font.BOLD);
        private Font BlockHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA, 14, Font.NORMAL);
        private Font NormalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, Font.NORMAL);

        private Discussion discussion;
        private string PdfPathName;
        private Document document;

        public void Generate(Discussion discussion, string PdfPathName)
        {
            this.discussion = discussion;
            this.PdfPathName = PdfPathName;

            document = new Document();

            // we create a writer that listens to the document
            // and directs a PDF-stream to a file
            try
            {
                PdfWriter.GetInstance(document, new FileStream(PdfPathName, FileMode.Create));
            }
            catch (Exception e)
            {
                MessageDlg.Show("File I/O error " + e.Message);
                return;
            }

            document.Open();
            Assemble();
            document.Close();

            Process.Start(PdfPathName);
        }

        private void Assemble()
        {
            //headers
            document.Add(makeHeader("TOHOKU UNIVERSITY DISCUSSION SUPPORT SYSTEM"));
            document.Add(makeHeader("Discussion report"));

            InsertLine();

            //subject
            document.Add(makeHeader(discussion.Subject, true));

            InsertLine();

            //background
            Paragraph p = new Paragraph();
            ///p.Add(TextRefsAggregater.PlainifyRichText(discussion.Background));
            p.Add(new Chunk("\n"));
            document.Add(p);

            InsertLine();

            //sources
            backgroundSources();
            document.NewPage();

            //agreement blocks
            List<ArgPoint> agreed = new List<ArgPoint>();
            List<ArgPoint> disagreed = new List<ArgPoint>();
            List<ArgPoint> unsolved = new List<ArgPoint>();
            addBlockOfAgreement("Agreed", agreed);
            addBlockOfAgreement("Disagreed", disagreed);
            addBlockOfAgreement("Unsolved", unsolved);
        }

        private void backgroundSources()
        {
            Paragraph p = new Paragraph();
            p.Add("Discussion sources:");
            document.Add(p);
            foreach (Attachment at in discussion.Attachment)
            {
                BitmapSource src = MiniAttachmentManager.GetAttachmentBitmap2(at);
                System.Drawing.Image drawingImg = BitmapSourceToBitmap(src);
                iTextSharp.text.Image itextImg = iTextSharp.text.Image.GetInstance(drawingImg, BaseColor.WHITE);
                itextImg.ScaleToFit(150, 200);
                document.Add(itextImg);

                switch ((AttachmentFormat) at.Format)
                {
                    case AttachmentFormat.Pdf:
                        break;
                    case AttachmentFormat.Bmp:
                    case AttachmentFormat.Jpg:
                    case AttachmentFormat.Png:
                        break;
                    case AttachmentFormat.Youtube:
                        p = new Paragraph();
                        p.Add("Youtube: " + at.VideoLinkURL);
                        document.Add(p);
                        break;
                    default:
                        throw new NotSupportedException();
                }
            }
        }

        private void InsertLine()
        {
            Paragraph p = new Paragraph();
            p.Add(new Chunk("\n"));
            document.Add(p);
        }

        private Paragraph makeHeader(string str, bool bold = false)
        {
            Paragraph p = new Paragraph(str, bold ? BoldHeaderFont : HeaderFont);
            p.Alignment = 1;
            return p;
        }

        private void addBlockOfAgreement(string blockName, List<ArgPoint> items)
        {
            Paragraph p = new Paragraph();
            p.Alignment = 1;
            p.Add(new Chunk(blockName, BlockHeaderFont));
            p.Add(new Chunk("\n"));
            p.Add(new Chunk("\n"));
            document.Add(p);

            p = new Paragraph();
            foreach (ArgPoint pt in items)
            {
                p.Add(addArgPoint(pt));
                p.Add(new Chunk("\n"));
            }
            document.Add(p);
        }

        private PdfPCell getColoredTxtCell(string content, BaseColor color = null)
        {
            PdfPCell c = new PdfPCell(new Phrase(content));
            _getColoredCell(c, color);
            return c;
        }

        private PdfPCell getColoredImgCell(Image img, BaseColor color = null)
        {
            PdfPCell c = new PdfPCell(img);
            _getColoredCell(c, color);
            return c;
        }

        private PdfPCell _getColoredCell(PdfPCell c, BaseColor color = null)
        {
            if (color == null)
                c.BackgroundColor = new BaseColor(170, 170, 170);
            else
                c.BackgroundColor = color;

            return c;
        }

        private PdfPTable addArgPoint(ArgPoint pt)
        {
            PdfPTable aTable = new PdfPTable(3);

            BaseColor clr = DiscussionColors.GetSideColor(pt.SideCode);

            PdfPCell c = getColoredTxtCell("Point:  " + string.Format("{0}", pt.Point) + "\n" +
                                           "Description:  " + string.Format("{0}", pt.Point) + "\n" +
                                           "Author(Name/Email):  " +
                                           string.Format("{0}/{1}", pt.Person.Name,
                                                         pt.Person.Email),
                                           clr);
            c.Colspan = 3;

            aTable.AddCell(c);

            foreach (Comment comment in pt.Comment)
            {
                string comm = string.Format("{0} ({1},{2})\n",
                                            comment.Text,
                                            pt.Person.Name,
                                            pt.Person.Email);
                aTable.AddCell(getColoredTxtCell("Comment", clr));
                aTable.AddCell(getColoredTxtCell(comm, clr));
                aTable.AddCell(getColoredTxtCell(string.Format("{0},{1}",
                                                               comment.Person.Name,
                                                               comment.Person.Email), clr));
            }

            foreach (Attachment at in pt.Attachment)
            {
                aTable.AddCell(getColoredTxtCell("Attachment", clr));

                BitmapSource src = MiniAttachmentManager.GetAttachmentBitmap2(at);
                System.Drawing.Image drawingImg = BitmapSourceToBitmap(src);
                iTextSharp.text.Image itextImg = iTextSharp.text.Image.GetInstance(drawingImg, BaseColor.WHITE);
                itextImg.ScaleToFit(150, 200);
                aTable.AddCell(getColoredImgCell(itextImg));

                aTable.AddCell(getColoredTxtCell("", clr));
            }

            return aTable;
        }

        public static System.Drawing.Bitmap BitmapSourceToBitmap(BitmapSource srs)
        {
            System.Drawing.Bitmap btm = null;
            int width = srs.PixelWidth;
            int height = srs.PixelHeight;
            int stride = width*((srs.Format.BitsPerPixel + 7)/8);
            byte[] bits = new byte[height*stride];
            srs.CopyPixels(bits, stride, 0);
            unsafe
            {
                fixed (byte* pB = bits)
                {
                    IntPtr ptr = new IntPtr(pB);
                    btm = new System.Drawing.Bitmap(
                        width,
                        height,
                        stride,
                        System.Drawing.Imaging.PixelFormat.Format32bppPArgb,
                        ptr);
                }
            }
            return btm;
        }
    }
}