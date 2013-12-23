﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Discussions.DbModel;
using Discussions.DbModel.model;

namespace DiscSvc.Reporting
{
    public class Helpers
    {
        public static string GetBackground(QueryParams parameters, DiscCtx ctx)
        {          
            var discId = parameters.DiscussionId;
            var disc = ctx.Discussion.Single(d0 => d0.Id == discId);
            return disc.HtmlBackground;
        }

        public static IEnumerable<Person> Participants(Topic topic, Session session)
        {
            var topId = topic.Id;
            return session.Person.Where(p => p.Topic.Any(t => t.Id == topId));
        }

        public static IEnumerable<Tuple<Person,Person>> ParticipantsTuples(Topic topic, Session session)
        {
            var participants = Participants(topic, session);

            var res = new List<Tuple<Person, Person>>(); 

            Person prev = null;
            foreach (var current in participants)
            {
                if (prev != null)
                {
                    res.Add(new Tuple<Person, Person>(prev,current));
                    prev = null;                    
                }
                else
                    prev = current;
            }
            if (prev != null)
                res.Add(new Tuple<Person, Person>(prev, null));

            return res;
        }

        public static string IntToHtml(int color)
        {
            return "#" + (color & 0x00ffffff).ToString("X6");
        }

        public static string GetPastableHtml(Attachment a, string baseUrl)
        {
            if (IsGraphicFormat((AttachmentFormat)a.Format))
            {
                var imgThumbUrl = string.Format("{0}/discsvc.svc/Attachment({1})/$value",
                                          baseUrl, a.Id);

                return string.Format("<img src=\"{0}\">", imgThumbUrl);
            }
            if (a.Format == (int)AttachmentFormat.Youtube)
            {
                //return
                //    string.Format(
                //        "<iframe width=\"640\" height=\"360\" src=\"{0}\" frameborder=\"0\" allowfullscreen></iframe>",
                //        a.VideoEmbedURL);

                return string.Format("<img src=\"{0}\" style=\"max-width:800px\">", a.VideoThumbURL);
            }
            if (a.Format == (int)AttachmentFormat.Pdf && a.Thumb!=null)
            {
                var base64img = Convert.ToBase64String(a.Thumb);
                return string.Format("<img src=\"data:image/jpeg;base64,{0}\" style=\"max-width:800px\">", base64img);
            }
          
            return "";
        }

        public static string GetAttachmentLink(Attachment a, string baseUrl, int i)
        {
            var target = getAttachmentLink(a, baseUrl);

            if (a.Format == (int)AttachmentFormat.PngScreenshot)
                return string.Format("<a target=\"_blank\" href=\"{0}\">{1}. {2}</a>",target, i, a.Title);

            return string.Format("<a target=\"_blank\" href=\"{1}\">{0}. {1}</a>", i, target);
        }

        static string getAttachmentLink(Attachment a, string baseUrl)
        {
            var mediaUrl = string.Format("{0}/discsvc.svc/Attachment({1})/$value",
                                            baseUrl, a.Id);
            if (IsGraphicFormat((AttachmentFormat)a.Format) && 
                a.Format!=(int)AttachmentFormat.PngScreenshot)
            {
                if (a.Format != (int)AttachmentFormat.PngScreenshot && 
                    !string.IsNullOrWhiteSpace(a.Link))
                    return a.Link;
            }
            if (a.Format == (int)AttachmentFormat.Youtube)
            {
                return a.VideoLinkURL;
            }
            if (a.Format == (int)AttachmentFormat.Pdf)
            {
                if (!string.IsNullOrWhiteSpace(a.Link))
                    return a.Link;
                return mediaUrl;
            }
            return mediaUrl;
        }

        public static bool IsGraphicFormat(AttachmentFormat af)
        {
            return
                af == AttachmentFormat.Bmp ||
                af == AttachmentFormat.Jpg ||
                af == AttachmentFormat.Png ||
                af == AttachmentFormat.PngScreenshot;
        }

        public static IEnumerable<ArgPoint> ArgPointsOf(Person pers, Discussion d, Topic t)
        {
            return pers.ArgPoint.Where(ap => ap.Topic != null && ap.Topic.Id == t.Id);
        }

        public static string processLineBreaks(string bio)
        {
            return bio.Replace("\r\n", "\n").Replace("\r", "\n").Replace("\n","<br>");            
        }

        public static string ArgPointToStr(ArgPoint ap)
        {
            return string.Format("Point#{0}. {1}", ap.OrderNumber, ap.Point);
        }

        public static string BaseUrl(HttpRequest request)
        {
            string baseUrl = request.Url.Scheme + "://" + request.Url.Authority +
                             request.ApplicationPath.TrimEnd('/') + "/";
            return baseUrl;
        }
    }
}