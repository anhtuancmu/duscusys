using System.Collections.Generic;
using System.Linq;
using Badass.Util;

namespace Badass.Model
{
    public class Project
    {
        /// <summary>
        /// Constructor for deserializer
        /// </summary>
        public Project()
        {            
        }

        public Project(string name)
        {
            SessionTypes = new List<string>();
            SessionNames = new List<string>();
            Sessions = new List<Session>();
            Codes = new List<Code>();
            Subcodes = new List<Subcode>();
            Users = new List<User>();
            Name = name;
        }

        public string Name { get; set; }
        public List<string> SessionTypes { get; set; }
        public List<string> SessionNames { get; set; }
        public List<Subcode> Subcodes { get; set; }
        public List<Code> Codes { get; set; }
        public List<User> Users { get; set; }
        public List<Session> Sessions { get; set; }
        public List<Event> Events { get; set; }

        public void Save(string pathName)
        {
            ObjectSerializerHelper.SerializeObjectToXmlFile(this, pathName);
        }

        void FixupReferences()
        {
            //available subcodes in codes
            foreach (var code in Codes)
                code.FixupReferences();

            //checked subcodes in events 
           // foreach (var session in Sessions)   
        }

        public static Project Load(string pathName)
        {
            var result = ObjectSerializerHelper.DeSerializeObjectFromFile<Project>(pathName);
            result.FixupReferences();

            return result;
        }

        public Subcode GetSubcode(string name)
        {
            return Subcodes.FirstOrDefault(s => s.Name == name);
        }
        public Code GetCode(string name)
        {
            return Codes.FirstOrDefault(s => s.Name == name);
        }
        public User GetUser(int userId)
        {
            return Users.FirstOrDefault(u => u.Id == userId);
        }
        public Session GetSession(string name, string type)
        {
            return Sessions
                .FirstOrDefault(s => s.Name == name && s.Type == type);
        }

        public bool AddSessionName(string name)
        {
            if (SessionNames.Contains(name))
                return false;

            SessionNames.Add(name);

            //generate sessions with the new name 
            foreach (var sessionType in SessionTypes)
            {
                Sessions.Add(new Session(name, sessionType));                    
            }            
            
            return true;
        }

        public bool RemoveSessionType(string sessionType)
        {
            if (!SessionTypes.Contains(sessionType))
                return false;

            //remove sessions with the given type
            foreach (var s in Sessions.Where(s => s.Type == sessionType).ToArray())
            {
                Sessions.Remove(s);
            }

            SessionTypes.Remove(sessionType);

            return true;
        }

        public bool AddSessionType(string sessionType)
        {
            if (SessionTypes.Contains(sessionType))
                return false;

            SessionTypes.Add(sessionType);

            //generate sessions with the new type 
            foreach (var name in SessionNames)
            {
                Sessions.Add(new Session(name, sessionType));
            }

            return true;
        }

        public bool RemoveSessionName(string sessionName)
        {
            if (!SessionNames.Contains(sessionName))
                return false;

            //remove sessions with the given type
            foreach (var s in Sessions.Where(s => s.Name == sessionName).ToArray())
            {
                Sessions.Remove(s);
            }

            SessionNames.Remove(sessionName);

            return true;
        }

        public bool AddSubcode(string name)
        {
            if (Subcodes.FirstOrDefault(sc => sc.Name == name) != null)
                return false;

            Subcodes.Add(new Subcode(name));
            return true;
        }

        public bool RemoveSubcode(string name)
        {
            var toRemove = Subcodes.FirstOrDefault(sc => sc.Name == name);
            if (toRemove == null)
                return false;

            Subcodes.Remove(toRemove);

            //delete the subcode from all events   
            foreach (var session in Sessions)
                foreach (var ev in session.Events)
                    ev.RemoveSubcode(toRemove);

            return true;
        }

        public bool AddCode(string name)
        {
            if (Codes.FirstOrDefault(c => c.Name == name) != null)
                return false;

            Codes.Add(new Code(name));
            return true;
        }
        public bool RemoveCode(string name)
        {
            var toRemove = Codes.FirstOrDefault(c => c.Name == name);
            if (toRemove == null)
                return false;

            Codes.Remove(toRemove);

            //delete the code from all events   
            foreach (var session in Sessions)
            {
                var eventsToRemove = session.Events
                    .Where(ev => ev.Code == toRemove.Name)
                    .ToArray();
                
                foreach (var ev in eventsToRemove)
                    session.RemoveEvent(ev);
            }

            return true;
        }

        public bool AddUser(User user)
        {
            if (Users.FirstOrDefault(u => u.Id == user.Id) != null)
                return false;

            Users.Add(user);

            return true;
        }

        public bool RemoveUser(User user)
        {
            if (Users.FirstOrDefault(u => u.Id == user.Id) == null)
                return false;

            Users.Remove(user);

            //remove all events of the user
            foreach (var session in Sessions)
                foreach (var ev in session.Events.Where(ev=>ev.UserId == user.Id).ToArray())
                    session.RemoveEvent(ev);
                    
            return true;
        }


    }
}