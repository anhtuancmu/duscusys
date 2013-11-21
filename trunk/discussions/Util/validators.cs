using System.Net;

namespace Discussions.util
{
    public static class Validators
    {

        private static System.Text.RegularExpressions.Regex _uriRegex;
        public static bool UriOk(string uri)
        {
            if (string.IsNullOrWhiteSpace(uri))
                return false;

            try
            {
                //Creating the HttpWebRequest
                var request = WebRequest.Create(uri) as HttpWebRequest;
                //Setting the Request method HEAD, you can also use GET too.
                request.Method = "HEAD";
                //Getting the Web Response.
                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                //Returns TRUE if the Status code == 200
                return (response.StatusCode == HttpStatusCode.OK);
            }
            catch
            {
                //Any exception will returns false.
                return false;
            }

        }
    }
}