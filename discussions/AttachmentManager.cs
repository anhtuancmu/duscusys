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
        static BitmapImage _pdfIcon = null;
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
            return
                a.Format == (int)AttachmentFormat.Bmp ||
                a.Format == (int)AttachmentFormat.Jpg ||
                a.Format == (int)AttachmentFormat.Png ||
                a.Format == (int)AttachmentFormat.PngScreenshot;
        }

        public static BitmapSource GetAttachmentBitmap3(Attachment a)
        {
            if (a == null)
                return null;

            switch (a.Format)                
            {
                case (int)AttachmentFormat.Pdf:
                    if(a.Thumb!=null)
                        return LoadImageFromBlob(a.Thumb);
                    else
                        return PdfIcon;
                case (int)AttachmentFormat.Jpg:
                case (int)AttachmentFormat.Png:
                case (int)AttachmentFormat.PngScreenshot:
                case (int)AttachmentFormat.Bmp:
                    return LoadImageFromBlob(a.MediaData.Data);
                case (int)AttachmentFormat.Youtube:
                    if(a.Thumb!=null)
                        return LoadImageFromBlob(a.Thumb);
                    else
                        return GetYoutubeThumb(a.VideoThumbURL);                    
            }

            return null;
        }

        static BitmapSource GetYoutubeThumb(string thumbUrl)
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

        static ImageSource ProcessCommand2(ArgPoint Point, AttachCmd cmd, string Url, ref Attachment a)
        {
            a = null;

            try
            {
                switch (cmd)
                {
                    case AttachCmd.ATTACH_IMG_OR_PDF:
                        a = AttachmentManager.AttachPictureOrPdf(Point);
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
                            return new BitmapImage();//returning stub for external error-checking
                        break;
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Cannot process attachment. If it's link, check it's correct. If it's file, ensure program has permissions to access it",
                                "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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
            return AttachAsBlob("Graphics |*.jpg;*.jpeg;*.bmp;*.png", Point);
        }

        public static Attachment AttachPDF(ArgPoint Point)
        {
            return AttachAsBlob("PDF |*.pdf", Point);
        }

        public static Attachment AttachPictureOrPdf(ArgPoint Point)
        {
            return AttachAsBlob("Graphics or PDF |*.jpg;*.jpeg;*.bmp;*.png;*.pdf", Point);
        }

        //if URL==null, shows URL input dialog. else uses provided URL, no dialog
        static Attachment AttachFromURL(ArgPoint Point, string Url)
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
            res.Format = (int)AttachmentFormat.Jpg; //all downloads are jpg 
            res.MediaData = DaoUtils.CreateMediaData(ImgFileToBytes(tmpFile));
            res.Title = "";
            res.Link = Url;

            if (Point != null)
                Point.Attachment.Add(res);
            //CtxSingleton.Get().SaveChanges();

            return res;
        }

        //if URL==null, shows URL input dialog. else uses provided URL, no dialog
        static Attachment AttachPdfFromURL(ArgPoint Point, string Url)
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
            res.Format = (int)AttachmentFormat.Pdf;
            res.MediaData = DaoUtils.CreateMediaData(PdfFileToBytes(tmpFile));
            res.Title = "";
            res.Thumb = TryCreatePdfThumb(tmpFile);
            res.Link = Url;

            if (Point != null)
                Point.Attachment.Add(res);
            ///CtxSingleton.Get().SaveChanges();

            return res;
        }

        //if Url!=null, uses it. otherwice asks for URL
        static Attachment AttachFromYoutube(ArgPoint Point, string Url)
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

                res.Format = (int)AttachmentFormat.Youtube;
                res.VideoEmbedURL = videoInfo.EmbedUrl; 
                res.VideoThumbURL = videoInfo.ThumbNailUrl;
                res.VideoLinkURL = videoInfo.LinkUrl;
                res.Link = videoInfo.LinkUrl;
                res.Title = videoInfo.VideoTitle;
                res.Name = URLToUse;
                res.Thumb = ImageToBytes(GetYoutubeThumb(videoInfo.ThumbNailUrl), new JpegBitmapEncoder());  

                if (Point != null)
                    Point.Attachment.Add(res);
                ///CtxSingleton.Get().SaveChanges();
            }
            finally
            {
                BusyWndSingleton.Hide();   
            }
            return res;
        }

        static string DownloadImageFromURL(string url)
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
            catch(Exception)
            {
                return null;
            }
        }

        static string DownloadPdfFromUrl(string pdfUrl)
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

        static Attachment AttachAsBlob(string filter, ArgPoint Point)
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
                switch (Path.GetExtension(a.Name).ToLower())
                {
                    case ".pdf":
                        a.MediaData = DaoUtils.CreateMediaData(PdfFileToBytes(openFileDialog1.FileName));                       
                        a.Thumb = TryCreatePdfThumb(openFileDialog1.FileName);
                        a.Format = (int)AttachmentFormat.Pdf;
                        break;
                    case ".jpg":
                        a.Format = (int)AttachmentFormat.Jpg;
                        a.MediaData = DaoUtils.CreateMediaData(ImgFileToBytes(openFileDialog1.FileName));
                        break;
                    case ".jpeg":
                        a.Format = (int)AttachmentFormat.Jpg;
                        a.MediaData = DaoUtils.CreateMediaData(ImgFileToBytes(openFileDialog1.FileName));
                        break;
                    case ".png":
                        a.Format = (int)AttachmentFormat.Png;
                        a.MediaData = DaoUtils.CreateMediaData(ImgFileToBytes(openFileDialog1.FileName));
                        break;
                    case ".bmp":
                        a.Format = (int)AttachmentFormat.Bmp;
                        a.MediaData = DaoUtils.CreateMediaData(ImgFileToBytes(openFileDialog1.FileName));
                        break;
                }
                a.Name = attachmentName;

                if (Point != null)
                {
                    //todo: it's ok for multiple attachments
                    //if (Point.Attachment.Count > 0)
                    //    Point.Attachment.Clear();

                    Point.Attachment.Add(a);
                }

                // var ctx = CtxSingleton.Get();                
                //ctx.SaveChanges();

                return a;
            }

            return null;
        }

        public static Attachment AttachScreenshot(ArgPoint Point, Bitmap screenshot)
        {
            var screenPath = Utils.RandomFilePath(".png");
            screenshot.Save(screenPath, ImageFormat.Png);

            Attachment a = new Attachment();
            a.Name = screenPath;
            a.Format = (int)AttachmentFormat.PngScreenshot;
            a.MediaData = DaoUtils.CreateMediaData(ImgFileToBytes(screenPath));
            a.Title = "Screenshot, " + Environment.MachineName + " " + DateTime.Now;
            a.Link = a.Title; 
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

        static BitmapSource fileToBmpSrc(string PathName)
        {
            BitmapImage bmp = new BitmapImage();
            bmp.BeginInit();
            bmp.UriSource = new Uri(PathName);
            bmp.EndInit();
            return bmp;
        }

        static byte[] PdfFileToBytes(string PathName)
        {
            using (FileStream fs = new FileStream(PathName, FileMode.Open, FileAccess.Read))
            {
                byte[] res = new byte[fs.Length];
                fs.Read(res, 0, (int)fs.Length);
                return res;
            }
        }

        static BitmapEncoder GetEncoder(string ImgFileName)
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
        
        static byte[] ImageToBytes(BitmapSource img, BitmapEncoder enc)
        {
            MemoryStream memStream = new MemoryStream();
            enc.Frames.Add(BitmapFrame.Create(img));
            enc.Save(memStream);
            return memStream.GetBuffer();
        }

        public static string GetAppDir()
        {
            return Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        }

        public static string GetResourcesDir()
        {
            return Path.Combine(GetAppDir(), "Resources");
        }

        public static void RunViewer(System.Windows.Controls.Image img)
        {
            if (img == null)
                return;

            var a = img.DataContext as Attachment;
            if (a == null)
                return;

            if (a.Format == (int)AttachmentFormat.Pdf && a.MediaData != null)
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
                    MessageBox.Show(e.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else if (MiniAttachmentManager.IsGraphicFormat(a))
            {
                if(a.Format== (int)AttachmentFormat.PngScreenshot)
                    Utils.ReportMediaOpened(StEvent.ScreenshotOpened, a);
                else
                    Utils.ReportMediaOpened(StEvent.ImageOpened, a);
                ImageWindow wnd = new ImageWindow();
                wnd.img.Source = img.Source;
                wnd.Show();
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
                doc = (Acrobat.CAcroPDDoc)new Acrobat.AcroPDDocClass();

                if (doc.Open(pdfPathName))
                {
                    if (doc.GetNumPages() > 0)
                    {
                        // get reference to page
                        // pages use a zero based index so 0 = page1
                        page = (Acrobat.CAcroPDPage)doc.AcquirePage(0);

                        // get dimensions of page and create rect to indicate full size
                        Acrobat.AcroPoint pt = (Acrobat.AcroPoint)page.GetSize();
                        Acrobat.CAcroRect rect = new Acrobat.AcroRectClass();
                        rect.Top = 0;
                        rect.Left = 0;
                        rect.right = pt.x;
                        rect.bottom = pt.y;

                        // copy current page to clipboard as image                        
                        page.CopyToClipboard(rect, 0, 0, 100);

                        // get image from clipboard as bitmap
                        IDataObject data = Clipboard.GetDataObject();
                        var bmp = (System.Drawing.Bitmap)data.GetData(DataFormats.Bitmap);
                        var thumb = bmp.GetThumbnailImage(pt.x/6,pt.y/6, null, IntPtr.Zero);
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
