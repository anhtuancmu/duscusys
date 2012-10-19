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
        void Children(string folder,
                      Dispatcher dispatch,
                      Func<int, FileEntry, bool> addEntry);
        string RootFolder();
    }

    public enum StorageType { Dropbox, GoogleDrive };
}
