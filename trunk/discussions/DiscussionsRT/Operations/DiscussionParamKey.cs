using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Discussions.RTModel.Operations
{
    public enum DiscussionParamKey : byte
    {
        Message             = 0,
        ArrayOfX            = 1,
        ArrayOfY            = 2,
        ArrayOfOrientations = 3,
        ArrayOfIds          = 4,
        NumArrayEntries     = 5,
        UserCursorName      = 6,
        UserCursorState     = 7,
        BoxWidth            = 8,
        BoxHeight           = 9,
        BadgeExpansionFlag  = 10,
        ArgPointId          = 11,
        ArrayOfViewTypes    = 12,
        ChangedTopicId      = 13,                
        GeometryChangedWithStruct  = 14,
        StructChangeActorNr = 15,
        UserCursorX         = 16,
        UserCursorY         = 17,
        UserCursorUsrId     = 18,
        AnnotationId        = 19,

        //structure change notification not fired on initiating client. but when 
        //moderator dashboad is open, and moderator adds/removes topic, and private board is open,
        //we want the list of topics in private board to be updated. 
        ForceSelfNotification= 20,
        
        StatsEvent         = 21, 
        DiscussionId       = 22, 
        UserId             = 23,
        DeviceType         = 24,

        /// distributed vector editor 
        ShapeId            = 25,
        DoLock             = 26,
        ShapeOwnerId       = 27,
        BoolParameter1     = 28,
        BoolParameter2     = 29,
        Radius             = 30,
        AnchorX            = 31,
        AnchorY            = 32,
        InitialShapeOwnerId  = 33,
        ShapeType          = 34,
        ArrayOfInts        = 35,
        ArrayOfDoubles     = 36,
        AutoTakeCursor     = 37,
        Tag                = 38,
        PointChangeType    = 39,
        LinkEnd1Id         = 40,
        LinkEnd2Id         = 41,
        ClusterId          = 42,
        DoBroadcast        = 43,
        CallToken          = 44,
        InkData            = 45,
        ArrayOfBytes       = 46,

        /// reporting
        NumClusters        = 47,
        NumClusteredBadges = 48,
        NumLinks           = 49,
        ClusterCaption     = 50,
        ArrayOfInts2       = 51,
        ArrayOfStrings     = 52,
        ArgPointId2        = 53,
        ClusterCaption2    = 54,
        LinkHeadTypeKey    = 55,
        ClusterId2         = 56,
    }
}
