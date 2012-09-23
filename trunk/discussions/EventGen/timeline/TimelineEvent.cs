﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Discussions.model;
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

        bool _isEvSelected = false;
        public bool IsEvSelected
        {
            get
            {
                return _isEvSelected;
            }
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

        string _userName = "";
        public string UserName
        {
            get
            {
                return _userName; 
            }
        }

        string _eventName = "";
        public string EventName
        {
            get
            {
                return _eventName;
            }
        }
         
        string _devName = "";
        public string DeviceName
        {
            get
            {
                return _devName;
            }
        }

        SolidColorBrush _userColor;
        public SolidColorBrush UserColor 
        {
            get
            {
                return _userColor;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        TimeSpan _timespan;
        public TimeSpan Span
        {
            get 
            {
                return _timespan;
            }

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

        public TimelineEvent(StEvent e, int userId, int discussionId, Timeline timeline, TimeSpan timespan, int topicId, DeviceType devType)
        {
            this.timeline = timeline;
            this.e = e;
            this.userId  = userId;
            this.discussionId = discussionId;
            this.Span    = timespan;
            this.topicId = topicId;
            this.devType = devType;

            var evm = new EventViewModel(e, userId, DateTime.Now, devType);
            _userColor = evm.userColor;
            _userName  = evm.userName;
            _devName   = evm.devType;
            _eventName = evm.evt;       
        } 
    }
}
