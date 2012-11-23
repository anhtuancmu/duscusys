using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;

namespace Reporter.pdf
{
    class PdfTools
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

        public static PdfPCell ColoredTxtCell(string content, BaseColor color = null)
        {
            PdfPCell c = new PdfPCell(new Phrase(content));
            _getColoredCell(c, color);
            return c;
        }
        public static PdfPCell ColoredImgCell(Image img, BaseColor color = null)
        {
            PdfPCell c = new PdfPCell(img);
            _getColoredCell(c, color);
            return c;
        }
        public static PdfPCell _getColoredCell(PdfPCell c, BaseColor color = null)
        {
            if (color == null)
                c.BackgroundColor = new BaseColor(170, 170, 170);
            else
                c.BackgroundColor = color;

            return c;
        }
    }
}
