using System.Linq;
using System.Web;
using Discussions.DbModel;

namespace DiscSvc.Reporting
{
    public class QueryParams
    {
        public int DiscussionId;
        public int TopicId;
        public int SessionId; 

        //throws if query parameters are incorrect
        public QueryParams(HttpRequest request)
        {            
            DiscussionId = int.Parse(request.QueryString["discussionId"]);
            TopicId = int.Parse(request.QueryString["topicId"]);
            SessionId = int.Parse(request.QueryString["sessionId"]);  
        }

        public ReportParameters Materialize(DiscCtx ctx)
        {         
            var res = new ReportParameters
                {
                    Discussion = ctx.Discussion.FirstOrDefault(d0 => d0.Id == DiscussionId),
                    Topic = ctx.Topic.FirstOrDefault(t0 => t0.Id == TopicId),
                    Session = ctx.Session.FirstOrDefault(s0 => s0.Id == SessionId)
                };

            if (res.Discussion == null || res.Topic == null || res.Session==null)
                return null;

            return res;
        }
    }
}