using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.DbModel;
using System.Data.Objects;
using System.Data;

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

        Person _person = null;

        public Person person
        {
            get
            {
                return getPerson(null);
            }
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

            if(IsAttachedTo(Ctx2.Get(),entity))
                return Ctx2.Get().Person.FirstOrDefault(p0=>p0.Id==_person.Id);
            else if (IsAttachedTo(CtxSingleton.Get(), entity))
                return CtxSingleton.Get().Person.FirstOrDefault(p0 => p0.Id == _person.Id);
            else
                return _person;
        }        

        public void setPerson(Person p)
        {
            _person = p;
        }

        public Discussion discussion;

        public int currentTopicId = -1;

        public static bool IsAttachedTo( ObjectContext context, object entity)
        {
            if (entity == null)
                return false;
            ObjectStateEntry entry;
            if (context.ObjectStateManager.TryGetObjectStateEntry(entity, out entry))
            {
                return (entry.State != EntityState.Detached);
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
            if(_inst==null)
                return;
            
            var ctx = CtxSingleton.Get();

            if (_inst.discussion != null)
                _inst.discussion = ctx.Discussion.FirstOrDefault(d0 => d0.Id == _inst.discussion.Id);
        }
    }
}
