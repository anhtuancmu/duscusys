using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.DbModel;
using Discussions.model;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public class Serializers
    {
        public static Dictionary<byte, object> WriteBadgeViews(UserCursor cursor, 
                                                               SharedView[] badgeViews)
        {
            //serialize badge geometry 
            // number of badges 
            // array of x's  
            // array of y's 
            // array of orientations
            // array of Ids (DB Ids of points)
            double[] Xs = new double[badgeViews.Length];
            double[] Ys = new double[badgeViews.Length];
            double[] Orientations = new double[badgeViews.Length];
            bool[] ViewTypes = new bool[badgeViews.Length];
            int[] Ids = new int[badgeViews.Count()];
            
            for (int i = 0; i < badgeViews.Count(); ++i)
            {
                Xs[i] = badgeViews[i].badgeGeometry.CenterX;
                Ys[i] = badgeViews[i].badgeGeometry.CenterY;
                Orientations[i] = badgeViews[i].badgeGeometry.Orientation;
                ViewTypes[i] = badgeViews[i].viewType;
                Ids[i] = badgeViews[i].ViewId;
            }
            Dictionary<byte, object> data = new Dictionary<byte, object>();
            data.Add((byte)DiscussionParamKey.NumArrayEntries, badgeViews.Length);
            data.Add((byte)DiscussionParamKey.ArrayOfX, Xs);
            data.Add((byte)DiscussionParamKey.ArrayOfY, Ys);
            data.Add((byte)DiscussionParamKey.ArrayOfOrientations, Orientations);
            data.Add((byte)DiscussionParamKey.ArrayOfIds, Ids);
            data.Add((byte)DiscussionParamKey.ArrayOfViewTypes, ViewTypes);
            data.Add((byte)DiscussionParamKey.UserCursorName, cursor.Name);
            data.Add((byte)DiscussionParamKey.UserCursorState, (int)cursor.State);

            return data;
        }

        public static SharedView[] ReadBadgeViews(Dictionary<byte, object> badgeViews, out UserCursor cursor)
        {
            int count = (int)badgeViews[(byte)DiscussionParamKey.NumArrayEntries];
            double[] Xs = (double[])badgeViews[(byte)DiscussionParamKey.ArrayOfX];
            double[] Ys = (double[])badgeViews[(byte)DiscussionParamKey.ArrayOfY];
            double[] Orientations = (double[])badgeViews[(byte)DiscussionParamKey.ArrayOfOrientations];
            int[] argPointIds = (int[])badgeViews[(byte)DiscussionParamKey.ArrayOfIds];
            bool[] viewTypes = (bool[])badgeViews[(byte)DiscussionParamKey.ArrayOfViewTypes];            

            if (!badgeViews.ContainsKey((byte)DiscussionParamKey.UserCursorName))
            {
                Console.WriteLine("No key");
            }

            cursor = new UserCursor((string)badgeViews[(byte)DiscussionParamKey.UserCursorName],
                                    (CursorInputState)badgeViews[(byte)DiscussionParamKey.UserCursorState]);

            var res = new SharedView[count];
            for (int i = 0; i < count; ++i)
            {
                SharedView ap = new SharedView(i,viewTypes[i]);
                ap.badgeGeometry.CenterX = Xs[i];
                ap.badgeGeometry.CenterY = Ys[i];
                ap.badgeGeometry.Orientation = Orientations[i];
                ap.ViewId = argPointIds[i];
                res[i] = ap;
            }

            return res;
        }

        public static Dictionary<int, SharedView> ArrToDict(IEnumerable<SharedView> a)
        {
            Dictionary<int, SharedView> dict = new Dictionary<int, SharedView>();           
            foreach(SharedView sv in a)
                dict.Add(sv.ViewId, sv);

            return dict;
        }

        public static SharedView[] SVUnion(SharedView[] a1, SharedView[] a2)
        {
            SharedView[] res = new SharedView[a1.Length + a2.Length];
            for (int i = 0; i < a1.Length; i++)
                res[i] = a1[i];
            for (int i = 0; i < a2.Length; i++)
                res[a1.Length + i] = a2[i];

            return res;
        }

        public static void ReadBoxDimensions(Dictionary<byte, object> par, out double boxWidth,out double boxHeight)
        {
            boxWidth  = (double)par[(byte)DiscussionParamKey.BoxWidth];
            boxHeight = (double)par[(byte)DiscussionParamKey.BoxHeight];
        }

        public static void WriteBoxDimensions(Dictionary<byte, object> par, double boxWidth, double boxHeight)
        {
            par[(byte)DiscussionParamKey.BoxWidth] = boxWidth;
            par[(byte)DiscussionParamKey.BoxHeight] = boxHeight;
        }

        public static bool ReadBadgeExpanded(Dictionary<byte, object> par, out int argPointId)
        {
            argPointId = (int)par[(byte)DiscussionParamKey.ArgPointId];
            return (bool)par[(byte)DiscussionParamKey.BadgeExpansionFlag];
        }

        public static Dictionary<byte, object> WriteBadgeExpanded(bool expanded, int argPointId)
        {
            Dictionary<byte, object> res = new Dictionary<byte, object>();
            res[(byte)DiscussionParamKey.ArgPointId] = argPointId;
            res[(byte)DiscussionParamKey.BadgeExpansionFlag] = expanded;
            return res;
        }

        public static int ReadChangedTopicId(Dictionary<byte, object> par)
        {
            return (int)par[(byte)DiscussionParamKey.ChangedTopicId];
        }

        public static Dictionary<byte, object> WriteChangedTopicId(int Id)
        {
            Dictionary<byte, object> res = new Dictionary<byte,object>();
            res[(byte)DiscussionParamKey.ChangedTopicId] = Id;
            return res;
        }

        public static Dictionary<byte, object> AddChangedTopicId(Dictionary<byte, object> param,  int Id)
        {
            param.Add((byte)DiscussionParamKey.ChangedTopicId, Id);
            return param;
        }

        public static Dictionary<byte, object> WriteUserCursor(UserCursor c)
        {
            Dictionary<byte, object> res = new Dictionary<byte, object>();

            res[(byte)DiscussionParamKey.UserCursorName] = c.Name;
            res[(byte)DiscussionParamKey.UserCursorState] = c.State;
            res[(byte)DiscussionParamKey.UserCursorUsrId] = c.usrId;
            res[(byte)DiscussionParamKey.UserCursorX] = c.x;
            res[(byte)DiscussionParamKey.UserCursorY] = c.y;

            return res;
        }

        public static UserCursor ReadUserCursor(Dictionary<byte, object> dict)
        {
            UserCursor res = new UserCursor((string)dict[(byte)DiscussionParamKey.UserCursorName]);
            res.State = (CursorInputState)dict[(byte)DiscussionParamKey.UserCursorState];
            res.usrId = (int)dict[(byte)DiscussionParamKey.UserCursorUsrId];
            res.x = (double)dict[(byte)DiscussionParamKey.UserCursorX]; 
            res.y = (double)dict[(byte)DiscussionParamKey.UserCursorY]; 
            return res;
        }

        public static Dictionary<byte, object> WriteChangedArgPoint(int ArgPointId, int topicId, PointChangedType pointChangeType)                                                                 
        {
            var res = new Dictionary<byte, object>();
            res[(byte)DiscussionParamKey.PointChangeType] = pointChangeType; 
            res[(byte)DiscussionParamKey.ArgPointId] = ArgPointId;
            res[(byte)DiscussionParamKey.ChangedTopicId] = topicId;
            return res;
        }

        public static int ReadChangedArgPoint(Dictionary<byte, object> dict, out PointChangedType pointChangeType, out int topicId)
        {
            pointChangeType = (PointChangedType)dict[(byte)DiscussionParamKey.PointChangeType];
            topicId = (int)dict[(byte)DiscussionParamKey.ChangedTopicId];
            return (int)dict[(byte)DiscussionParamKey.ArgPointId];            
        }

        public static void ReadStatEventParams(Dictionary<byte, object> par, out StEvent e, out int userId,
                                                out int discussionId, out int topicId, out DeviceType deviceType)
        {            
            discussionId = (int)par[(byte)DiscussionParamKey.DiscussionId];
            userId = (int)par[(byte)DiscussionParamKey.UserId];
            e = (StEvent)par[(byte)DiscussionParamKey.StatsEvent];
            topicId = (int)par[(byte)DiscussionParamKey.ChangedTopicId];
            deviceType = (DeviceType)par[(byte)DiscussionParamKey.DeviceType];        
        }

        public static Dictionary<byte, object> WriteStatEventParams(StEvent e, int userId, int discussionId, 
                                                                    int topicId, DeviceType deviceType)
        {
            Dictionary<byte, object> res = new Dictionary<byte, object>();
            res[(byte)DiscussionParamKey.DiscussionId] = discussionId;
            res[(byte)DiscussionParamKey.UserId] = userId;
            res[(byte)DiscussionParamKey.StatsEvent] = e;
            res[(byte)DiscussionParamKey.ChangedTopicId] = topicId;
            res[(byte)DiscussionParamKey.DeviceType] = deviceType; 
            return res;
        }
    }
}
