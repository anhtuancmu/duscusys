using System.Linq;
using System.Web;
using Discussions;
using Discussions.DbModel;

//http://192.168.0.7/discsvc/bgpage?id=1

namespace DiscSvc
{
    /// <summary>
    /// Summary description for Handler1
    /// </summary>
    public class DiscBgHandler : IHttpHandler
    {
        public void ProcessRequest(HttpContext context)
        {
            context.Response.ContentType = "text/html";
            var discId = -1;
            if (int.TryParse(context.Request.QueryString["id"], out discId) || discId == -1)
            {
                DiscCtx ctx = new DiscCtx(ConfigManager.ConnStr);
                var disc = ctx.Discussion.FirstOrDefault(d0 => d0.Id == discId);
                if (disc != null)
                    context.Response.Write(disc.HtmlBackground);
            }
            else
            {
                context.Response.Write("Cannot find background for this discussion");
            }
        }

        public bool IsReusable
        {
            get { return false; }
        }
    }
}