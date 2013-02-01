using System.ComponentModel;
using Discussions.Annotations;
using Discussions.DbModel;

namespace Discussions.model
{
    public class PersonExt : INotifyPropertyChanged 
    {
        private readonly Person _pers;
        public Person Pers
        {
            get { return _pers; }
        }

        /// <summary>
        /// True if the person has at least one arg.point A in currently selected topic (in private board) 
        /// such that A has at least one comment not read by current (session) person. 
        /// </summary>
        private bool _hasPointsWithUnreadComments;  
        public bool HasPointsWithUnreadComments
        {
            get { return _hasPointsWithUnreadComments; }
            set
            {
                if (value != _hasPointsWithUnreadComments)
                {
                    _hasPointsWithUnreadComments = value;
                    OnPropertyChanged("HasPointsWithUnreadComments");
                }
            }
        }

        public PersonExt(Person pers)
        {
            _pers = pers;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}