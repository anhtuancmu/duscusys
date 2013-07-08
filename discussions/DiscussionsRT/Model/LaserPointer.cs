using System.Collections.Generic;
using Discussions.RTModel.Operations;

namespace Discussions.RTModel.Model
{
    public class LaserPointer
    {
        public int UserId;
        public int Color;
        public string Name;
        public int TopicId;
        public double X;
        public double Y;

        public static LaserPointer Read(Dictionary<byte, object> par)
        {
            var res = new LaserPointer
                {
                    UserId = (int)par[(byte)DiscussionParamKey.UserId],
                    Color = (int)par[(byte)DiscussionParamKey.Color],
                    Name = (string)par[(byte)DiscussionParamKey.UserCursorName],
                    TopicId = (int)par[(byte)DiscussionParamKey.ChangedTopicId],
                    X = (double)par[(byte)DiscussionParamKey.UserCursorX],
                    Y = (double)par[(byte)DiscussionParamKey.UserCursorY]
                };
            return res;
        }

        public static Dictionary<byte, object> Write(int userId, int color, string name, int topicId, double x, double y)
        {
            var lp = new LaserPointer { Color = color, Name = name, TopicId = topicId, UserId = userId, X = x, Y = y };
            return lp.ToDict();
        }

        public Dictionary<byte, object> ToDict()
        {
            var res = new Dictionary<byte, object>
                {
                    {(byte) DiscussionParamKey.UserId, UserId},
                    {(byte) DiscussionParamKey.Color, Color},
                    {(byte) DiscussionParamKey.UserCursorName, Name },                   
                    {(byte) DiscussionParamKey.ChangedTopicId, TopicId} ,
                    {(byte) DiscussionParamKey.UserCursorX, X},
                    {(byte) DiscussionParamKey.UserCursorY, Y},
                };
            return res;
        }
    }
}