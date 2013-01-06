//using System;
//using System.Collections.Generic;
//using System.Drawing;
//using Discussions;
//using PDFjet.NET;
//using Color = PDFjet.NET.Color;
//using Font = PDFjet.NET.Font;

//namespace Reporter.pdf
//{
//    public static class PdfTools3
//    {
//        public const int CoverPageBg = (0xFF << 24) | (0x2C << 16) | (0xA1 << 8) | 0xCF;

//        //standard top/left "margins" for normal pages
//        public const int TOP_MARGIN = 50;
//        public const int LEFT_MARGIN = 50;

//        public const int DEFAULT_FONT_SIZE = 14;

//        public static TextLine WithColor(this TextLine line, int color)
//        {
//            line.SetColor(color);
//            return line;
//        }

//        public static Paragraph EmptyPara(Font font, int numLines)
//        {
//            var p = new Paragraph();
//            for (int i = 0; i < numLines; i++)
//                p.Add(new TextLine(font, ""));
//            return p;
//        }

//        public static Paragraph Centered(this Paragraph para)
//        {
//            para.SetAlignment(Align.CENTER);
//            return para;
//        }

//        public static TextColumn AdjustCol(Font font, float width, float height, float lineSpacing = 1)
//        {
//            var column = new TextColumn(font);
//            column.SetLineBetweenParagraphs(true);
//            column.SetLineSpacing(lineSpacing);
//            column.SetSize(width, height);
//            return column;
//        }

//        public static Cell WithColor(this Cell cell, int color)
//        {
//            if (color==-1)
//                cell.SetBgColor(Color.lightgrey);
//            else
//                cell.SetBgColor(color);

//            return cell;
//        }

//        public static TextLine SectionHeader(this TextLine t, string text)
//        {
//            t.SetText(text);
//            t.GetFont().SetSize(20);
//            t.SetColor((0x49 << 16) | (0x89 << 8) | 0xFF);
//            return t;
//        }

//        public static TextLine MakeSectionHeader(string Text, Font mainFont)
//        {
//            return new TextLine(mainFont).SectionHeader(Text).TopLeft();
//        }

//        public static TextLine TopLeft(this TextLine t)
//        {
//            t.SetPosition(LEFT_MARGIN, TOP_MARGIN);
//            return t;
//        }

//        public static Table TableDefaults(this Table table, float pageWidth, int numColumns)
//        {
//            table.SetCellBordersWidth(1);
//            table.SetCellBordersColor(Color.white);

//            var cw = ContentWidth(pageWidth);
//            for (int i = 0; i < numColumns; i++)
//                table.SetColumnWidth(i, cw/numColumns);

//            return table;
//        }

//        public static Cell ToCell(this string text, Font font, int color = -1)
//        {
//            return new Cell(font, text).WithColor(color);
//        }

//        public static float ContentWidth(float pageWidth)
//        {
//            return pageWidth - 2*LEFT_MARGIN;
//        }

//        /// <summary>
//        /// Slices tall image into a set of slices. Each slice has width equal to width of original and height not greater 
//        /// than height of 300 DPI A4 page
//        /// </summary>
//        /// <param name="pathName">path name to original</param> 
//        /// <returns>enumerable with pathnames to slice images</returns>
//        public static IEnumerable<string> SliceImage(string pathName)
//        {
//            var sizeTemplate = new System.Drawing.Bitmap(pathName);
//            var maxImgHeight = PdfTools2.PAGE_HEIGHT*sizeTemplate.Width/PdfTools2.PAGE_WIDTH;
//            if (sizeTemplate.Height < maxImgHeight)
//                return new string[] {pathName};

//            var res = new List<string>();

//            var pagesRequired = (int) Math.Ceiling((double) sizeTemplate.Height/maxImgHeight);

//            //height of original not yet sliced 
//            var heightRemains = sizeTemplate.Height;

//            // index of pixel row in original image that will become top row of slice image
//            var nextYToHandle = 0;
//            for (int page = 0; page < pagesRequired; page++)
//            {
//                var slicePathName = Utils2.RandomFilePath(".png");
//                var sliceHeight = Math.Min(heightRemains, maxImgHeight);
//                var sliceWidth = sizeTemplate.Width;
//                using (var sliceImg = new Bitmap(sliceWidth, sliceHeight))
//                {
//                    using (Graphics graphics = Graphics.FromImage(sliceImg))
//                    {
//                        graphics.DrawImage(sizeTemplate,
//                                           new Rectangle(0, 0, sliceWidth, sliceHeight), //dst rect
//                                           new Rectangle(0, nextYToHandle, sliceWidth, sliceHeight), //src rect
//                                           GraphicsUnit.Pixel);
//                    }
//                    sliceImg.Save(slicePathName);
//                }
//                res.Add(slicePathName);

//                heightRemains -= sliceHeight;
//                nextYToHandle += sliceHeight;
//            }

//            sizeTemplate.Dispose();

//            return res;
//        }
//    }
//}