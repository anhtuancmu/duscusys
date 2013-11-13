using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Discussions.TdsSvcRef;

namespace Discussions.ViewModel
{
    public class ArgPointViewModel : INotifyPropertyChanged
    {
        public int Id { get; set; }

        public string Point { get; set; }

        public int SideCode { get; set; }

        public bool SharedToPublic { get; set; }

        public string RecentlyEnteredSource { get; set; }

        public string RecentlyEnteredMediaUrl { get; set; }

        public bool ChangesPending { get; set; }

        public int OrderNumber { get; set; }

        public bool ChildrenLoaded { get; set; }

        private readonly ObservableCollection<SourceViewModel> _sources =
            new ObservableCollection<SourceViewModel>();
        public ObservableCollection<SourceViewModel> Sources
        {
            get { return _sources; }
        }



        private readonly ObservableCollection<AttachmentViewModel> _attachments =
            new ObservableCollection<AttachmentViewModel>();
        public ObservableCollection<AttachmentViewModel> Attachments
        {
            get { return _attachments; }
        }

        public ArgPointViewModel()
        {            
        }

        public ArgPointViewModel(SArgPoint argPoint)
        {
            Id = argPoint.Id;
            OrderNumber = argPoint.OrderNumber;
            ChangesPending = false;
            SharedToPublic = argPoint.SharedToPublic;
            Point = argPoint.Point;
            SideCode = argPoint.SideCode;
            RecentlyEnteredSource = argPoint.RecentlyEnteredSource;
            RecentlyEnteredMediaUrl = argPoint.RecentlyEnteredMediaUrl;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
