using System;
using System.Windows.Threading;

namespace CloudStorage.Model
{
    internal interface IStorageClient
    {
        void Download(string file, string saveWhere, Dispatcher dispatch, Action done);

        void Children(int folderRequestId,
                      string folder,
                      Dispatcher dispatch,
                      Func<int, FileEntry, int, bool> addEntry);

        string RootFolder();
    }
}