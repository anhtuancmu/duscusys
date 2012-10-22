using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace CloudStorage
{
    interface IStorageClient
    {
        void Download(string file, string saveWhere, Dispatcher dispatch, Action done);
        void Children(int folderRequestId,
                      string folder,
                      Dispatcher dispatch,
                      Func<int, FileEntry, int, bool> addEntry);
        string RootFolder();
    }

    public enum StorageType { Dropbox, GoogleDrive };
}
