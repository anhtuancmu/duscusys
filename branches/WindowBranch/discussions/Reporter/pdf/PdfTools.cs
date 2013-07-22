using System.Linq;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Text;
using iTextSharp.text;

namespace Reporter.pdf
{
    internal class PdfTools
    {
        public static Font HeaderFont = FontFactory.GetFont("Segoe UI Light", 30, Font.NORMAL, BaseColor.WHITE);
        public static Font SubHeaderFont = FontFactory.GetFont("Segoe UI Light", 20, Font.NORMAL, BaseColor.WHITE);
        public static Font SubHeaderFont2 = FontFactory.GetFont("Segoe UI Light", 20);
        public static Font BlockHeaderFont = FontFactory.GetFont(FontFactory.HELVETICA, 14, Font.NORMAL);
        public static Font NormalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12, Font.NORMAL);

        public static void CenteredParagraph(string str, Font font, Document document)
        {
            Paragraph p = new Paragraph(str, font);
            p.Alignment = 1;
            document.Add(p);
        }

        public static void LeftParagraph(string str, Font font, Document document)
        {
            Paragraph p = new Paragraph(str, font);
            p.SpacingAfter = 7f;
            document.Add(p);
        }

        public static void SingleLine(string str, Document document)
        {
            //document.Add(new Chunk(str));
            var p = new Paragraph(str);
            p.SpacingAfter = 7f;
            document.Add(p);
        }

        public static void EmptyParagraph(int nLines, Document document)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < nLines; i++)
                sb.AppendLine();

            Paragraph p = new Paragraph(sb.ToString(), HeaderFont);
            p.Alignment = 1;

            document.Add(p);
        }

        public static void EmptyLine(Document document)
        {
            Paragraph p = new Paragraph();
            p.Add(new Chunk("\n"));
            document.Add(p);
        }

        public static void SwitchBgColor(Document document, BaseColor clr)
        {
            iTextSharp.text.Rectangle pageSize = new iTextSharp.text.Rectangle(PageSize.A4);
            pageSize.BackgroundColor = clr;
            document.SetPageSize(pageSize);
        }

        public static PdfPTable TableDefaults(PdfPTable table)
        {
            table.WidthPercentage = 100.0f;
            table.HorizontalAlignment = Element.ALIGN_LEFT;
            return table;
        }

        public static PdfPCell ColoredTxtCell(string content, BaseColor color = null)
        {
            PdfPCell c = new PdfPCell(new Phrase(content));
            //     c.Border = PdfPCell.BOTTOM_BORDER | PdfPCell.TOP_BORDER | PdfPCell.LEFT_BORDER | PdfPCell.RIGHT_BORDER;
            c.Padding = 4;
            c.BorderWidth = 1;
            c.BorderColor = new BaseColor(System.Drawing.Color.Transparent);
            _getColoredCell(c, color);
            return c;
        }

        public static PdfPCell TxtCell(string content)
        {
            PdfPCell c = new PdfPCell(new Phrase(content));
            c.Padding = 4;
            c.BorderWidth = 1;
            c.BorderColor = new BaseColor(System.Drawing.Color.Transparent);
            return c;
        }

        public static PdfPCell ColoredImgCell(Image img, BaseColor color = null)
        {
            PdfPCell c = new PdfPCell(img);
            c.Padding = 4;
            c.BorderWidth = 1;
            c.BorderColor = new BaseColor(System.Drawing.Color.Transparent);
            _getColoredCell(c, color);
            return c;
        }

        public static PdfPCell _getColoredCell(PdfPCell c, BaseColor color = null)
        {
            if (color == null)
                c.BackgroundColor = new BaseColor(230, 230, 240);
            else
                c.BackgroundColor = color;

            return c;
        }
    }
}