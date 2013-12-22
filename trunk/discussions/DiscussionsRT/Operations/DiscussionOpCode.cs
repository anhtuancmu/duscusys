namespace Discussions.RTModel.Operations
{
    public enum DiscussionOpCode : byte
    {
        Test = 0,

        //group/ungroup or drag/drop operation happened
        NotifyStructureChanged = 5,

        //any attribute of given arg.point changed. replacement for NotifyStructureChanged when 
        //only single arg.point changed
        NotifyArgPointChanged = 7,

        //server supports online status list across all discussions. when client 
        //disconnects, server detects disconnection and puts the user offline. it takes time.
        //client can send this notification to inform server it's about to disconnect to speed up things. 
        //lite leave request doesn't work
        NotifyLeaveUser = 8,

        //when moderator creates new user account or deletes existing one, the client sends this to server. 
        //server broadcasts to other clients, including the original client 
        NotifyUserAccPlusMinus = 10,

        //when one of stats events occurs, client who generated the event sends it to server. server adds event 
        //record to DB
        StatsEvent = 11,

        //when user changes his name after login (by clicking user name near avatar at top right of main form)
        //other clients show previous name in online list. By sending this to server, client ensures that server
        //broadcasts [acc plus minus] event to force all clients to update online list
        NotifyNameChanged = 12,


        ///// distributed vector editor 

        //client tries to set (or unset) own cursor on shape 
        CursorRequest = 13,

        //client creates shape 
        CreateShapeRequest = 14,

        //client removes shape 
        DeleteShapesRequest = 15,

        //client unselected all shapes 
        UnselectAllRequest = 17,

        DeleteSingleShapeRequest = 18,

        //shape state changed, initiator notifies
        StateSyncRequest = 20,

        //when client connects to annotation stream, it sends initial load request to server 
        InitialSceneLoadRequest = 21,

        LinkCreateRequest = 22,

        UnclusterBadgeRequest = 24,

        ClusterBadgeRequest = 25,

        ClusterMoveRequest = 27,

        InkRequest = 28,

        ///   reporting  
        ClusterStatsRequest = 32,

        DEditorReport = 33,

        LinkReportRequest = 34,

        //
        // explanation mode 
        //
        BadgeViewRequest = 35,

        ExplanationModeSyncViewRequest = 36,


        // reporting, screenshots
        ScreenshotRequest = 37,

        //
        //comment notifications
        //
        CommentReadRequest = 38,


        /// <summary>
        /// A user enables laser pointer mode. This event is broadcast to all clients. 
        /// </summary>
        AttachLaserPointerRequest = 39,

        /// <summary>
        /// A user with laser pointer either exits laser mode.
        /// </summary>
        DetachLaserPointerRequest = 40,


        LaserPointerMovedRequest = 41,


        DetachLaserPointerFromAnyTopicRequest = 42, 


        /// <summary>
        /// Any manipulation in image viewer happens and client notifies the server
        /// </summary>
        ImageViewerManipulateRequest = 43,

        ImageViewerStateRequest = 44,


        //browser scroll bar sync.
        BrowserScrollChanged = 45,

        GetBrowserScrollPos = 46, 

        //pdf scroll bar sync
        PdfScrollChanged = 50,

        GetPdfScrollPos = 48
    }
}