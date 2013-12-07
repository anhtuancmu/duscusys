using System.Data.Objects;
using System.Linq;
using Discussions.ctx;
using Discussions.DbModel;
using LoginEngine;

namespace Discussions
{
    public class SessionInfo
    {
        private static SessionInfo _inst = null;

        public static SessionInfo Get()
        {
            if (_inst == null)
            {
                _inst = new SessionInfo();
            }

            return _inst;
        }

        //public static void Set(SessionInfo val)
        //{
        //    _inst = val;
        //}

        public static void Reset()
        {
            if (_inst != null)
            {
                _inst.setPerson(null);
                _inst.discussion = null;
            }
        }

        public bool ExperimentMode = false;

        public bool ScreenshotMode = false;
        public int screenTopicId = -1;
        public int screenDiscId = -1;
        public string screenMetaInfo = null;

        private Person _person = null;

        public Person person
        {
            get { return getPerson(null); }
        }

        //returns current person taken from the same context as entity is attached to
        public Person getPerson(object entity)
        {
            if (_person == null)
                return null;

            if (entity == null)
                entity = discussion;

            //discussion not set
            if (entity == null)
                return _person;

            if (IsAttachedTo(PrivateCenterCtx.Get(), entity))
                return PrivateCenterCtx.Get().Person.FirstOrDefault(p0 => p0.Id == _person.Id);
            else if (IsAttachedTo(PublicBoardCtx.Get(), entity))
                return PublicBoardCtx.Get().Person.FirstOrDefault(p0 => p0.Id == _person.Id);
            else if (IsAttachedTo(DbCtx.Get(), entity))
                return DbCtx.Get().Person.FirstOrDefault(p0 => p0.Id == _person.Id);

            return _person;
        }

        public void setPerson(Person p)
        {
            _person = p;
        }

        public Discussion discussion;

        public int currentTopicId = -1;

        public static bool IsAttachedTo(ObjectContext context, object entity)
        {
            if (entity == null)
                return false;
            ObjectStateEntry entry;
            if (context.ObjectStateManager.TryGetObjectStateEntry(entity, out entry))
            {
                return true;
            }
            return false;
        }

        public bool IsModerator
        {
            get
            {
                if (_person == null)
                    return false;
                return _person.Name.StartsWith(DaoUtils.MODER_SUBNAME);
            }
        }

        public static void UpdateToNewCtx()
        {
            if (_inst == null)
                return;

            var ctx = PublicBoardCtx.Get();

            if (_inst.discussion != null)
                _inst.discussion = ctx.Discussion.FirstOrDefault(d0 => d0.Id == _inst.discussion.Id);
        }
    }
}