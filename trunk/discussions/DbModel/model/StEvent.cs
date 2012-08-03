using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Discussions.model
{
    public enum StEvent
    {
        RecordingStarted, //done, 0
        RecordingStopped, //done, 1

        BadgeCreated,  //done  2
        BadgeEdited,   //done  3
        BadgeMoved,    //done  4
        BadgeZoomIn,   //done  5

        ClusterCreated, //done 6
        ClusterDeleted, //done 7
        ClusterIn,      //done 8
        ClusterOut,     //done 9
        ClusterMoved,   //done 10 

        LinkCreated,    //done 11
        LinkRemoved,    //done 12
         
        FreeDrawingCreated, //done, 13
        FreeDrawingRemoved, //done, 14
        FreeDrawingResize,  //done, 15    
        FreeDrawingMoved,   //done, 16

        SceneZoomedIn,  //done 17
        SceneZoomedOut, //done 18

        ArgPointTopicChanged, //done 19

        SourceAdded,   //done 20
        SourceRemoved, //done 21

        ImageAdded,    //done 22
        ImageUrlAdded, //done 23

        PdfAdded, //done 24
        PdfUrlAdded, //done 25

        YoutubeAdded, //done 26

        ScreenshotAdded, //done 27

        MediaRemoved, //done 28

        CommentAdded, //done 29
        CommentRemoved, //done 30

        ImageOpened,      //done 31
        VideoOpened,      //done 32
        ScreenshotOpened, //done 33
        PdfOpened,        //done 34
        SourceOpened,     //done 35

        //this event is only for internal use in WPF client.
        //such events are never sent to photon
        LocalIgnorableEvent // 36
    }
}
