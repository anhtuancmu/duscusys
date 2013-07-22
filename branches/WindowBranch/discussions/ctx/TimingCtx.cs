using Discussions.DbModel;

namespace Discussions.ctx
{
    //only to fetch comment notifications 
    class TimingCtx : DiscCtx
    {
        private static readonly DuplicateEventRecognizer _dup = new DuplicateEventRecognizer(200);

        private static DiscCtx _cached;

        public static DiscCtx GetFresh()
        {            
            if (_cached==null || !_dup.IsDuplicate())
            {
                _cached = new DiscCtx(ConfigManager.ConnStr);               
            }

            _dup.RecordEvent();

            return _cached;
        }

        public static void Drop()
        {
            if (_cached != null)
            {
                _cached.Dispose();
                _cached = null;
            }
        }
    }
}
