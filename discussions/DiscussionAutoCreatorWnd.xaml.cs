using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Discussions.DbModel;
using Discussions.model;

namespace Discussions
{
    public partial class DiscussionAutoCreatorWnd : Window
    {
        public DiscussionAutoCreatorWnd()
        {
            InitializeComponent();

            DataContext = this;

            lstDiscussions.ItemsSource = PublicBoardCtx.Get().Discussion;
        }

        private Tuple<Discussion, Discussion> GetTemplates()
        {
            if (lstDiscussions.SelectedItems.Count != 2)
            {
                return new Tuple<Discussion, Discussion>(null, null);
            }
            else
            {
                return new Tuple<Discussion, Discussion>((Discussion) lstDiscussions.SelectedItems[0],
                                                         (Discussion) lstDiscussions.SelectedItems[1]);
            }
        }

        private void btnRun_Click_1(object sender, RoutedEventArgs e)
        {
            var ab = GetTemplates();
            Run(ab.Item1, ab.Item2);
        }

        private void discName_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            FillExample(GetTemplates());
        }

        private Tuple<int, int, string> GetRange()
        {
            int nFrom;
            if (!int.TryParse(from.Text, out nFrom))
                return new Tuple<int, int, string>(-1, -1, "Enter integer");

            int nTo;
            if (!int.TryParse(to.Text, out nTo))
                return new Tuple<int, int, string>(-1, -1, "Enter integer");

            if (nTo < nFrom)
                return new Tuple<int, int, string>(-1, -1, "Negative range");

            return new Tuple<int, int, string>(nFrom, nTo, "");
        }

        private string injectNumber(string template, int number)
        {
            return template.Replace("#", number.ToString());
        }

        private void FillExample(Tuple<Discussion, Discussion> ab)
        {
            if (txtExample != null)
                txtExample.Text = "";

            if (from == null || to == null || txtExample == null || ab.Item1 == null || ab.Item2 == null)
                return;

            Tuple<int, int, string> range = GetRange();
            if (range.Item3 != "")
            {
                txtExample.Text = range.Item3;
                return;
            }

            var sb = new StringBuilder();

            for (int i = range.Item1; i <= range.Item2; i++)
            {
                //create discussion
                sb.AppendLine(injectNumber(ab.Item1.Subject, i));
                foreach (var topic in ab.Item1.Topic)
                {
                    sb.Append("       ");
                    sb.AppendLine(injectNumber(topic.Name, i));
                }
                sb.AppendLine();


                sb.AppendLine(injectNumber(ab.Item2.Subject, i));
                foreach (var topic in ab.Item2.Topic)
                {
                    sb.Append("       ");
                    sb.AppendLine(injectNumber(topic.Name, i));
                }
                sb.AppendLine();
            }
            txtExample.Text = sb.ToString();
        }

        private void Run(Discussion A, Discussion B)
        {
            if (A == null || B == null)
                return;

            Tuple<int, int, string> range = GetRange();
            if (range.Item3 != "")
                return;

            var ctx = PublicBoardCtx.Get();

            var moderator = ctx.Person.FirstOrDefault(p => p.Name == "moderator");
            if (moderator == null)
            {
                MessageDlg.Show("Cannot find moderator in DB");
                return;
            }

            for (int i = range.Item1; i <= range.Item2; i++)
            {
                var disc = cloneDiscussion(ctx, A, moderator, i);
                DaoUtils.SetGeneralSide(moderator, disc, (int) SideCode.Neutral);
                ctx.AddToDiscussion(disc);

                var disc2 = cloneDiscussion(ctx, B, moderator, i);
                DaoUtils.SetGeneralSide(moderator, disc2, (int) SideCode.Neutral);
                ctx.AddToDiscussion(disc2);
            }
            ctx.SaveChanges();

            MessageDlg.Show("Done");
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
                    attach.Thumb = (byte[]) media.Thumb.Clone();

                if (media.MediaData != null && media.MediaData.Data != null)
                {
                    var mediaClone = new MediaData();
                    mediaClone.Data = (byte[]) media.MediaData.Data.Clone();
                    attach.MediaData = mediaClone;
                }

                attach.Person = moderator;

                d.Attachment.Add(attach);
            }

            d.HtmlBackground = original.HtmlBackground;

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
            FillExample(GetTemplates());
        }

        private void to_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            FillExample(GetTemplates());
        }

        private void DiscussionAutoCreatorWnd_Loaded_1(object sender, RoutedEventArgs e)
        {
            FillExample(GetTemplates());
        }

        private void SurfaceTextBox_TextChanged_1(object sender, TextChangedEventArgs e)
        {
            FillExample(GetTemplates());
        }

        private void lstDiscussions_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            FillExample(GetTemplates());
        }
    }
}