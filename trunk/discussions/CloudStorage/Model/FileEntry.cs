using System;
using System.Windows.Media;
using DropNet.Models;
using System.Linq;

namespace CloudStorage.Model
{
    public class FileEntry
    {
        public DateTime Modified { get; set; }
        public string Title { get; set; }
        public ImageSource Image { get; set; }
        public string IdString { get; set; }
        public bool IsFolder { get; set; }
        public bool IsViewable { get; set; }
        public string GdocWebUrl { get; set; }
        public bool IsGDrive { get; set; } //true iif it's gdrive file

        public FileEntry(DateTime modified, string title, ImageSource img, string idString, bool isFolder, bool isViewable)
        {
            Modified = modified;
            Title = title;
            Image = img;
            IdString = idString;
            IsFolder = isFolder;
            IsViewable = isViewable;
        }

        public FileEntry(MetaData md, ImageSource thumb)
        {
            Modified = md.ModifiedDate;
            Title = md.Name;
            Image = thumb;
            IdString = md.Path;
            IsFolder = md.Is_Dir;
            IsGDrive = false;
            IsViewable = !md.Is_Dir && IsExtViewable(md.Extension.Replace(".", ""));
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
            IsViewable = !IsFolder && (IsExtViewable(file.FileExtension) || 
                                       !string.IsNullOrWhiteSpace(GdocWebUrl));
        }

        static readonly string[] _images = new string[] { "jpg", "jpeg", "bmp", "png" };
        static readonly string[] _word = new string[] { "docx", "docm", "dotx", "dotm", ".doc", "rtf", "odt" };
        static readonly string[] _excel = new string[] { "xlsx", "xlsm", "xlsb", "xltx", ".xltm", "xls", "xlt" };
        static readonly string[] _powerPoint = new string[] { "pptx", "ppt", "pptm", "ppsx", ".pps", "potx", "pot", "potm", "odp" };
        public static bool IsExtViewable(string ext)
        {
            if (ext == null)
                return false;

            ext = ext.ToLower();

            if (ext == "txt")
                return true;

            if (_images.Contains(ext))
                return true;

            if (_word.Contains(ext))
                return true;

            if (_excel.Contains(ext))
                return true;

            if (_powerPoint.Contains(ext))
                return true;

            return false;
        }
    }
}