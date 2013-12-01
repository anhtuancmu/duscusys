using Discussions.TdsSvcRef;
using Discussions.ViewModel;

namespace Discussions
{
    public class ScreenshotTask
    {
        public int TopicId = -1;
        public int DiscId = -1;
        public string MetaInfo = null;
    }


    public class SessionInfo
    {
        private static SessionInfo _inst;
        public static SessionInfo Get()
        {
            if (_inst == null)
                _inst = new SessionInfo();

            return _inst;
        }

        public bool ExperimentMode { get; set; }
        public ScreenshotTask ScreenshotTask { get; set; }
        public SPerson Person { get; set; }
        public DiscussionViewModel Discussion { get; set; }
        public bool IsModerator
        {
            get
            {
                if (Person == null)
                    return false;

                const string moderSubname = "moder";
                return Person.Name.StartsWith(moderSubname);
            }
        }
    }
}