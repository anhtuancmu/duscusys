using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using MigraDoc.DocumentObjectModel;
using MigraDoc.DocumentObjectModel.Shapes;
using MigraDoc.DocumentObjectModel.Tables;
using MigraDoc.Rendering;
using Discussions;
using Discussions.DbModel;

namespace Reporter.pdf
{
    static class PdfTools2
    {
        public const int PAGE_HEIGHT = 842;
        public const int PAGE_WIDTH  = 595;

        public static Paragraph LeftParaIndent(this Paragraph p, int points)
        {
            p.Format.LeftIndent = Unit.FromPoint(points);
            return p;
        }

        public static Table TableDefaults(this Table t, byte r = 0xFF, byte g = 0xFF, byte b = 0xFF)
        {
            t.Borders.Color = Colors.Transparent;
            t.Borders.Width = Unit.FromPoint(1);
            if (r == 0xFF && g == 0xFF && b == 0xFF)
                t.Shading.Color = new MigraDoc.DocumentObjectModel.Color(230, 230, 240);
            else
                t.Shading.Color = new MigraDoc.DocumentObjectModel.Color(r,g,b);
            return t;
        }

        public static Table TableDefaults(this Table t, int rgb)
        {
            t.Borders.Color = Colors.Transparent;
            t.Borders.Width = Unit.FromPoint(1);
            t.Shading.Color = new MigraDoc.DocumentObjectModel.Color((uint)rgb);
            return t;
        }

        public static Paragraph SectionHeader(this Paragraph p)
        {
            p.Format.Font.Size = Unit.FromPoint(20);
            p.Format.Font.Name = "Segoe UI";
            p.Format.Font.Color = new MigraDoc.DocumentObjectModel.Color(0x49, 0x89, 0xFF);
            p.Format.Font.Bold = true;
            return p;
        }

        /// <summary>
        /// Slices tall image into a set of slices. Each slice has width equal to width of original and height not greater 
        /// than height of 300 DPI A4 page
        /// </summary>
        /// <param name="pathName">path name to original</param> 
        /// <returns>enumerable with pathnames to slice images</returns>
        public static IEnumerable<string> SliceImage(string pathName)
        {
            var sizeTemplate = new System.Drawing.Bitmap(pathName);     
            var maxImgHeight = PAGE_HEIGHT * sizeTemplate.Width / PAGE_WIDTH;
            if (sizeTemplate.Height < maxImgHeight)
                return new string[] { pathName };

            var res = new List<string>();

            var pagesRequired = (int)Math.Ceiling((double)sizeTemplate.Height / maxImgHeight);

            //height of original not yet sliced 
            var heightRemains = sizeTemplate.Height;

            // index of pixel row in original image that will become top row of slice image
            var nextYToHandle = 0;
            for (int page = 0; page < pagesRequired; page++)
            {
                var slicePathName = Utils2.RandomFilePath(".png");
                var sliceHeight = Math.Min(heightRemains, maxImgHeight);
                var sliceWidth = sizeTemplate.Width;
                using (var sliceImg = new Bitmap(sliceWidth, sliceHeight))
                {
                    using (Graphics graphics = Graphics.FromImage(sliceImg))
                    {
                        graphics.DrawImage(sizeTemplate,
                                            new Rectangle(0, 0, sliceWidth, sliceHeight),//dst rect
                                            new Rectangle(0, nextYToHandle, sliceWidth, sliceHeight),//src rect
                                            GraphicsUnit.Pixel);
                    }
                    sliceImg.Save(slicePathName);
                }
                res.Add(slicePathName);

                heightRemains -= sliceHeight;
                nextYToHandle += sliceHeight;
            }

            sizeTemplate.Dispose();

            return res;
        }

        public static void AddPageBg(Section s, Document doc, byte r, byte g, byte b) 
        {
            var tf = s.AddTextFrame();
            tf.FillFormat.Color = new MigraDoc.DocumentObjectModel.Color(r, g, b);
            tf.Width = doc.DefaultPageSetup.PageWidth.Point;
            tf.Height = doc.DefaultPageSetup.PageHeight.Point;
            tf.RelativeHorizontal = RelativeHorizontal.Page;
            tf.RelativeVertical = RelativeVertical.Page;
        }

        public static Paragraph WhiteFontPara(this Paragraph p)
        {
            p.Format.Font.Color = Colors.White;
            return p;
        }

        public static Paragraph BlackFontPara(this Paragraph p)
        {
            p.Format.Font.Color = Colors.Black;
            return p;
        }

        public static Paragraph ParaFontColor(this Paragraph p, int rgb)
        {
            p.Format.Font.Color = new MigraDoc.DocumentObjectModel.Color((uint)rgb);
            return p;
        }

        public static MigraDoc.DocumentObjectModel.Color PersonToColor(this Person p)
        {
            return new MigraDoc.DocumentObjectModel.Color((uint)p.Color);
        }

        public static Paragraph SubsectionStyle(this Paragraph p2)
        {
            p2.Format.Alignment = ParagraphAlignment.Center;
            p2.Format.Font.Color = Colors.White;
            p2.Format.Font.Size = 30;
            p2.Format.SpaceBefore = Unit.FromPoint(100);
            return p2;
        }

        public static Paragraph AddBold(this Paragraph p2, string text)
        {            
            p2.Format.Font.Name = "Segoe UI";
            p2.AddFormattedText(text, TextFormat.Bold);
            p2.Format.Font.Color = new MigraDoc.DocumentObjectModel.Color(0x46, 0x42, 0x60);
            return p2;
        }

        public static void AlignWithTable(this Paragraph p)
        {
            p.Format.LeftIndent = Unit.FromPoint(-3); //align with table 
            p.Format.RightIndent = Unit.FromPoint(5);
            p.Format.SpaceBefore = Unit.FromPoint(1);
        }
    }
}
