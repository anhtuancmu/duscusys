using System.Collections.ObjectModel;
using Discussions.DbModel;
using Discussions.model;

namespace Discussions.ViewModel
{
    public class PrivateCenterViewModel
    {
        public PrivateCenterViewModel()
        {
            OwnArgPoints = new ObservableCollection<ArgPointExt>();
            TopicsOfDiscussion = new ObservableCollection<Topic>();
        }

        public ObservableCollection<Topic> TopicsOfDiscussion { get; set; }

        public ObservableCollection<ArgPointExt> OwnArgPoints { get; set; }
    }
}