namespace Discussions.RTModel.Operations
{
    public enum DiscussionEventCode : byte
    {
        //a client notified server structure was changed, broadcast
        StructureChanged = 2,

        //
        UserCursorChanged = 3,

        //arg.point changed
        ArgPointChanged = 4,

        //used for instant updates of global online list
        //1. in response to NotifyLeaveNotify server notifies clients for fast onlien list update
        //2. client joins lobby, not discussion room. 
        InstantUserPlusMinus = 5,

        //new user account created/deleted 
        UserAccPlusMinus = 7,

        //server broadcasts stats event
        StatsEvent = 8,

        //client locked vector shape 
        CursorEvent = 9,

        //client created shape
        CreateShapeEvent = 10,

        UnselectAllEvent = 13,

        DeleteSingleShapeEvent = 14,

        ApplyPointEvent = 15,

        StateSyncEvent = 16,

        LinkCreateEvent = 17,

        UnclusterBadgeEvent = 19,

        ClusterBadgeEvent = 20,

        InkEvent = 21,

        //send to client when initial loading sequence is done
        SceneLoadingDone = 22,

        DEditorReportEvent = 23,

        ClusterStatsEvent = 24,

        LinkStatsEvent = 25,


        //
        // explanation mode 
        //
        BadgeViewEvent = 26,

        SourceViewEvent = 27,


        //
        //comment notifications
        //
        CommentReadEvent = 28,

        //
        //laser pointers
        //
        AttachLaserPointerEvent = 29,

        DetachLaserPointerEvent = 30,

        LaserPointerMovedEvent = 31,

        //image viewer, explanation mode
        ImageViewerManipulatedEvent = 32,
        
        BrowserScrollChangedEvent = 33,
    }
}