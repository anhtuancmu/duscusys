using System;
using System.Windows.Media;
using DropNet.Models;

namespace CloudStorage.Model
{
    public class FileEntry
    {
        public DateTime Modified { get; set; }
        public string Title { get; set; }
        public ImageSource Image { get; set; }
        public string IdString { get; set; }
        public bool IsFolder { get; set; }
        public string GdocWebUrl { get; set; }
        public bool IsGDrive { get; set; } //true iif it's gdrive file

        public FileEntry(DateTime modified, string title, ImageSource img, string idString, bool isFolder)
        {
            Modified = modified;
            Title = title;
            Image = img;
            IdString = idString;
            IsFolder = isFolder;
        }

        public FileEntry(MetaData md, ImageSource thumb)
        {
            Modified = md.ModifiedDate;
            Title = md.Name;
            Image = thumb;
            IdString = md.Path;
            IsFolder = md.Is_Dir;
            IsGDrive = false;
        }

        public FileEntry(Google.Apis.Drive.v2.Data.File file, ImageSource thumb, string gdocWebUrl)
        {
            Modified = DateTime.Parse(file.ModifiedDate);
            Title = file.Title;
            Image = thumb;
            IdString = file.Id;
            IsFolder = GDriveStorage.IsFolder(file);
            GdocWebUrl = gdocWebUrl;
            IsGDrive = true;
        }
    }
}