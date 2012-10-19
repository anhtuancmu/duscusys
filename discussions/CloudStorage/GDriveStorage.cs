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
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using DotNetOpenAuth.OAuth2;
using Google.Apis.Authentication;
using Google.Apis.Authentication.OAuth2;
using Google.Apis.Authentication.OAuth2.DotNetOpenAuth;
using Google.Apis.Drive.v2;
using Google.Apis.Drive.v2.Data;
using Google.Apis.Util;
using QuickZip.Tools;

namespace CloudStorage
{
    public class GDriveStorage : IStorageClient
    {
        string GOOGLE_DRIVE_CLIENT_ID = "579103823915.apps.googleusercontent.com";
        string GOOGLE_DRIVE_CLIENT_SECRET = "7RxhbcWlt8qfw1Siy58D4xl8";

        Dictionary<string, BitmapImage> _thumbCache = new Dictionary<string, BitmapImage>();

        NativeApplicationClient _nativeAppClient = null;
        IAuthorizationState _state = null;
        DriveService _service = null;

        Action<string> _webViewCallback = null;

        public GDriveStorage(Action<string> webViewCallback)
        {            
            _webViewCallback = webViewCallback;

            // Register the authenticator and create the service
            _nativeAppClient = new NativeApplicationClient(GoogleAuthenticationServer.Description,
                                                           GOOGLE_DRIVE_CLIENT_ID,
                                                           GOOGLE_DRIVE_CLIENT_SECRET);
            var auth = new OAuth2Authenticator<NativeApplicationClient>(_nativeAppClient, GetAuthorization);
            _service = new DriveService(auth);            
        }

        private IAuthorizationState GetAuthorization(NativeApplicationClient arg)
        {
            // Get the auth URL:
            _state = new AuthorizationState(new[] { DriveService.Scopes.Drive.GetStringValue() });
            _state.Callback = new Uri(NativeApplicationClient.OutOfBandCallbackUrl);
            Uri authUri = arg.RequestUserAuthorization(_state);

            // Request authorization from the user (by opening a browser window):
            //Process.Start(authUri.ToString());    
            _webViewCallback(authUri.ToString());

            //show Login UI
            var dlg = new AuthDlg(StorageType.GoogleDrive);
            dlg.ShowDialog();            
           
            // Retrieve the access token by using the authorization code:
            return arg.ProcessUserAuthorization(dlg.AuthCode, _state);
        }

        public void Download(string file, string saveWhere, Dispatcher dispatch, Action done)
        {
            _service.Files.Get(file).FetchAsync(
                (LazyResult<Google.Apis.Drive.v2.Data.File> response) =>
                {
                    Google.Apis.Drive.v2.Data.File fileRes = response.GetResult();
                  
                    var stream = DownloadFile(_service.Authenticator, fileRes);
                    if (stream != null)
                    {
                        using (var fs = new FileStream(saveWhere, FileMode.Create))
                        {
                            stream.CopyTo(fs);
                            dispatch.BeginInvoke(new Action(() => { done(); }));
                        }
                    }
                    else
                    {
                        dispatch.BeginInvoke(new Action(() => { done(); }));   
                    }
                }
            );
        }

        System.IO.Stream DownloadFile(IAuthenticator authenticator,  Google.Apis.Drive.v2.Data.File file)
        {
            if (!String.IsNullOrEmpty(file.DownloadUrl))
            {
                try
                {
                    HttpWebRequest request = (HttpWebRequest)WebRequest.Create(new Uri(file.DownloadUrl));
                    authenticator.ApplyAuthenticationToRequest(request);
                    HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                    if (response.StatusCode == HttpStatusCode.OK)
                    {
                        return response.GetResponseStream();
                    }
                    else
                    {
                        Console.WriteLine(
                            "An error occurred: " + response.StatusDescription);
                        return null;
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    return null;
                }
            }
            else
            {
                // The file doesn't have any content stored on Drive.
                return null;
            }
        }

        public void Children(string folder,
                             Dispatcher dispatch,
                             Func<int, FileEntry, bool> addEntry)
        {
            ChildrenResource.ListRequest request = _service.Children.List(folder);
            do
            {
                try
                {
                    ChildList children = request.Fetch();

                    var expectedCount = children.Items.Count();

                    foreach (ChildReference child in children.Items)
                    {
                        var doContinue = true;
                        dispatch.Invoke(new Action(() =>
                        {
                            doContinue = addEntry(expectedCount, null);
                        }));
                        if (!doContinue)
                           break;                                                                                                    

                        FilesResource.GetRequest getReq = _service.Files.Get(child.Id);
                        getReq.FetchAsync((LazyResult<Google.Apis.Drive.v2.Data.File> response) =>
                        {
                            Google.Apis.Drive.v2.Data.File file = response.GetResult();
                            
                            doContinue = true;
                            dispatch.Invoke(new Action(() =>
                            {
                                doContinue = addEntry(expectedCount, null);
                            }));
                            if (!doContinue)
                            {
                                return;
                            }

                            //download thumbnail
                            if (file.ThumbnailLink != null)
                            {
                                if (_thumbCache.ContainsKey(file.Id))
                                {
                                    dispatch.BeginInvoke(new Action(() =>
                                    {
                                        addEntry(expectedCount, new FileEntry(file, _thumbCache[file.Id]));                                      
                                    }));
                                }
                                else
                                {
                                    try
                                    {
                                        using (WebClient client = new WebClient())
                                        {
                                            // Download data
                                            byte[] thumbData = client.DownloadData(file.ThumbnailLink);

                                            dispatch.BeginInvoke(new Action(() =>
                                            {
                                                var thumb = new BitmapImage();
                                                thumb.BeginInit();
                                                thumb.StreamSource = new MemoryStream(thumbData);
                                                thumb.EndInit();

                                                if(!_thumbCache.ContainsKey(file.Id))
                                                    _thumbCache.Add(file.Id, thumb);

                                                addEntry(expectedCount, new FileEntry(file, thumb));                                                                                               
                                            }));
                                        }
                                    }
                                    catch(Exception)
                                    {
                                        //thumbnails may be absent
                                        dispatch.BeginInvoke(new Action(() =>
                                        {
                                            addEntry(expectedCount, new FileEntry(file, GetFileIcon(file)));                               
                                        }));
                                    }
                                }
                            }
                            else
                            {
                                dispatch.BeginInvoke(new Action(() =>
                                {
                                    addEntry(expectedCount, new FileEntry(file, GetFileIcon(file)));                                      
                                }));
                            }
                        });
                    }
                    request.PageToken = children.NextPageToken;
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occurred: " + e.Message);
                    request.PageToken = null; 
                }

                var doContinue2 = true;
                dispatch.Invoke(new Action(() =>
                {
                    doContinue2 = addEntry(1, null);//1 anything != 0
                }));
                if (!doContinue2)
                    break;
            } while (!String.IsNullOrEmpty(request.PageToken));
        }

        ImageSource GetFileIcon(Google.Apis.Drive.v2.Data.File file)
        {
            ImageSource src = null;

            if(IsFolder(file))
            {
                src = (ImageSource)App.Current.FindResource("FolderIcon");
            }
            else
            {
                if (file.FileExtension == null)
                {
                    file.FileExtension = "";
                    var lastDot = file.Title.LastIndexOf(".");
                    if (lastDot != -1)
                        file.FileExtension = file.Title.Substring(lastDot, file.Title.Length - lastDot);
                }

                FileToIconConverter conv = (FileToIconConverter)App.Current.FindResource("iconConv");
                src = conv.GetImage(file.FileExtension, 64);
            }
            return src;
        }

        public static bool IsFolder(Google.Apis.Drive.v2.Data.File file)
        {
            const string FOLDER_MIME_TYPE = "application/vnd.google-apps.folder";
            return file.MimeType == FOLDER_MIME_TYPE;
        }

        public string RootFolder()
        {
            return "root";
        }
    }
}
