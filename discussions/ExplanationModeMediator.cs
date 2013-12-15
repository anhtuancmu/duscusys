using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using AbstractionLayer;
using Discussions.Annotations;
using Discussions.DbModel.model;
using Discussions.rt;

namespace Discussions
{
    public class ExplanationModeMediator : INotifyPropertyChanged
    {
        private static ExplanationModeMediator _inst = null;

        public static ExplanationModeMediator Inst
        {
            get
            {
                if (_inst == null)
                    _inst = new ExplanationModeMediator();
                return _inst;
            }
        }

        private class ViewerRecord
        {
            public PortableWindow wnd;
            public int attachId;
        }

        public int? CurrentTopicId
        {
            get; set;
        }

        public bool WebkitOpen { get; set; }
        public bool PdfOpen { get; set; }

        public bool ImageViewerOpen
        {
            get
            {
                return _openedViewers.Any();
            }
        }

        public bool ExplanationModeEnabled
        {
            get { return _explanationModeEnabled; }
            set
            {
                if (value.Equals(_explanationModeEnabled)) return;
                _explanationModeEnabled = value;
                OnPropertyChanged("ExplanationModeEnabled");
            }
        }

        private bool _lasersEnabled;
        public bool LasersEnabled
        {
            get { return _lasersEnabled; }
            set
            {
                if (value.Equals(_lasersEnabled)) return;
                _lasersEnabled = value;
                OnPropertyChanged("LasersEnabled");

                if (value)
                {
                    UISharedRTClient.Instance.clienRt.SendStatsEvent(
                        StEvent.LaserEnabled,
                        SessionInfo.Get().person.Id,
                        SessionInfo.Get().discussion.Id,
                        CurrentTopicId != null ? (int) CurrentTopicId : -1,
                        DeviceType.Wpf);
                }
            }
        }

        //all locally opened image windows 
        private readonly List<ViewerRecord> _openedViewers = new List<ViewerRecord>();

        //called when window is closed by any initiator 
        public Action<int> CloseReq;

        //called when window is opened by any initiator 
        public Action<int, bool> OpenReq;
        private bool _explanationModeEnabled;

        //called every time a window is closed by any initiator
        public void OnWndClosed(PortableWindow wnd)
        {
            var viewRec = _openedViewers.FirstOrDefault(vr => vr.wnd == wnd);
            if (viewRec != null)
            {
                _openedViewers.Remove(viewRec);
                if (CloseReq != null)
                    CloseReq(viewRec.attachId);
            }
        }

        //called every time a window is opened by any initiator
        public void OnWndOpened(PortableWindow w, int attId, bool localRequest)
        {
            if (attId < 0)
                return;

            var prevInstOfAttach = _openedViewers.FirstOrDefault(vr => vr.attachId == attId);
            if (prevInstOfAttach == null)
            {
                _openedViewers.Add(new ViewerRecord {attachId = attId, wnd = w});
                if (OpenReq != null)
                    OpenReq(attId, localRequest);
            }
        }

        public bool IsViewerOpened(int attId)
        {
            var prevInstOfAttach = _openedViewers.FirstOrDefault(vr => vr.attachId == attId);
            return prevInstOfAttach != null;
        }

        public void EnsureInstanceClosed(int attachmentId)
        {
            var prevInstOfAttach = _openedViewers.FirstOrDefault(vr => vr.attachId == attachmentId);
            if (prevInstOfAttach != null)
            {
                _openedViewers.Remove(prevInstOfAttach);
                prevInstOfAttach.wnd.Close();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}