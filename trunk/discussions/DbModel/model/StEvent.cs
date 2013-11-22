namespace Discussions.DbModel.model
{
    public enum StEvent
    {
        RecordingStarted = 0, 
        RecordingStopped = 1, 

        BadgeCreated = 2, 
        BadgeEdited = 3, 
        BadgeMoved = 4, 
        BadgeZoomIn = 5, 

        ClusterCreated = 6, 
        ClusterDeleted = 7, 
        ClusterIn = 8, 
        ClusterOut = 9, 
        ClusterMoved = 10,
        ClusterTitleAdded = 37,
        ClusterTitleEdited = 38,
        ClusterTitleRemoved = 39,  

        LinkCreated = 11, 
        LinkRemoved = 12,

        FreeDrawingCreated = 13, 
        FreeDrawingRemoved = 14,
        FreeDrawingResize = 15,   
        FreeDrawingMoved = 16,

        SceneZoomedIn = 17, 
        SceneZoomedOut = 18, 

        ArgPointTopicChanged = 19, 

        SourceAdded = 20, 
        SourceRemoved = 21, 

        ImageAdded = 22, 
        ImageUrlAdded = 23,

        PdfAdded = 24, 
        PdfUrlAdded = 25, 

        YoutubeAdded = 26, 

        ScreenshotAdded = 27, 

        MediaRemoved = 28, 

        CommentAdded = 29, 
        CommentRemoved = 30,

        ImageOpened = 31,
        VideoOpened = 32, 
        ScreenshotOpened = 33, 
        PdfOpened = 34,
        SourceOpened = 35,

        //this event is only for internal use in WPF client.
        //such events are never sent to photon
        LocalIgnorableEvent = 36,

        LaserEnabled = 40,  
    }
}