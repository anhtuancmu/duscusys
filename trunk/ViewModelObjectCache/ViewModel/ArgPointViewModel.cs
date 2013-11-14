using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using Discussions.TdsSvcRef;

namespace Discussions.ViewModel
{
    public class ArgPointViewModel : INotifyPropertyChanged
    {
        public ArgPointViewModel()
        {
        }

        public ArgPointViewModel(SArgPoint argPoint)
        {
            Id = argPoint.Id;
            PersonId = argPoint.PersonId;
            Description = argPoint.Description;
            OrderNumber = argPoint.OrderNumber;
            ChangesPending = false;
            SharedToPublic = argPoint.SharedToPublic;
            Point = argPoint.Point;
            SideCode = argPoint.SideCode;
            RecentlyEnteredSource = argPoint.RecentlyEnteredSource;
            RecentlyEnteredMediaUrl = argPoint.RecentlyEnteredMediaUrl;
            NumUnreadComments = argPoint.NumUnreadComments;
        }

        public int Id { get; set; }

        public string Point { get; set; }

        public int SideCode { get; set; }

        public bool SharedToPublic { get; set; }

        public string RecentlyEnteredSource { get; set; }

        public string RecentlyEnteredMediaUrl { get; set; }

        public bool ChangesPending { get; set; }

        public int OrderNumber { get; set; }

        public int PersonId { get; set; }

        public string Description { get; set; }

        public bool ChildrenLoaded { get; set; }

        public bool CanEdit
        {
            get
            {
                if (SessionInfo.Get().person == null)
                    return false;
                return SessionInfo.Get().person.Id == PersonId;
            }
        }

        private int _numUnreadComments;
        public int NumUnreadComments
        {
            get { return _numUnreadComments; }
            set
            {
                if (value == _numUnreadComments)
                    return;
                
                _numUnreadComments = value;
                OnPropertyChanged();
                OnPropertyChanged("HasDot");
            }
        }

        public bool HasDot
        {
            get
            {
                return NumUnreadComments > 0;
            }
        }

        private string _commentReadInfo;
        public string CommentReadInfo
        {
            get { return _commentReadInfo; }
            set
            {
                if (value == _commentReadInfo)
                    return;
                
                _commentReadInfo = value;
                OnPropertyChanged();
            }
        }


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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        public void HandleCommentRead(CommentsReadEvent ev)
        {
            if (ev.ArgPointId != Id)
                return;

            if (ev.PersonId == SessionInfo.Get().person.Id)
            {
                 App.Service.Proxy.UpdateLocalReadCounts(this);
            }
            else
            {
                SCommentReadInfo readInfo = App.Service.Proxy.GetCommentReadInfo(ev.CommentId, SessionInfo.Get().person.Id);
                CommentReadInfo = UpdateRemoteReadCounts(readInfo);
            }   
        }

        public static string UpdateRemoteReadCounts(SCommentReadInfo readInfo)
        {
            if (readInfo.Comment == null)
                return "";

            if(readInfo.EveryoneInTopicRead)
                return string.Format("\"{0}\" seen by all", SummaryTextConvertor.ShortenLine(readInfo.Comment.Text, 15));

            var res = new StringBuilder(string.Format("\"{0}\" seen by ",
                                            SummaryTextConvertor.ShortenLine(readInfo.Comment.Text, 15)
                                           )
                                       );
            var atLeastOneReader = false;

            foreach (var pers in readInfo.PersonsWhoRead)
            {
                if (atLeastOneReader)
                {
                    res.Append(", ");
                }
                atLeastOneReader = true;

                res.Append(pers.Name);
            }

            if (atLeastOneReader)
                return res.ToString();

            return "";
        }
    }
}
