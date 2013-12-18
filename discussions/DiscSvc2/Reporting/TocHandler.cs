using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using Discussions;
using Discussions.DbModel;

namespace DiscSvc.Reporting
{
    public class TocHandler : IHttpHandler
    {
        /// <summary>
        /// You will need to configure this handler in the Web.config file of your 
        /// web and register it with IIS before being able to use it. For more information
        /// see the following link: http://go.microsoft.com/?linkid=8101007
        /// </summary>
        #region IHttpHandler Members

        public bool IsReusable
        {
            // Return false in case your Managed Handler cannot be reused for another request.
            // Usually this would be false in case you have some state information preserved per request.
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            var toc = new Toc { TocRows = GetTopicList() };
            context.Response.Write(toc.TransformText());
        }

        #endregion

        List<TocTableRow> GetTopicList()
        {
            var topics = new List<TocTableRow>();

            var enumeratedTopics = new List<Topic>();

            var ctx = new DiscCtx(ConfigManager.ConnStr);
            foreach(var session in ctx.Session.ToArray())
            {
                foreach (var person in session.Person.ToArray())
                {
                    foreach (var topic in person.Topic)
                    {
                        if (enumeratedTopics.FirstOrDefault(t => t.Id == topic.Id) == null)
                        {
                            enumeratedTopics.Add(topic);

                            var row = new TocTableRow
                            {
                                Date = session.EstimatedDateTime.ToString(),
                                Discussion = topic.Discussion.Subject,
                                Report = ReportUrl(ConfigManager.ServiceServer,
                                    topic.Discussion.Id,
                                    topic.Id,
                                    session.Id),
                                Session = session.Name,
                                Topic = topic.Name,
                                Participants = BuildParticipantsString(topic)
                            };
                            topics.Add(row);
                        }
                    }
                }
            }

            return topics;
        }

        string BuildParticipantsString(Topic topic)
        {
            var topicParticipants = topic.Person.Select(p => p.Name).ToArray();
            var sb = new StringBuilder();
            foreach (var topicParticipant in topicParticipants)
            {
                sb.Append(topicParticipant);
                if (topicParticipant != topicParticipants.Last())
                    sb.AppendLine(",");
            }
            return sb.ToString();
        }

        static string ReportUrl(string svcServer, int discussionId, int topicId, int sessionId)
        {
            return string.Format("http://{0}/discsvc/report?discussionId={1}&topicId={2}&sessionId={3}", 
                                 svcServer, discussionId, topicId, sessionId);
        }
    }
}
