using System.Collections.Generic;
using System.Linq;
using Discussions.DbModel;

namespace Reporter
{
    public class NewSessionTopicDialogVm
    {
        public NewSessionTopicDialogVm()
        {
            SelectedReportTargets = new List<ReportParameters>();
            
            GenerateReportTargets();            
        }

        void GenerateReportTargets()
        {
            ReportTargets = new List<ReportParameters>();

            var ctx = new DiscCtx(Discussions.ConfigManager.ConnStr);

            var sessions = ctx.Session.ToList();

            var sessionIdToPersons = new Dictionary<int, List<Person>>();

            foreach (var session in sessions)
            {
                List<Person> sessionPersons = GetPersonsOfSession(session);
                sessionIdToPersons.Add(session.Id, sessionPersons);
            }

            var allTopics = new List<Topic>();

            var usersOfAllSessions = new List<Person>();
            foreach (var session in sessions)
            {
                //get users of current session             
                foreach (var person in session.Person)
                {
                    if (usersOfAllSessions.All(u => u.Id != person.Id))
                        usersOfAllSessions.Add(person);

                    foreach (var persTopic in person.Topic)
                    {
                        if (allTopics.All(t => t.Id != persTopic.Id))
                        {
                            allTopics.Add(persTopic);

                            var reportParams = new ReportParameters(
                                sessionIdToPersons[session.Id].Select(p => p.Id).ToList(),
                                session,
                                persTopic,
                                persTopic.Discussion
                                );

                            ReportTargets.Add(reportParams);
                        }
                    }
                }
            }
        }

        static List<Person> GetPersonsOfSession(Session session)
        {
            var persons = new List<Person>();
            foreach (var person in session.Person)
            {
                if (persons.All(u => u.Id != person.Id))
                    persons.Add(person);
            }
            return persons;
        }

        //static void GetTopicsOfUsers(List<Person> users, List<Topic> topics)
        //{
        //    foreach (var person in users)
        //    {
        //        foreach (var persTopic in person.Topic)
        //        {
        //            if(topics.All(t => t.Id != persTopic.Id))
        //                topics.Add(persTopic);
        //        }
        //    }
        //}

        public List<ReportParameters> ReportTargets { get; set; }

        public List<ReportParameters> SelectedReportTargets { get; set; }
    }
}