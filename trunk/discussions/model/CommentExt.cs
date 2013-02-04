using System.ComponentModel;
using Discussions.Annotations;
using Discussions.DbModel;

namespace Discussions.model
{
    public class CommentExt : INotifyPropertyChanged
    {
        private readonly Comment _comment;

        public CommentExt(Comment comment)
        {
            _comment = comment;
        }

        public Comment Comment
        {
            get { return _comment; }           
        }

        //true if the comment is not read by current (session) user
        public bool IsNew
        {
            get { return _isNew; }
            set
            {
                if (value==_isNew) return;
                _isNew = value;
                OnPropertyChanged("IsNew");
            }
        }
        private bool _isNew = false; 

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}