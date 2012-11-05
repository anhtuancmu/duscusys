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
        public ObservableCollection<Topic> Topics
        {
            get;
            set;
        }

        public DiscussionAutoCreatorWnd()
        {
            InitializeComponent();

            Topics = new ObservableCollection<Topic>();
            
            var t1 = new Topic(); 
            t1.Name = "Topic1";
            Topics.Add(t1);
            
            var t2 = new Topic(); 
            t2.Name = "Topic2";
            Topics.Add(t2);            

            DataContext = this;          
        }

        private void btnAddTopic_Click_1(object sender, RoutedEventArgs e)
        {          
            var t = new Topic(); 
            t.Name = "<Topic>"; 
            Topics.Add(t);

            FillExample();
        }

        private void btnRemoveTopic_Click_1(object sender, RoutedEventArgs e)
        {
            var top = topics.SelectedItem;
            if (top == null)
                return;

            Topics.Remove(topics.SelectedItem as Topic);
            FillExample();
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
            if (from == null || to == null || txtExample == null)
                return;

            Tuple<int, int,string> range = GetRange();
            if (range.Item3!="")
            {
                txtExample.Text = range.Item3;
                return;
            }

            var discTemplate = discName.Text;

            var sb = new StringBuilder();

            for(int i=range.Item1;i<=range.Item2;i++)
            {
                //create discussion
             
                sb.AppendLine(injectNumber(discTemplate, i));
                foreach(var topic in Topics)                
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
            Tuple<int, int, string> range = GetRange();
            if (range.Item3 != "")
                return;

            var ctx = CtxSingleton.Get();

            var moderator = ctx.Person.FirstOrDefault(p=>p.Name=="moderator");
            if (moderator == null)
            {
                MessageBox.Show("Cannot find moderator in DB");
                return; 
            }

            var discTemplate = discName.Text;

            for (int i = range.Item1; i <= range.Item2; i++)
            {
                //create discussion

                var d = new Discussion();
                d.Subject = injectNumber(discTemplate, i);
                d.Background = new RichText();
                d.Background.Text = "";                

                foreach (var topic in Topics)
                {
                    var t = new Topic();
                    t.Name = injectNumber(topic.Name, i);
                    t.Person.Add(moderator);
                    d.Topic.Add(t);
                }
                ctx.AddToDiscussion(d);
            }
            ctx.SaveChanges();

            MessageBox.Show("Done");
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