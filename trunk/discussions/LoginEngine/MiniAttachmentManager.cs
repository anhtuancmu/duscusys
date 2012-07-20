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

namespace Discussions
{
    public class MiniAttachmentManager
    {
        public static bool IsGraphicFormat(Attachment a)
        {
            return
                a.Format == (int)AttachmentFormat.Bmp ||
                a.Format == (int)AttachmentFormat.Jpg ||
                a.Format == (int)AttachmentFormat.Png ||
                a.Format == (int)AttachmentFormat.PngScreenshot;
        }

        public static BitmapSource GetAttachmentBitmap2(Attachment a)
        {
            switch (a.Format)
            {
                case (int)AttachmentFormat.Jpg:
                case (int)AttachmentFormat.Png:
                case (int)AttachmentFormat.Bmp:
                case (int)AttachmentFormat.PngScreenshot:
                    return LoadImageFromBlob(a.MediaData.Data);
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
    }
}
