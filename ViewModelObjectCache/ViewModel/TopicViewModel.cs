using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Discussions.Annotations;

namespace Discussions.ViewModel
{
    public class TopicViewModel : INotifyPropertyChanged
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public byte[] Annotation { get; set; }

        public bool Running { get; set; }

        public int CumulativeDuration { get; set; }

        public ObservableCollection<ArgPointViewModel> ArgPoints { get; set; }
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}