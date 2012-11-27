using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Discussions
{
    public class WebScreenshoter
    {
        public const int VERT_SCROLLBAR_WIDTH = 20;        
        
        Action<string> _imgPathName;

        int _width;

        //hot ctor 
        public WebScreenshoter(string Uri, Action<string> imgPathName, int width)
        {
            Run(Uri, imgPathName, width);
        }

        public void Run(string Uri, Action<string> imgPathName, int width)
        {
            _imgPathName = imgPathName;

            _width = width;

            var browser = new System.Windows.Forms.WebBrowser();

            browser.DocumentCompleted += OnDocumentCompleted;

            browser.Navigate(Uri);
        }

        void OnDocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            var browser = (System.Windows.Forms.WebBrowser)sender;

            browser.Width = _width < 0 ? browser.Document.Body.ScrollRectangle.Size.Width : _width;
            browser.Height = browser.Document.Body.ScrollRectangle.Size.Height;

            browser.Width += 20;
            browser.Height += 20;
            
            using (Graphics graphics = browser.CreateGraphics())
            {
                graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;
                using (Bitmap bitmap = new Bitmap(browser.Width - VERT_SCROLLBAR_WIDTH, browser.Height, graphics))
                {
                    Rectangle bounds = new Rectangle(0, 0, bitmap.Width, bitmap.Height);
                    browser.DrawToBitmap(bitmap, bounds);

                    var pathName = Utils2.RandomFilePath(".png");                    
                    bitmap.Save(pathName, ImageFormat.Png);
                    _imgPathName(pathName);
                }
            }
            browser.Dispose();
        }
    }
}
