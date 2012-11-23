using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.DbModel;
using Discussions.model;
using Discussions.rt;
using Discussions.RTModel.Model;
using System.Collections.ObjectModel;

namespace Reporter.pdf
{
    class DaoUtils
    {
        public static IEnumerable<Person> Participants(Topic topic, Session session)
        {
            var topId = topic.Id;            
            return session.Person.Where(p=>p.Topic.Any(t=>t.Id==topId));
        }
    }
}
