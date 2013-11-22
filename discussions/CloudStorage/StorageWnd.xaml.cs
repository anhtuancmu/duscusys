using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;
using AbstractionLayer;
using CloudStorage.Model;
using Discussions;

namespace CloudStorage
{
    public partial class StorageWnd : PortableWindow
    {
        //file entries of currently viewed level
        private ObservableCollection<FileEntry> _entries = new ObservableCollection<FileEntry>();

        public ObservableCollection<FileEntry> Entries
        {
            get { return _entries; }
            set { _entries = value; }
        }

        private NavState _navState = new NavState();

        public NavState NavState
        {
            get { return _navState; }
            set { _navState = value; }
        }

        private IStorageClient _storage = null;

        private bool _isBusy = false;
        private bool _cancelled = false;
        private int _folderRequestId = 0;

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
            if (IsBusy() || storageType==StorageType.Undefined)
                return;

            progressInfo.Visibility = Visibility.Visible;
            Entries.Clear(); // need to clear here for the case if authentication throws exception

            Action worker = () =>
                {
                    try
                    {
                        switch (storageType)
                        {
                            case StorageType.Dropbox:
                                _storage = new DropStorage(webViewCallback);
                                _navState.Reset("Dropbox");
                                break;
                            case StorageType.GDrive:
                                _storage = new GDriveStorage(webViewCallback);
                                _navState.Reset("Google Drive");
                                break;
                            default:
                                throw new NotSupportedException();
                        }

                        NavigateTo(_storage.RootFolder(), null, NavigationDirection.LevelDown, true);
                    }
                    catch (Exception e)
                    {
                        downloadProgress.Visibility = Visibility.Collapsed;
                        MessageDlg.Show("Authentication error. Please try to connect again: \n" + e.ToString(), "Error",
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
            LoginAndEnumFiles(StorageType.GDrive);
        }

        //if nav up, folderId doesn't matter
        private void NavigateTo(string folderId, string folderPathName, NavigationDirection navDirection, bool directCall)
        {
            if (IsBusy())
                return;

            switch (navDirection)
            {
                case NavigationDirection.LevelDown:
                    if (folderId == null)
                        return;
                    _navState.LevelDown(folderId, folderPathName);
                    break;
                case NavigationDirection.LevelUp:
                    if (_navState.LevelUp() == null)
                        return;
                    break;
            }

            FetchEntries(directCall);
        }

        private void FetchEntries(bool directCall)
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
                                      AddEntry);
                });

            if (directCall)
                worker();
            else
                new Task(worker).Start();
        }

        private bool AddEntry(int expected, FileEntry entry, int folderRequestId)
        {
            if (entry != null)
            {
                _entries.Add(entry);
                numFetched.Text = _entries.Count + " of " + expected + " (" + ((int) (100*_entries.Count/expected)) +
                                  "%)";
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
                var fe = (FileEntry) e.AddedItems[0];
                if (fe.IsFolder)
                {
                    fileList.UnselectAll();
                    NavigateTo(fe.IdString, fe.Title, NavigationDirection.LevelDown, false);
                }
            }
        }

        private void MainWindow_KeyDown_1(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Back:
                    btnUp_Click_1(null, null);
                    break;
            }
        }

        private IEnumerable<FileEntry> SelectedEntries()
        {
            return fileList.SelectedItems.Cast<FileEntry>().Where(selected => !selected.IsFolder).ToList();
        }

        private bool IsBusy()
        {
            return _isBusy;
        }

        private void btnAttach_Click_1(object sender, RoutedEventArgs e)
        {
            var entries = SelectedEntries();
            var tempDir = TempDir();

            var downloaded = 0;
            filesToAttach = new List<StorageSelectionEntry>();
            downloadProgress.Visibility = Visibility.Visible;
            foreach (var entry in entries)
            {
                var pathName = System.IO.Path.Combine(tempDir, Guid.NewGuid().ToString() + entry.Title);
                _storage.Download(entry.IdString,
                                  pathName,
                                  Dispatcher,
                                  () =>
                                      {
                                          if (File.Exists(pathName)) //some files are empty
                                              filesToAttach.Add(new StorageSelectionEntry
                                                  {
                                                      PathName = pathName,
                                                      Title = entry.Title
                                                  });

                                          attachmentsProcessed.Text = downloaded + " of " + entries.Count();
                                          if (++downloaded == entries.Count())
                                          {
                                              Close();
                                          }
                                      });
            }
        }

        private void FileRequestView(object sender, RoutedEventArgs e)
        {
            var fec = e.OriginalSource as FileEntryControl;
            if (fec == null)
                return;

            var fe = fec.DataContext as FileEntry;
            if (fe == null)
                return;

            if (fe.IsFolder)
                return;

            OpenFileViewer(fe);
        }

        private void CustSelectionEvent(object sender, RoutedEventArgs e)
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
                MessageDlg.Show("Select single file to view");
                return;
            }

            OpenFileViewer(entries.First());
        }

        private void OpenFileViewer(FileEntry entry)
        {
            var tempDir = TempDir();

            var pathName = System.IO.Path.Combine(tempDir, Guid.NewGuid().ToString() + entry.Title);
            _storage.Download(entry.IdString,
                              pathName,
                              Dispatcher,
                              () =>
                                  {
                                      if (File.Exists(pathName)) //some files are empty
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

        private void RefreshSorting()
        {
            if (IsBusy())
                return;

            var fn = (bool) btnSortByTime.IsChecked ? SortingFunction.ByTime : SortingFunction.ByName;
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
            FetchEntries(false);
        }

        private void btnUp_Click_1(object sender, RoutedEventArgs e)
        {
            if (IsBusy())
                return;
            NavigateTo(null, null, NavigationDirection.LevelUp, false);
        }

        private void btnRootFolder_Click_1(object sender, RoutedEventArgs e)
        {
            if (IsBusy())
                return;

            _navState.Reset(null);
            NavigateTo(_storage.RootFolder(), null, NavigationDirection.LevelDown, false);
        }

        private void btnCancelFetch_Click_1(object sender, RoutedEventArgs e)
        {
            _cancelled = true;
        }     
    }
}