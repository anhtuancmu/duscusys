﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Windows.Threading;
using Microsoft.Surface.Presentation.Controls;

namespace CloudStorage
{   
    public partial class StorageWnd : SurfaceWindow
    {
        public enum NavitionType { LevelDown, LevelUp };

        public enum SortingFunction { ByTime, ByName };

        //file entries of currently viewed level
        ObservableCollection<FileEntry> _entries = new ObservableCollection<FileEntry>();
        public ObservableCollection<FileEntry> Entries
        {
            get
            {
                return _entries;
            }
            set
            {
                _entries = value;
            }
        }

        NavState _navState = new NavState();
        public NavState NavState
        {
            get { return _navState; }
            set {_navState = value; }
        }
        IStorageClient _storage = null;

        bool _isBusy = false;
        bool _cancelled = false;
        int _folderRequestId = 0;

        public struct StorageSelectionEntry
        {
            public string PathName;
            public string Title;
        }

        public List<StorageSelectionEntry> filesToAttach = null;

        public Action<string> fileViewCallback = null;
        public Action<string> webViewCallback = null; 

        public StorageWnd()
        {
            InitializeComponent();

            DataContext = this;            
        }

        //for external use only
        public void LoginAndEnumFiles(StorageType storageType)
        {
            if (IsBusy())
                return;
            
            progressInfo.Visibility = Visibility.Visible;
            Entries.Clear(); // need to clear here for the case if authentication throws exception

            Action worker = ()=>
            {                         
                try
                {
                    switch(storageType)
                    {
                        case StorageType.Dropbox:
                            _storage = new DropStorage(webViewCallback);
                            _navState.Reset("Dropbox");               
                            break;
                        case StorageType.GoogleDrive:
                            _storage = new GDriveStorage(webViewCallback);
                            _navState.Reset("Google Drive");                     
                            break;
                        default:
                            throw new NotSupportedException();
                    }
                  
                    NavigateTo(_storage.RootFolder(), null, NavitionType.LevelDown, true);
                }
                catch (Exception e)
                {
                    downloadProgress.Visibility = Visibility.Collapsed;
                    MessageBox.Show("Authentication error. Please try to connect again: " + e.ToString(), "Error",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                }
                finally
                {
                    progressInfo.Visibility = Visibility.Collapsed;     
                }
            };

            this.Dispatcher.BeginInvoke(worker, DispatcherPriority.Background);
        }
   
        private void btnLoginDropbox_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy())
                return;
            LoginAndEnumFiles(StorageType.Dropbox);
        }

        private void btnLoginGDrive_Click(object sender, RoutedEventArgs e)
        {
            if (IsBusy())
                return;
            LoginAndEnumFiles(StorageType.GoogleDrive);
        }

        //if nav up, folderId doesn't matter
        void NavigateTo(string folderId, string folderPathName, NavitionType navType, bool directCall)
        {
            if (IsBusy())
                return;

            switch (navType)
            {   
                case NavitionType.LevelDown:
                    if (folderId == null)
                        return;
                    _navState.LevelDown(folderId, folderPathName);
                    break;
                case NavitionType.LevelUp:
                    if (_navState.LevelUp() == null)
                        return;                        
                    break;
            }

            FetchEntries(directCall);
        }

        void FetchEntries(bool directCall)
        {
            if (IsBusy())
                return;
            
            Entries.Clear();
            
            fetchProgress.Visibility = Visibility.Visible;

            var worker = new Action(() =>
            {
                _isBusy = true;
                _cancelled = false;
                _storage.Children(++_folderRequestId,
                                  _navState.CurrentFolderId,
                                   this.Dispatcher,
                                   addEntry);
            });

            if (directCall)
                worker();
            else
                new Task(worker).Start();
        }

        bool addEntry(int expected, FileEntry entry, int folderRequestId)
        {
            if (entry != null)
            {
                _entries.Add(entry);
                numFetched.Text = _entries.Count + " of " + expected + " (" + ((int)(100 * _entries.Count / expected)) + "%)";
            }
            var doContinue = _entries.Count < expected && !_cancelled && folderRequestId == _folderRequestId;
            if (!doContinue)
            {
                _isBusy = false;
                fetchProgress.Visibility = Visibility.Collapsed;
            }
            return doContinue;
        }

        private void SurfaceListBox_SelectionChanged_1(object sender, SelectionChangedEventArgs e)
        {
            if (IsBusy())
                return;
            
            if (e.AddedItems != null && e.AddedItems.Count > 0) 
            {
                var fe = (FileEntry)e.AddedItems[0];
                if (fe.IsFolder)
                {
                    fileList.UnselectAll();
                    NavigateTo(fe.IdString, fe.Title, NavitionType.LevelDown, false);
                }
            }
        }

        private void MainWindow_KeyDown_1(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Back:
                    btnUp_Click_1(null,null);
                    break;
            }
        }

        IEnumerable<FileEntry> SelectedEntries()
        {
            var res = new List<FileEntry>();
            foreach (FileEntry selected in fileList.SelectedItems)
            {
                if (!selected.IsFolder)
                    res.Add(selected);
            }
            return res;             
        }

        bool IsBusy()
        {
            return _isBusy;
        }

        void btnAttach_Click_1(object sender, RoutedEventArgs e)
        {
            var entries = SelectedEntries();
            var tempDir = TempDir(); 

            var downloaded = 0;
            filesToAttach = new List<StorageSelectionEntry>();
            downloadProgress.Visibility = Visibility.Visible;
            foreach(var entry in entries)
            {
                var pathName = System.IO.Path.Combine(tempDir, Guid.NewGuid().ToString() + entry.Title);
                _storage.Download(entry.IdString, 
                                 pathName,
                                 Dispatcher,
                                    ()=>
                                    {
                                        if(File.Exists(pathName))//some files are empty
                                            filesToAttach.Add(new StorageSelectionEntry{ PathName=pathName, Title=entry.Title });

                                        attachmentsProcessed.Text = downloaded + " of " + entries.Count();
                                        if(++downloaded == entries.Count())
                                        {
                                            Close();                        
                                        }
                                    });
            }
        }

        void fileRequestView(object sender, RoutedEventArgs e)
        {
            var fec = e.OriginalSource as FileEntryControl;
            if(fec==null)
                return;

            var fe = fec.DataContext as FileEntry;
            if (fe == null)
                return;

            if (fe.IsFolder)
                return;

            OpenFileViewer(fe);
        }

        void custSelectionEvent(object sender, RoutedEventArgs e)
        {
            var fec = e.OriginalSource as FileEntryControl;
            if (fec == null)
                return;
                
            //invert selection
            if (fileList.SelectedItems.Contains(fec.DataContext))
            {
                if (fileList.SelectionMode == SelectionMode.Multiple)
                    fileList.SelectedItems.Remove(fec.DataContext);
            }
            else
            {
                if (fileList.SelectionMode == SelectionMode.Multiple)
                    fileList.SelectedItems.Add(fec.DataContext);
                else
                    fileList.SelectedItem = fec.DataContext;
            }
        }

        private void btnView_Click_1(object sender, RoutedEventArgs e)
        {
            if (fileViewCallback == null)
                return;

            var entries = SelectedEntries();

            if (entries.Count() != 1)
            {
                MessageBox.Show("Select single file to view");
                return;
            }

            OpenFileViewer(entries.First());
        }

        void OpenFileViewer(FileEntry entry)
        {
            var tempDir = TempDir();

            var pathName = System.IO.Path.Combine(tempDir, Guid.NewGuid().ToString() + entry.Title);
            _storage.Download(entry.IdString,
                              pathName,
                              Dispatcher,
                              () =>
                              {
                                  if (File.Exists(pathName))//some files are empty
                                      fileViewCallback(pathName);
                                  else if (entry.IsGDrive && !string.IsNullOrEmpty(entry.GdocWebUrl))
                                  {                                      
                                      webViewCallback(entry.GdocWebUrl);
                                  }
                              });            
        }

        public static string TempDir()
        {
            string tempPath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "discusys");
            if (!Directory.Exists(tempPath))
                Directory.CreateDirectory(tempPath);
            return tempPath;
        }

        private void btnSortAlphab_Click_1(object sender, RoutedEventArgs e)
        {
            if (IsBusy())
                return;
            btnSortByTime.IsChecked = !btnSortAlphab.IsChecked;
            RefreshSorting();
        }

        private void btnSortByTime_Click_1(object sender, RoutedEventArgs e)
        {
            if (IsBusy())
                return;
            btnSortAlphab.IsChecked = !btnSortByTime.IsChecked;
            RefreshSorting();
        }

        void RefreshSorting()
        {
            if (IsBusy())
                return;

            var fn = (bool)btnSortByTime.IsChecked ? SortingFunction.ByTime : SortingFunction.ByName;
            IOrderedEnumerable<FileEntry> ordered = null;
            switch (fn)
            {
                case SortingFunction.ByName:
                    ordered = Entries.ToArray().OrderBy((FileEntry fe) => fe.Title);
                    break;
                case SortingFunction.ByTime:
                    ordered = Entries.ToArray().OrderByDescending((FileEntry fe) => fe.Modified);
                    break;
            }

            Entries.Clear();
            foreach (var fe in ordered)
            {
                Entries.Add(fe);
            }   
        }

        private void btnRefresh_Click_1(object sender, RoutedEventArgs e)
        {
            if (IsBusy())
                return;
            FetchEntries(false);
        }

        private void btnUp_Click_1(object sender, RoutedEventArgs e)
        {
            if (IsBusy())
                return;
            NavigateTo(null, null, NavitionType.LevelUp, false);
        }

        private void btnRootFolder_Click_1(object sender, RoutedEventArgs e)
        {
            if (IsBusy())
                return;
           
            _navState.Reset(null);
            NavigateTo(_storage.RootFolder(), null, NavitionType.LevelDown, false);
        }

        private void btnCancelFetch_Click_1(object sender, RoutedEventArgs e)
        {                
            _cancelled = true;
        }

        private void btnSelMode_Click_1(object sender, RoutedEventArgs e)
        {
            if ((bool)btnSelMode.IsChecked)
            {
                fileList.SelectionMode = SelectionMode.Multiple;
            }
            else
            {
                fileList.SelectionMode = SelectionMode.Single;
            }
        }
    }
}
