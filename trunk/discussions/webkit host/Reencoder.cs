using System.Net;
using System.Text;

namespace Discussions.webkit_host
{
    public class Reencoder
    {
        public static string ShiftJisToUtf8(string shiftJis)
        {
            var shiftJisEncoding = Encoding.GetEncoding("SHIFT_JIS");
            byte[] utfBytes = shiftJisEncoding.GetBytes(shiftJis);
            return Encoding.UTF8.GetString(utfBytes);
        }

        public static string GetUrlContent(string url)
        {
            var webClient = new WebClient();
            webClient.Encoding = Encoding.GetEncoding("SHIFT_JIS");
            return webClient.DownloadString(url);
        }
    }
}