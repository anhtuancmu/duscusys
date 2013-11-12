using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.ctx;
using Discussions.DbModel;
using System.Drawing;

namespace Discussions.model
{
    internal static class Ctors
    {
        public static bool DiscussionExists(Discussion disc)
        {
            return PublicBoardCtx.Get().Discussion.FirstOrDefault(d => d.Id == disc.Id) != null;
        }

        public static Person NewPerson(string Name, string Email)
        {
            Person res = new Person();
            res.Name = Name;
            res.Email = Email;
            Random rnd = new Random();
            res.Color = Color.FromArgb(255, rnd.Next(155) + 100, rnd.Next(155) + 100, rnd.Next(155) + 100).ToArgb();
            return res;
        }

        public static GeneralSide NewGenSide(Person p, Discussion d, int side)
        {
            var res = new GeneralSide();
            res.Person = p;
            res.Discussion = d;
            res.Side = side;
            return res;
        }

        public static ArgPoint NewArgPoint(string Point)
        {
            ArgPoint res = new ArgPoint();
            res.Point = Point;
            return res;
        }

        public static Comment NewComment(string c, Person p)
        {
            Comment res = new Comment();

            res.Text = c;
            res.Person = p;

            return res;
        }

        public static string ToString(this Person p)
        {
            return String.Format("{0} ({1})", p.Name, p.Email);
        }

        public static string SideCodeToString(int sideCode)
        {
            switch (sideCode)
            {
                case (int) SideCode.Cons:
                    return "Cons";
                case (int) SideCode.Pros:
                    return "Pros";
                case (int) SideCode.Neutral:
                    return "Neutral";
                default:
                    return null;
            }
        }

        public static Attachment GetFirstAttachment(ArgPoint pt)
        {
            if (pt != null && pt.Attachment.Count > 0)
                return pt.Attachment.First();
            else
                return null;
        }
    }
}