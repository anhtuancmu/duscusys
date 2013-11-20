using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Discussions.Annotations;
using Discussions.view;

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
            public ImageWindow wnd;
            public int attachId;
        }

        public int? CurrentTopicId
        {
            get; set;
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
            }
        }

        private bool _imageLasersEnabled;
        public bool ImageLasersEnabled
        {
            get { return _imageLasersEnabled; }
            set
            {
                if (value.Equals(_imageLasersEnabled)) return;
                _imageLasersEnabled = value;
                OnPropertyChanged("ImageLasersEnabled");
            }
        }

        //all locally opened image windows 
        private readonly List<ViewerRecord> _openedViewers = new List<ViewerRecord>();

        //called when window is closed by any initiator 
        public Action<int> CloseReq;

        //called when window is opened by any initiator 
        public Action<int> OpenReq;
        private bool _explanationModeEnabled;

        //called every time a window is closed by any initiator
        public void OnWndClosed(ImageWindow wnd)
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
        public void OnWndOpened(ImageWindow w, int attId)
        {
            if (attId < 0)
                return;

            var prevInstOfAttach = _openedViewers.FirstOrDefault(vr => vr.attachId == attId);
            if (prevInstOfAttach == null)
            {
                _openedViewers.Add(new ViewerRecord {attachId = attId, wnd = w});
                if (OpenReq != null)
                    OpenReq(attId);
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