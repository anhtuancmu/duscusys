using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;
using Microsoft.Surface.Presentation.Controls;

namespace Discussions
{
    /// <summary>
    /// Interaction logic for CopyDlg.xaml
    /// </summary>
    public partial class CopyDlg : SurfaceWindow
    {
        public ObservableCollection<Topic> srcTopics { get; set; }
        public ObservableCollection<Topic> dstTopics { get; set; }

        public ObservableCollection<ArgPoint> srcPoints { get; set; }
        public ObservableCollection<ArgPoint> dstPoints { get; set; }

        private UISharedRTClient _sharedClient;

        public bool operationPerformed = false;

        public CopyDlg(UISharedRTClient sharedClient, int topicToSelect)
        {
            InitializeComponent();

            _sharedClient = sharedClient;

            var dId = SessionInfo.Get().discussion.Id;
            var disc = PrivateCenterCtx.Get().Discussion.FirstOrDefault(d0 => d0.Id == dId);

            var topics = disc.Topic;
            srcTopics = new ObservableCollection<Topic>(topics);
            dstTopics = new ObservableCollection<Topic>(topics);

            srcPoints = new ObservableCollection<ArgPoint>();
            dstPoints = new ObservableCollection<ArgPoint>();

            DataContext = this;

            //select topic that was selected in main board
            var selectedTopic = disc.Topic.FirstOrDefault(t0 => t0.Id == topicToSelect);
            lstSrcTopics.SelectedItem = selectedTopic;
            updateSrcPoints(selectedTopic);

            if (dstTopics.Count > 0)
                lstDstTopics.SelectedIndex = 0;
        }

        private void updateSrcPoints(Topic t)
        {
            int selfId = SessionInfo.Get().person.Id;
            srcPoints.Clear();

            if (t == null)
                return;

            foreach (var ap in t.ArgPoint.Where(ap0 => ap0.Person.Id == selfId))
                srcPoints.Add(ap);
        }

        private void lstSrcTopics_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            var t = lstSrcTopics.SelectedItem as Topic;
            updateSrcPoints(t);
        }

        private void lstDstTopics_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
        }

        private void btnToCopied_Click(object sender, RoutedEventArgs e)
        {
            foreach (var it in lstSrcPoints.SelectedItems.Cast<ArgPoint>().ToArray())
            {
                var ap = it as ArgPoint;
                if (!dstPoints.Contains(ap))
                {
                    dstPoints.Add(ap);
                    srcPoints.Remove(ap);
                }
            }
        }

        private void btnToOriginal_Click(object sender, RoutedEventArgs e)
        {
            foreach (var it in lstDstPoints.SelectedItems.Cast<ArgPoint>().ToArray())
            {
                var ap = it as ArgPoint;
                if (!srcPoints.Contains(ap))
                {
                    dstPoints.Remove(ap);
                    srcPoints.Add(ap);
                }
            }
        }

        private void btnConfirm_Click_1(object sender, RoutedEventArgs e)
        {
            var dstTopic = lstDstTopics.SelectedItem as Topic;
            if (dstTopic == null)
            {
                MessageBox.Show("Target topic not selected");
                return;
            }

            BusyWndSingleton.Show("Copying points...");
            try
            {
                var ownId = SessionInfo.Get().person.Id;
                var discId = SessionInfo.Get().discussion.Id;
                foreach (var ap in dstPoints)
                {
                    copyArgPointTo(ap, dstTopic);
                }

                PrivateCenterCtx.SaveChangesIgnoreConflicts();

                _sharedClient.clienRt.SendNotifyStructureChanged(dstTopic.Id, ownId, DeviceType.Wpf);
                var srcTopic = lstSrcTopics.SelectedItem as Topic;
                _sharedClient.clienRt.SendStatsEvent(StEvent.ArgPointTopicChanged, ownId, discId, srcTopic.Id,
                                                     DeviceType.Wpf); //src
                _sharedClient.clienRt.SendStatsEvent(StEvent.ArgPointTopicChanged, ownId, discId, dstTopic.Id,
                                                     DeviceType.Wpf); //dst
            }
            finally
            {
                BusyWndSingleton.Hide();
            }
        }

        private void copyArgPointTo(ArgPoint ap, Topic t)
        {
            var pointCopy = DaoUtils.clonePoint(PrivateCenterCtx.Get(),
                                                ap,
                                                t,
                                                SessionInfo.Get().person,
                                                ap.Point + "_Copy");

            operationPerformed = true;

            Close();
        }
    }
}