﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace EventGen.timeline
{
    public class Timeline : INotifyPropertyChanged 
    {
        public event PropertyChangedEventHandler PropertyChanged;
        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        ObservableCollection<TimelineEvent> _events = new ObservableCollection<TimelineEvent>();
        public ObservableCollection<TimelineEvent> Events
        {
            get
            {
                return _events;
            }
        }

        public Timeline(TimeSpan range)
        {
            Range = range;
        }

        TimeSpan _range = TimeSpan.FromSeconds(100);//initial, will be overriden
        public TimeSpan Range
        {
            get
            {
                return _range;
            }
            set
            {
                _range = value;
            }
        }

        TimeSpan _currentTime = TimeSpan.FromSeconds(0);
        public TimeSpan CurrentTime
        {
            get
            {
                return _currentTime;
            }
            set
            {
                if (value != _currentTime)
                {
                    if (0 <= value.TotalSeconds && value.TotalSeconds <= Range.TotalSeconds)
                    {
                        _currentTime = value;
                        NotifyPropertyChanged("CurrentTime");
                    }
                }
            }
        }

        public void AddEvent(TimelineEvent ev)
        {
            ev.Span = CurrentTime;
            _events.Add(ev);
        }

        public void RemoveSelectedEvents()
        {
            var numSelected = _events.Where(ev => ev.IsEvSelected).Count();
            if (numSelected > 1)
            {
                if (MessageBox.Show("Delete " + numSelected + " events?", "Bulk delete", MessageBoxButtons.OKCancel) != DialogResult.OK)
                    return;
            }

            foreach (var selected in _events.ToArray())
                if (selected.IsEvSelected)
                    _events.Remove(selected);
        }
    }
}
