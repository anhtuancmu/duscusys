using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace CloudStorage.Model
{
    public class NavState : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        //ID of currently viewer folder
        public string _currentFolderId = null;

        public string CurrentFolderId
        {
            get { return _currentFolderId; }
        }

        private string _rootDisplayName = null;
        public string _currentFolderName = null;

        public string CurrentAddress
        {
            get
            {
                var res = _rootDisplayName;
                foreach (var pathElem in _stackOfParents.Reverse())
                {
                    res += "/" + pathElem.Item2;
                }
                return res;
            }
        }

        //id of the parent folder of currently viewed folder. If null, there is no parent folder.
        private readonly Stack<Tuple<string, string>> _stackOfParents = new Stack<Tuple<string, string>>();

        public Stack<Tuple<string, string>> StackOfParents
        {
            get { return _stackOfParents; }
        }

        public void LevelDown(string folderToView, string displayName)
        {
            if (_currentFolderId != null)
                _stackOfParents.Push(new Tuple<string, string>(_currentFolderId, displayName));

            _currentFolderId = folderToView;
            NotifyPropertyChanged("CurrentFolderId");

            _currentFolderName = displayName;
            NotifyPropertyChanged("CurrentAddress");
        }

        //switches current folder one level up
        //returns id of folder to view, or null if unavailable  
        public string LevelUp()
        {
            if (_stackOfParents.Count == 0)
                return null; //we are at the root 

            var parent = _stackOfParents.Pop();

            _currentFolderId = parent.Item1;
            NotifyPropertyChanged("CurrentFolderId");

            _currentFolderName = parent.Item2;
            NotifyPropertyChanged("CurrentAddress");

            return CurrentFolderId;
        }

        public void Reset(string rootDisplayName)
        {
            _stackOfParents.Clear();

            if (rootDisplayName != null)
                _rootDisplayName = rootDisplayName;

            _currentFolderId = null;
            NotifyPropertyChanged("CurrentFolderId");

            _currentFolderName = null;
            NotifyPropertyChanged("CurrentAddress");
        }
    }
}