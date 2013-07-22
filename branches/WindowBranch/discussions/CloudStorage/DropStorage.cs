using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DropNet;
using DropNet.Exceptions;
using DropNet.Models;
using QuickZip.Tools;

namespace CloudStorage
{
    public class DropStorage : IStorageClient
    {
        private string DROPBOX_APP_KEY = "79fe39q2u2xzgye";
        private string DROPBOX_APP_SECRET = "4fvj2ksros6aimw";

        private DropNetClient _client = null;
        private UserLogin _accessToken = null;

        //folder id to thumb
        private Dictionary<string, BitmapImage> _thumbCache = new Dictionary<string, BitmapImage>();

        private volatile List<TaskCompletionSource<byte[]>> _thumbDownloaders = new List<TaskCompletionSource<byte[]>>();

        private Action<string> _webViewCallback = null;

        public DropStorage(Action<string> webViewCallback)
        {
            _webViewCallback = webViewCallback;

            _client = new DropNetClient(DROPBOX_APP_KEY, DROPBOX_APP_SECRET);

            //Get Request Token 
            _client.GetToken();

            var authUri = _client.BuildAuthorizeUrl();
            //Process.Start(authUri);
            _webViewCallback(authUri);

            //don't need it, web view callback is blocking 
            //var dlg = new AuthDlg(StorageType.Dropbox);
            //dlg.ShowDialog();

            _accessToken = _client.GetAccessToken(); //Store this token for "remember me" function
        }

        public void Download(string file, string saveWhere, Dispatcher dispatch, Action done)
        {
            var shareResp = _client.GetMedia(file);

            using (WebClient webclient = new WebClient())
            {
                webclient.DownloadFileCompleted +=
                    (object sender, AsyncCompletedEventArgs e) =>
                        { dispatch.BeginInvoke(new Action(() => { done(); })); };
                webclient.DownloadFileAsync(new Uri(shareResp.Url), saveWhere);
            }
        }

        public void Children(int folderRequestId,
                             string folder,
                             Dispatcher dispatch,
                             Func<int, FileEntry, int, bool> addEntry)
        {
            var expectedLen = _client.GetMetaData(folder).Contents.Count();

            foreach (var md in _client.GetMetaData(folder).Contents)
            {
                var doContinue = true;
                dispatch.Invoke(new Action(() => { doContinue = addEntry(expectedLen, null, folderRequestId); }));
                if (!doContinue)
                    break;

                if (md.Thumb_Exists)
                {
                    if (_thumbCache.ContainsKey(md.Path))
                    {
                        dispatch.Invoke(
                            new Action(
                                () =>
                                    {
                                        doContinue = addEntry(expectedLen, new FileEntry(md, _thumbCache[md.Path]),
                                                              folderRequestId);
                                    }));
                        if (!doContinue)
                            break;
                    }
                    else
                    {
                        do
                        {
                            lock (_thumbDownloaders)
                            {
                                if (_thumbDownloaders.Count < 12)
                                    break;
                            }

                            System.Threading.Thread.Sleep(100);
                        } while (true);

                        dispatch.Invoke(new Action(() => { doContinue = addEntry(expectedLen, null, folderRequestId); }));
                        if (!doContinue)
                            break;

                        TaskCompletionSource<byte[]> thumbTaskSrc = null;
                        lock (_thumbDownloaders)
                        {
                            thumbTaskSrc = DownloadThumb(md);
                            _thumbDownloaders.Add(thumbTaskSrc);
                        }

                        thumbTaskSrc.Task.ContinueWith(
                            (Task<byte[]> thumbTask) =>
                                {
                                    if (thumbTask.Result != null)
                                    {
                                        dispatch.BeginInvoke(new Action(() =>
                                            {
                                                BitmapImage bmp = new BitmapImage();
                                                bmp.BeginInit();
                                                bmp.StreamSource = new MemoryStream(thumbTask.Result);
                                                bmp.EndInit();

                                                if (!_thumbCache.ContainsKey(md.Path))
                                                    _thumbCache.Add(md.Path, bmp);
                                                addEntry(expectedLen, new FileEntry(md, bmp), folderRequestId);
                                            }));
                                    }
                                    else
                                    {
                                        dispatch.BeginInvoke(
                                            new Action(
                                                () =>
                                                    {
                                                        addEntry(expectedLen, new FileEntry(md, GetFileIcon(md)),
                                                                 folderRequestId);
                                                    }));
                                    }
                                    lock (_thumbDownloaders)
                                        _thumbDownloaders.Remove(thumbTaskSrc);
                                });
                    }
                }
                else
                {
                    dispatch.Invoke(
                        new Action(
                            () =>
                                {
                                    doContinue = addEntry(expectedLen, new FileEntry(md, GetFileIcon(md)),
                                                          folderRequestId);
                                }));
                    if (!doContinue)
                        break;
                }
            }
        }

        private TaskCompletionSource<byte[]> DownloadThumb(MetaData md)
        {
            var tcs = new TaskCompletionSource<byte[]>();

            _client.GetThumbnailAsync(
                md,
                ThumbnailSize.Large,
                (byte[] thumb) => { tcs.SetResult(thumb); },
                (DropboxException) => { tcs.SetResult(null); }
                );

            return tcs;
        }

        private ImageSource GetFileIcon(MetaData md)
        {
            ImageSource src = null;
            if (md.Is_Dir)
            {
                src = (ImageSource) App.Current.FindResource("FolderIcon");
            }
            else
            {
                FileToIconConverter conv = (FileToIconConverter) App.Current.FindResource("iconConv");
                src = conv.GetImage(md.Extension, 64);
            }
            return src;
        }

        public string RootFolder()
        {
            return "/";
        }
    }
}