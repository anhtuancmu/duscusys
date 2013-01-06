using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
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

        private ObservableCollection<TimelineEvent> _events = new ObservableCollection<TimelineEvent>();

        public ObservableCollection<TimelineEvent> Events
        {
            get { return _events; }
        }

        public Timeline(TimeSpan range)
        {
            Range = range;
        }

        private TimeSpan _range = TimeSpan.FromSeconds(100); //initial, will be overriden

        public TimeSpan Range
        {
            get { return _range; }
            set { _range = value; }
        }

        private TimeSpan _currentTime = TimeSpan.FromSeconds(0);

        public TimeSpan CurrentTime
        {
            get { return _currentTime; }
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
            _events.Add(ev);
        }

        public void RemoveEvent(TimelineEvent ev)
        {
            _events.Remove(ev);
        }

        public void RemoveSelectedEvents(CommandManager cmdMgr)
        {
            var numSelected = _events.Where(ev => ev.IsEvSelected).Count();
            if (numSelected > 1)
            {
                if (MessageBox.Show("Delete " + numSelected + " events?", "Bulk delete", MessageBoxButtons.OKCancel) !=
                    DialogResult.OK)
                    return;
            }

            foreach (var selected in _events.ToArray())
                if (selected.IsEvSelected)
                {
                    cmdMgr.RegisterDoneCommand(new DeleteEventCommand(selected, true));
                }
        }

        public void Write(BinaryWriter w)
        {
            w.Write(Range.TotalSeconds);
            w.Write(CurrentTime.TotalSeconds);
            w.Write(Events.Count());
            foreach (var ev in Events)
            {
                ev.Write(w);
            }
        }

        public void Read(BinaryReader r)
        {
            Range = TimeSpan.FromSeconds(r.ReadDouble());
            CurrentTime = TimeSpan.FromSeconds(r.ReadDouble());
            var nEvents = r.ReadInt32();
            for (int i = 0; i < nEvents; i++)
            {
                AddEvent(new TimelineEvent(r, this));
            }
        }
    }
}