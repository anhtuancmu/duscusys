using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Surface;
using Microsoft.Surface.Presentation;
using Microsoft.Surface.Presentation.Controls;
using Microsoft.Surface.Presentation.Input;
using System.Collections.ObjectModel;
using Discussions.DbModel;
using Discussions.model;
using System.Data;
using Discussions.rt;

namespace Discussions 
{
    public partial class DiscussionAutoCreatorWnd : SurfaceWindow
    {    
        Discussion _d = null;

        public DiscussionAutoCreatorWnd(Discussion d)
        {
            InitializeComponent();

            _d = d;            
            
            DataContext = this;          
        }

        private void btnRun_Click_1(object sender, RoutedEventArgs e)
        {
            Run();
        }

        private void discName_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            FillExample();
        }

        Tuple<int, int,string> GetRange()
        {
            int nFrom;
            if (!int.TryParse(from.Text, out nFrom))
                return new Tuple<int, int,string>(-1, -1, "Enter integer");

            int nTo;
            if (!int.TryParse(to.Text, out nTo))
                return new Tuple<int, int, string>(-1, -1, "Enter integer");

            if (nTo < nFrom)
                return new Tuple<int, int, string>(-1, -1, "Negative range");

            return new Tuple<int, int, string>(nFrom, nTo, "");
        }

        string injectNumber(string template, int number)
        {
            return template.Replace("#", number.ToString()); 
        }

        void FillExample()
        {
            if (from == null || to == null || txtExample == null || _d==null)
                return;

            Tuple<int, int, string> range = GetRange();
            if (range.Item3 != "")
            {
                txtExample.Text = range.Item3;
                return;
            }

            var discTemplate = _d.Subject;

            var sb = new StringBuilder();

            for (int i = range.Item1; i <= range.Item2; i++)
            {
                //create discussion

                sb.AppendLine(injectNumber(discTemplate, i));
                foreach (var topic in _d.Topic)
                {
                    sb.Append("       ");
                    sb.AppendLine(injectNumber(topic.Name, i));
                }
                sb.AppendLine();
            }
            txtExample.Text = sb.ToString();
        }

        void Run()
        {
            if (_d == null)
                return; 

            Tuple<int, int, string> range = GetRange();
            if (range.Item3 != "")
                return;

            var ctx = CtxSingleton.Get();

            var moderator = ctx.Person.FirstOrDefault(p => p.Name == "moderator");
            if (moderator == null)
            {
                MessageBox.Show("Cannot find moderator in DB");
                return;
            }
           
            for (int i = range.Item1; i <= range.Item2; i++)
            {
                var disc = cloneDiscussion(ctx, _d, moderator, i);
                DaoUtils.SetGeneralSide(moderator, disc, (int)SideCode.Neutral);
                ctx.AddToDiscussion(disc);
            }
            ctx.SaveChanges();

            MessageBox.Show("Done");
        }
        
        public Discussion cloneDiscussion(DiscCtx ctx, Discussion original, Person moderator, int i)
        {
            var d = new Discussion();
            d.Subject = injectNumber(original.Subject, i);

            //copy background
            d.Background = new RichText();
            d.Background.Text = original.Background.Text;
            foreach (var src in original.Background.Source)
            {
                var s = new Source();
                s.Text = src.Text;
                s.OrderNumber = src.OrderNumber;                
                d.Background.Source.Add(s);
            }

            foreach (var media in original.Attachment)
            {
                var attach = new Attachment();
                attach.Discussion = d;
                attach.Format = media.Format;
                attach.Link = media.Link;
                attach.Name = media.Name;
                attach.Title = media.Title;
                attach.VideoEmbedURL = media.VideoEmbedURL;
                attach.VideoLinkURL = media.VideoLinkURL;
                attach.VideoThumbURL = media.VideoThumbURL;
                attach.OrderNumber = media.OrderNumber;

                if (media.Thumb != null)
                    attach.Thumb = (byte[])media.Thumb.Clone();

                if (media.MediaData != null && media.MediaData.Data != null)
                {
                    var mediaClone = new MediaData();
                    mediaClone.Data = (byte[])media.MediaData.Data.Clone();
                    attach.MediaData = mediaClone;
                }

                attach.Person = moderator;

                d.Attachment.Add(attach);
            }

            foreach (var topic in original.Topic)
            {
                var t = new Topic();
                t.Name = injectNumber(topic.Name, i);
                t.Person.Add(moderator);
                d.Topic.Add(t);
            }

            return d;
        }

        private void from_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            FillExample();
        }

        private void to_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            FillExample();
        }

        private void DiscussionAutoCreatorWnd_Loaded_1(object sender, RoutedEventArgs e)
        {
            FillExample();
        }

        private void SurfaceTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            FillExample();
        }
    }
}