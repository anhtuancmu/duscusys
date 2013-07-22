using System.ComponentModel;
using Discussions.Annotations;
using Discussions.DbModel;

namespace Discussions.model
{
    public sealed class ArgPointExt : INotifyPropertyChanged
    {
        private readonly ArgPoint _ap;

        public ArgPoint Ap
        {
            get { return _ap; }
        }

        public ArgPointExt(ArgPoint ap)
        {
            _ap = ap;
        }

        private int _numUnreadComments;

        public int NumUnreadComments
        {
            get
            {
                return _numUnreadComments;
            }
            set
            {
                if (value != _numUnreadComments)
                {
                    _numUnreadComments = value;
                    //OnPropertyChanged("NumUnreadComments");
                    //OnPropertyChanged("PlusNumUnreadComments"); 
                    OnPropertyChanged(""); 
                }
            }
        }
        
        public string PlusNumUnreadComments
        {
            get
            {
                if (NumUnreadComments>0)
                    return "+" + NumUnreadComments;
                return "    ";
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}