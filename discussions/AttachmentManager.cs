using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Reflection;
using System.IO;
using System.Windows.Media.Imaging;
using System.Windows.Controls;
using System.Windows.Media;
using System.Net;
using System.Drawing;
using System.Drawing.Imaging;
using CloudStorage;
using CloudStorage.Model;
using Discussions.model;
using Discussions.DbModel;
using System.Diagnostics;
using Discussions.YouViewer;
using System.Runtime.InteropServices;
using Discussions.pdf_reader;
using Discussions.rt;

namespace Discussions
{
    public class AttachmentManager
    {
        private static BitmapImage _pdfIcon = null;

        public static BitmapImage PdfIcon
        {
            get
            {
                if (_pdfIcon == null)
                {
                    _pdfIcon = new BitmapImage(new Uri("pack://application:,,,/Resources/PDF_ICON.png"));
                }

                return _pdfIcon;
            }
        }

        private static BitmapImage _excelIcon = null;

        public static BitmapImage ExcelIcon
        {
            get
            {
                if (_excelIcon == null)
                {
                    _excelIcon = new BitmapImage(new Uri("pack://application:,,,/Resources/ExcelIcon.png"));
                }

                return _excelIcon;
            }
        }

        private static BitmapImage _wordIcon = null;

        public static BitmapImage WordIcon
        {
            get
            {
                if (_wordIcon == null)
                {
                    _wordIcon = new BitmapImage(new Uri("pack://application:,,,/Resources/WordIcon.png"));
                }

                return _wordIcon;
            }
        }

        private static BitmapImage _powerPointIcon = null;

        public static BitmapImage PowerPointIcon
        {
            get
            {
                if (_powerPointIcon == null)
                {
                    _powerPointIcon = new BitmapImage(new Uri("pack://application:,,,/Resources/PowerPointIcon.png"));
                }

                return _powerPointIcon;
            }
        }


        //recognizes format of attachment by URL's structure
        public static AttachCmd DeriveCmdFromUrl(string Url)
        {
            Url = Url.ToLower();
            if (Url.StartsWith("http://www.youtube.com"))
                return AttachCmd.ATTACH_YOUTUBE;
            else if (Url.EndsWith(".pdf"))
                return AttachCmd.ATTACH_PDF_FROM_URL;
            else
                return AttachCmd.ATTACH_IMAGE_URL;
        }

        public static bool IsGraphicFormat(Attachment a)
        {
            return IsGraphicFormat((AttachmentFormat)a.Format);
        }

        public static bool IsGraphicFormat(AttachmentFormat af)
        {
            return
                af == AttachmentFormat.Bmp ||
                af == AttachmentFormat.Jpg ||
                af == AttachmentFormat.Png ||
                af == AttachmentFormat.PngScreenshot;
        }

        public static BitmapSource GetAttachmentBitmap3(Attachment a)
        {
            if (a == null)
                return null;

            switch (a.Format)
            {
                case (int) AttachmentFormat.Pdf:
                    if (a.Thumb != null)
                        return LoadImageFromBlob(a.Thumb);
                    else
                        return PdfIcon;
                case (int) AttachmentFormat.ExcelDocSet:
                    return ExcelIcon;
                case (int) AttachmentFormat.WordDocSet:
                    return WordIcon;
                case (int) AttachmentFormat.PowerPointDocSet:
                    return PowerPointIcon;
                case (int) AttachmentFormat.Jpg:
                case (int) AttachmentFormat.Png:
                case (int) AttachmentFormat.PngScreenshot:
                case (int) AttachmentFormat.Bmp:
                    return LoadImageFromBlob(a.MediaData.Data);
                case (int) AttachmentFormat.Youtube:
                    if (a.Thumb != null)
                        return LoadImageFromBlob(a.Thumb);
                    else
                        return GetYoutubeThumb(a.VideoThumbURL);
            }

            return null;
        }

        private static BitmapSource GetYoutubeThumb(string thumbUrl)
        {
            string tmpFile = DownloadImageFromURL(thumbUrl);
            if (tmpFile == null)
                return null;
            else
                return fileToBmpSrc(tmpFile);
        }

        //uses provided URL
        public static ImageSource ProcessAttachCmd(ArgPoint Point, string Url, ref Attachment a)
        {
            var cmd = DeriveCmdFromUrl(Url);
            return ProcessCommand2(Point, cmd, Url, ref a);
        }

        //asks for URL
        public static ImageSource ProcessAttachCmd(ArgPoint Point, AttachCmd cmd, ref Attachment a)
        {
            return ProcessCommand2(Point, cmd, null, ref a);
        }

        private static ImageSource ProcessCommand2(ArgPoint Point, AttachCmd cmd, string Url, ref Attachment a)
        {
            a = null;

            try
            {
                switch (cmd)
                {
                    case AttachCmd.ATTACH_IMG_OR_PDF:
                        a = AttachmentManager.AttachLocalFile(Point);
                        if (a != null)
                            return GetAttachmentBitmap3(a);
                        break;
                    case AttachCmd.ATTACH_PDF:
                        a = AttachmentManager.AttachPDF(Point);
                        if (a != null)
                            return GetAttachmentBitmap3(a);
                        break;
                    case AttachCmd.ATTACH_PDF_FROM_URL:
                        a = AttachmentManager.AttachPdfFromURL(Point, Url);
                        if (a != null)
                            return GetAttachmentBitmap3(a);
                        break;
                    case AttachCmd.ATTACH_IMAGE:
                        a = AttachmentManager.AttachPicture(Point);
                        if (a != null)
                            return GetAttachmentBitmap3(a);
                        break;
                    case AttachCmd.ATTACH_IMAGE_URL:
                        a = AttachmentManager.AttachFromURL(Point, Url);
                        if (a != null)
                            return GetAttachmentBitmap3(a);
                        break;
                    case AttachCmd.ATTACH_YOUTUBE:
                        a = AttachmentManager.AttachFromYoutube(Point, Url);
                        if (a != null)
                            return new BitmapImage(); //returning stub for external error-checking
                        break;
                }
            }
            catch (Exception)
            {
                MessageDlg.Show(
                    "Cannot process attachment. If it's link, check it's correct. If it's file, ensure program has permissions to access it",
                    "Error");
                return null;
            }

            return null;
        }

        public static BitmapImage LoadImageFromBlob(byte[] blob)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = new MemoryStream(blob);
            bmp.EndInit();
            return bmp;
        }

        public static Attachment AttachPicture(ArgPoint Point)
        {
            return AttachAsBlob("Graphics |*.jpg;*.jpeg;*.bmp;*.png", AttachmentFormat.None, true, Point);
        }

        public static Attachment AttachPDF(ArgPoint Point)
        {
            return AttachAsBlob("PDF |*.pdf", AttachmentFormat.Pdf, true, Point);
        }

        public static Attachment AttachLocalFile(ArgPoint Point)
        {
            var filterSb = new StringBuilder();
            filterSb.Append("Local file|");
            filterSb.Append("*.jpg;*.jpeg;*.bmp;*.png;");
            filterSb.Append("*.docx;*.docm;*.dotx;*.dotm;*.doc;*.rtf;*.odt;");
            filterSb.Append("*.xlsx;*.xlsm;*.xlsb;*.xltx;*.xltm;*.xls;*.xlt");
            filterSb.Append("*.pptx;*.ppt;*.pptm;*.ppsx;*.pps;*.potx;*.pot*;*.potm;*.odp");

            return AttachAsBlob(filterSb.ToString(),
                                AttachmentFormat.None,
                                true,
                                Point);
        }

        //if URL==null, shows URL input dialog. else uses provided URL, no dialog
        private static Attachment AttachFromURL(ArgPoint Point, string Url)
        {
            string UrlToUse = Url;
            if (UrlToUse == null)
            {
                InpDialog dlg = new InpDialog();
                dlg.ShowDialog();
                UrlToUse = dlg.Answer;

                if (UrlToUse == null || !UrlToUse.StartsWith("http://"))
                    return null;
            }

            string tmpFile = DownloadImageFromURL(UrlToUse);
            if (tmpFile == null)
                return null;

            Attachment res = new Attachment();
            res.Name = UrlToUse;
            res.Format = (int) AttachmentFormat.Jpg; //all downloads are jpg 
            res.MediaData = DaoUtils.CreateMediaData(ImgFileToBytes(tmpFile));
            res.Title = "";
            res.Link = Url;

            if (Point != null)
                Point.Attachment.Add(res);
            //PublicBoardCtx.Get().SaveChanges();

            return res;
        }

        //if URL==null, shows URL input dialog. else uses provided URL, no dialog
        private static Attachment AttachPdfFromURL(ArgPoint Point, string Url)
        {
            string UrlToUse = Url;
            if (UrlToUse == null)
            {
                InpDialog dlg = new InpDialog();
                dlg.ShowDialog();
                UrlToUse = dlg.Answer;

                UrlToUse = UrlToUse.ToLower();
                if (UrlToUse == null || !UrlToUse.StartsWith("http://") || UrlToUse.EndsWith(".pdf"))
                    return null;
            }

            string tmpFile = DownloadPdfFromUrl(UrlToUse);
            if (tmpFile == null)
                return null;

            Attachment res = new Attachment();
            res.Name = UrlToUse;
            res.Format = (int) AttachmentFormat.Pdf;
            res.MediaData = DaoUtils.CreateMediaData(AnyFileToBytes(tmpFile));
            res.Title = "";
            res.Thumb = TryCreatePdfThumb(tmpFile);
            res.Link = Url;

            if (Point != null)
                Point.Attachment.Add(res);
            ///PublicBoardCtx.Get().SaveChanges();

            return res;
        }

        //if Url!=null, uses it. otherwice asks for URL
        private static Attachment AttachFromYoutube(ArgPoint Point, string Url)
        {
            string URLToUse = Url;
            if (URLToUse == null)
            {
                InpDialog dlg = new InpDialog();
                dlg.ShowDialog();
                URLToUse = dlg.Answer;

                if (URLToUse == null || !URLToUse.StartsWith("http://"))
                    return null;
            }

            BusyWndSingleton.Show("Processing Youtube attachment...");
            Attachment res = new Attachment();
            try
            {
                YouTubeInfo videoInfo = YouTubeProvider.LoadVideo(URLToUse);
                if (videoInfo == null)
                    return null;

                res.Format = (int) AttachmentFormat.Youtube;
                res.VideoEmbedURL = videoInfo.EmbedUrl;
                res.VideoThumbURL = videoInfo.ThumbNailUrl;
                res.VideoLinkURL = videoInfo.LinkUrl;
                res.Link = videoInfo.LinkUrl;
                res.Title = videoInfo.VideoTitle;
                res.Name = URLToUse;
                res.Thumb = ImageToBytes(GetYoutubeThumb(videoInfo.ThumbNailUrl), new JpegBitmapEncoder());

                if (Point != null)
                    Point.Attachment.Add(res);
                ///PublicBoardCtx.Get().SaveChanges();
            }
            finally
            {
                BusyWndSingleton.Hide();
            }
            return res;
        }

        private static string DownloadImageFromURL(string url)
        {
            WebClient webClient = new WebClient();
            try
            {
                using (Stream stream = webClient.OpenRead(url))
                {
                    using (Bitmap bitmap = new Bitmap(stream))
                    {
                        stream.Flush();
                        stream.Close();

                        string imagePathName = Utils.RandomFilePath(".jpg");
                        bitmap.Save(imagePathName, ImageFormat.Jpeg);
                        bitmap.Dispose();
                        return imagePathName;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
        }

        private static string DownloadPdfFromUrl(string pdfUrl)
        {
            BusyWndSingleton.Show("Processing PDF-link...");
            WebClient webClient = new WebClient();
            try
            {
                using (Stream stream = webClient.OpenRead(pdfUrl))
                {
                    string PathName = Utils.RandomFilePath(".pdf");
                    using (var fs = new FileStream(PathName, FileMode.Create))
                    {
                        stream.CopyTo(fs);
                        stream.Flush();
                        return PathName;
                    }
                }
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                BusyWndSingleton.Hide();
            }
        }

        private static Dictionary<string, AttachmentFormat> extToFmt = null;

        private static AttachmentFormat GetImgFmt(string pathName)
        {
            if (extToFmt == null)
            {
                extToFmt = new Dictionary<string, AttachmentFormat>();
                extToFmt.Add(".jpg", AttachmentFormat.Jpg);
                extToFmt.Add(".jpeg", AttachmentFormat.Jpg);
                extToFmt.Add(".bmp", AttachmentFormat.Bmp);
                extToFmt.Add(".png", AttachmentFormat.Png);
                extToFmt.Add(".pdf", AttachmentFormat.Pdf);

                extToFmt.Add(".docx", AttachmentFormat.WordDocSet);
                extToFmt.Add(".docm", AttachmentFormat.WordDocSet);
                extToFmt.Add(".dotx", AttachmentFormat.WordDocSet);
                extToFmt.Add(".dotm", AttachmentFormat.WordDocSet);
                extToFmt.Add(".doc", AttachmentFormat.WordDocSet);
                extToFmt.Add(".rtf", AttachmentFormat.WordDocSet);
                extToFmt.Add(".odt", AttachmentFormat.WordDocSet);

                extToFmt.Add(".xlsx", AttachmentFormat.ExcelDocSet);
                extToFmt.Add(".xlsm", AttachmentFormat.ExcelDocSet);
                extToFmt.Add(".xlsb", AttachmentFormat.ExcelDocSet);
                extToFmt.Add(".xltx", AttachmentFormat.ExcelDocSet);
                extToFmt.Add(".xltm", AttachmentFormat.ExcelDocSet);
                extToFmt.Add(".xls", AttachmentFormat.ExcelDocSet);
                extToFmt.Add(".xlt", AttachmentFormat.ExcelDocSet);

                extToFmt.Add(".pptx", AttachmentFormat.PowerPointDocSet);
                extToFmt.Add(".ppt", AttachmentFormat.PowerPointDocSet);
                extToFmt.Add(".pptm", AttachmentFormat.PowerPointDocSet);
                extToFmt.Add(".ppsx", AttachmentFormat.PowerPointDocSet);
                extToFmt.Add(".pps", AttachmentFormat.PowerPointDocSet);
                extToFmt.Add(".ppsm", AttachmentFormat.PowerPointDocSet);
                extToFmt.Add(".potx", AttachmentFormat.PowerPointDocSet);
                extToFmt.Add(".pot", AttachmentFormat.PowerPointDocSet);
                extToFmt.Add(".potm", AttachmentFormat.PowerPointDocSet);
                extToFmt.Add(".odp", AttachmentFormat.PowerPointDocSet);
            }
            var ext = Path.GetExtension(pathName).ToLower();
            return extToFmt[ext];
        }

        private static Attachment AttachAsBlob(string filter, AttachmentFormat format, bool autoInferenceOfFormat,
                                               ArgPoint Point)
        {
            OpenFileDialog openFileDialog1 = new OpenFileDialog();

            openFileDialog1.Filter = filter;
            openFileDialog1.RestoreDirectory = true;

            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                string attachmentName = Path.GetFileName(openFileDialog1.FileName);

                Attachment a = new Attachment();
                a.Name = attachmentName;
                a.Title = attachmentName;
                a.Link = openFileDialog1.FileName;

                if (autoInferenceOfFormat)
                    format = GetImgFmt(openFileDialog1.FileName);
                switch (format)
                {
                    case AttachmentFormat.Pdf:
                        a.MediaData = DaoUtils.CreateMediaData(AnyFileToBytes(openFileDialog1.FileName));
                        a.Thumb = TryCreatePdfThumb(openFileDialog1.FileName);
                        break;
                    case AttachmentFormat.Jpg:
                        a.MediaData = DaoUtils.CreateMediaData(ImgFileToBytes(openFileDialog1.FileName));
                        break;
                    case AttachmentFormat.Png:
                        a.MediaData = DaoUtils.CreateMediaData(ImgFileToBytes(openFileDialog1.FileName));
                        break;
                    case AttachmentFormat.Bmp:
                        a.MediaData = DaoUtils.CreateMediaData(ImgFileToBytes(openFileDialog1.FileName));
                        break;
                    case AttachmentFormat.ExcelDocSet:
                        a.MediaData = DaoUtils.CreateMediaData(AnyFileToBytes(openFileDialog1.FileName));
                        break;
                    case AttachmentFormat.WordDocSet:
                        a.MediaData = DaoUtils.CreateMediaData(AnyFileToBytes(openFileDialog1.FileName));
                        break;
                    case AttachmentFormat.PowerPointDocSet:
                        a.MediaData = DaoUtils.CreateMediaData(AnyFileToBytes(openFileDialog1.FileName));
                        break;
                }
                a.Format = (int) format;
                a.Name = attachmentName;

                if (Point != null)
                    Point.Attachment.Add(a);

                return a;
            }

            return null;
        }

        public static Attachment AttachScreenshot(ArgPoint Point, Bitmap screenshot)
        {
            var screenPath = Utils.RandomFilePath(".png");
            screenshot.Save(screenPath, ImageFormat.Png);

            var a = new Attachment();
            a.Name = screenPath;
            a.Format = (int) AttachmentFormat.PngScreenshot;
            a.MediaData = DaoUtils.CreateMediaData(ImgFileToBytes(screenPath));
            a.Title = "Screenshot, " + Environment.MachineName + " " + DateTime.Now;
            a.Link = a.Title;
            if (Point != null)
                a.ArgPoint = Point;
            return a;
        }

        public class IncorrectAttachmentFormat : Exception
        {
        };

        public static Attachment AttachCloudEntry(ArgPoint Point, StorageSelectionEntry selEntry)
        {
            var a = new Attachment {Name = selEntry.Title};
            try
            {
                a.Format = (int) GetImgFmt(selEntry.Title); //may throw exception in case of unsupported file format
            }
            catch (Exception)
            {
                throw new IncorrectAttachmentFormat();
            }

            a.MediaData = DaoUtils.CreateMediaData(AnyFileToBytes(selEntry.PathName));
            a.Title = "";// selEntry.Title;
            a.Link = selEntry.Title;
            if (a.Format == (int) AttachmentFormat.Pdf)
                a.Thumb = TryCreatePdfThumb(selEntry.PathName);

            if (Point != null)
                a.ArgPoint = Point;
            return a;
        }

        public static byte[] ImgFileToBytes(string PathName)
        {
            BitmapEncoder enc = GetEncoder(PathName);
            if (enc == null)
                return null;

            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.StreamSource = new MemoryStream();
            bmp.UriSource = new Uri(PathName);
            bmp.EndInit();

            return ImageToBytes(bmp, enc);
        }

        private static BitmapSource fileToBmpSrc(string PathName)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(PathName);
            bmp.EndInit();
            return bmp;
        }

        private static byte[] AnyFileToBytes(string PathName)
        {
            using (FileStream fs = new FileStream(PathName, FileMode.Open, FileAccess.Read))
            {
                byte[] res = new byte[fs.Length];
                fs.Read(res, 0, (int) fs.Length);
                return res;
            }
        }

        private static BitmapEncoder GetEncoder(string ImgFileName)
        {
            switch (Path.GetExtension(ImgFileName).ToLower())
            {
                case ".jpeg":
                    return new JpegBitmapEncoder();
                case ".jpg":
                    return new JpegBitmapEncoder();
                case ".png":
                    return new PngBitmapEncoder();
                case ".bmp":
                    return new BmpBitmapEncoder();
            }
            return null;
        }

        private static byte[] ImageToBytes(BitmapSource img, BitmapEncoder enc)
        {
            MemoryStream memStream = new MemoryStream();
            enc.Frames.Add(BitmapFrame.Create(img));
            enc.Save(memStream);
            return memStream.GetBuffer();
        }

        public static void SaveBitmapSource(BitmapSource src, string pathName)
        {
            using (var fs = new FileStream(pathName, FileMode.Create))
            {
                var enc = new PngBitmapEncoder();
                enc.Frames.Add(BitmapFrame.Create(src));
                enc.Save(fs);
            }
        }

        public static string GetAppDir()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string GetResourcesDir()
        {
            return Path.Combine(GetAppDir(), "Resources");
        }

        public static void RunViewer(Attachment a)
        {
            if (a == null)
                return;

            if (a.Format == (int) AttachmentFormat.Pdf && a.MediaData != null)
            {
                string pdfPathName = Utils.RandomFilePath(".pdf");
                try
                {
                    using (var fs = new FileStream(pdfPathName, FileMode.Create))
                    {
                        fs.Write(a.MediaData.Data, 0, a.MediaData.Data.Length);
                    }
                    //Process.Start(pdfPathName);
                    Utils.ReportMediaOpened(StEvent.PdfOpened, a);
                    var pdfReader = new ReaderWindow(pdfPathName);
                    pdfReader.ShowDialog();
                }
                catch (Exception e)
                {
                    MessageDlg.Show(e.ToString(), "Error");
                }
            }
            else if (MiniAttachmentManager.IsGraphicFormat(a))
            {
                if (a.Format == (int) AttachmentFormat.PngScreenshot)
                    Utils.ReportMediaOpened(StEvent.ScreenshotOpened, a);
                else
                    Utils.ReportMediaOpened(StEvent.ImageOpened, a);

                ImageWindow wnd = new ImageWindow(a.Id);
                wnd.img.Source = LoadImageFromBlob(a.MediaData.Data);
                wnd.Show();
            }
            else
            {
                //office file
                var ext = Path.GetExtension(a.Link).ToLower();
                string pathName = Utils.RandomFilePath(ext);
                try
                {
                    using (var fs = new FileStream(pathName, FileMode.Create))
                    {
                        fs.Write(a.MediaData.Data, 0, a.MediaData.Data.Length);
                    }
                    Process.Start(pathName);
                }
                catch (Exception e)
                {
                    MessageDlg.Show(e.ToString(), "Error");
                }
            }
        }

        public static void RunViewer(string pathName)
        {
            var ext = Path.GetExtension(pathName).ToLower();
            if (ext == ".pdf")
            {
                var pdfReader = new ReaderWindow(pathName);
                pdfReader.ShowDialog();
            }
            else if (ext == ".jpg" || ext == ".jpeg" || ext == ".bmp" || ext == ".png")
            {
                ImageWindow wnd = new ImageWindow(-1);
                var bi = new BitmapImage(new Uri(pathName));
                wnd.img.Source = bi;
                wnd.Show();
            }
            else
            {
                try
                {
                    Process.Start(pathName);
                }
                catch (Exception)
                {
                }
            }
        }

        //returns bytes stream of image or null
        public static byte[] TryCreatePdfThumb(string pdfPathName)
        {
            Acrobat.CAcroPDDoc doc = null;
            Acrobat.CAcroPDPage page = null;

            try
            {
                // instanciate adobe acrobat
                doc = (Acrobat.CAcroPDDoc) new Acrobat.AcroPDDocClass();

                if (doc.Open(pdfPathName))
                {
                    if (doc.GetNumPages() > 0)
                    {
                        // get reference to page
                        // pages use a zero based index so 0 = page1
                        page = (Acrobat.CAcroPDPage) doc.AcquirePage(0);

                        // get dimensions of page and create rect to indicate full size
                        Acrobat.AcroPoint pt = (Acrobat.AcroPoint) page.GetSize();
                        Acrobat.CAcroRect rect = new Acrobat.AcroRectClass();
                        rect.Top = 0;
                        rect.Left = 0;
                        rect.right = pt.x;
                        rect.bottom = pt.y;

                        // copy current page to clipboard as image                        
                        page.CopyToClipboard(rect, 0, 0, 100);

                        // get image from clipboard as bitmap
                        IDataObject data = Clipboard.GetDataObject();
                        var bmp = (System.Drawing.Bitmap) data.GetData(DataFormats.Bitmap);
                        var thumb = bmp.GetThumbnailImage(pt.x/3, pt.y/3, null, IntPtr.Zero);
                        var ms = new MemoryStream();
                        thumb.Save(ms, ImageFormat.Jpeg);
                        return ms.ToArray();
                    }
                }
            }
            catch (Exception)
            {
                // MessageBox.Show(e.StackTrace);
                //Console.WriteLine(e);

                // if we get here and doc is null then we were unable to instanciate Acrobat
                //if (doc == null) 
                //    MessageBox.Show("Acrobat is not installed. Adobe Acrobat is required.");
            }
            finally
            {
                if (page != null) Marshal.ReleaseComObject(page);
                if (doc != null) Marshal.ReleaseComObject(doc);
            }

            return null;
        }
    }
}