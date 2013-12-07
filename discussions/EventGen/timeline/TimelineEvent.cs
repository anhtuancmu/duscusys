using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Discussions.DbModel.model;
using Discussions.stats;
using EventGen.timeline;

namespace EventGen
{
    public class TimelineEvent : INotifyPropertyChanged
    {
        public TimelineEventView view;
        public StEvent e;
        public int userId;
        public int discussionId;
        public int topicId;
        public DeviceType devType;
        public Timeline timeline;

        public void Write(BinaryWriter w)
        {
            w.Write((int) e);
            w.Write(userId);
            w.Write(discussionId);
            w.Write(topicId);
            w.Write((int) devType);
            w.Write(StickHeight);
            w.Write(Span.TotalSeconds);
        }

        public void Read(BinaryReader r)
        {
            e = (StEvent) r.ReadInt32();
            userId = r.ReadInt32();
            discussionId = r.ReadInt32();
            topicId = r.ReadInt32();
            devType = (DeviceType) r.ReadInt32();
            StickHeight = r.ReadDouble();
            Span = TimeSpan.FromSeconds(r.ReadDouble());
        }

        private double stickHeigth = (new Random()).Next(150) + 20; //only used by event view 

        public double StickHeight
        {
            get { return stickHeigth; }
            set
            {
                if (value != stickHeigth)
                {
                    stickHeigth = value;
                    NotifyPropertyChanged("StickHeight");
                }
            }
        }

        private bool _isEvSelected = false;

        public bool IsEvSelected
        {
            get { return _isEvSelected; }
            set
            {
                if (value != _isEvSelected)
                {
                    _isEvSelected = value;
                    NotifyPropertyChanged("IsEvSelected");
                    NotifyPropertyChanged("BorderThickness");
                    NotifyPropertyChanged("Thickness");
                }
            }
        }

        public Thickness BorderThickness
        {
            get
            {
                if (IsEvSelected)
                    return new Thickness(10.0);
                else
                    return new Thickness(1.0);
            }
        }

        public double Thickness
        {
            get
            {
                if (IsEvSelected)
                    return 10.0;
                else
                    return 0;
            }
        }

        private string _userName = "";

        public string UserName
        {
            get { return _userName; }
        }

        private string _eventName = "";

        public string EventName
        {
            get { return _eventName; }
        }

        private string _devName = "";

        public string DeviceName
        {
            get { return _devName; }
        }

        private SolidColorBrush _userColor;

        public SolidColorBrush UserColor
        {
            get { return _userColor; }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        private TimeSpan _timespan;

        public TimeSpan Span
        {
            get { return _timespan; }

            set
            {
                if (value != _timespan)
                {
                    if (0 <= value.TotalSeconds && value.TotalSeconds <= timeline.Range.TotalSeconds)
                    {
                        _timespan = value;
                        NotifyPropertyChanged("Span");
                    }
                }
            }
        }

        public TimelineEvent(StEvent e, int userId, int discussionId, Timeline timeline, TimeSpan timespan, int topicId,
                             DeviceType devType)
        {
            this.timeline = timeline;
            this.e = e;
            this.userId = userId;
            this.discussionId = discussionId;
            this.Span = timespan;
            this.topicId = topicId;
            this.devType = devType;

            var evm = new EventViewModel(e, userId, DateTime.Now, devType);
            _userColor = evm.userColor;
            _userName = evm.userName;
            _devName = evm.devType;
            _eventName = evm.evt;
        }

        public TimelineEvent(BinaryReader r, Timeline timeline)
        {
            this.timeline = timeline;

            Read(r);

            //update view params
            var evm = new EventViewModel(e, userId, DateTime.Now, devType);
            _userColor = evm.userColor;
            _userName = evm.userName;
            _devName = evm.devType;
            _eventName = evm.evt;
        }
    }
}