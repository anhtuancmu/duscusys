using System.Net;
using System.Net.Mail;
using System.Web;

namespace DiscSvc.Reporting
{
    public class EmailHandler : IHttpHandler
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
            var url = context.Request.Params["reportUrl"];
            var email = context.Request.Params["email"];            
            if (url == null || email==null)
            {
                context.Response.StatusCode = 400;
                return;
            }

            context.Response.ContentType = "text/plain";
            try
            {
                using (var mm = new MailMessage("discusys@yandex.com", email))
                {
                    mm.Subject = "Discussion report";
                    mm.Body = "To view results of your discussion, please follow the link: \n" + url + "\n\n" +                              
                              "Tohoku University " +
                              "Discussion System";
                    mm.IsBodyHtml = false;
                    using (var sc = new SmtpClient("smtp.yandex.ru", 25))
                    {
                        sc.EnableSsl = true;
                        sc.DeliveryMethod = SmtpDeliveryMethod.Network;
                        sc.UseDefaultCredentials = false;
                        sc.Credentials = new NetworkCredential("discusys@yandex.com", "fegwg434v3hdmr6");
                        sc.Send(mm);
                    }
                }
            }
            catch
            {
                context.Response.Write("An error occured, please retry");                
                return;
            }
            context.Response.Write("The link hass been sent");
        }

        #endregion
    }
}
