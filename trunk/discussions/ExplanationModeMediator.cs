using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Discussions.DbModel;

namespace Discussions
{
    public class ExplanationModeMediator
    {
        static ExplanationModeMediator _inst = null;
        public static ExplanationModeMediator Inst
        {
            get
            {
                if (_inst == null)
                    _inst = new ExplanationModeMediator();
                return _inst;
            }
        }

        class ViewerRecord
        {
            public ImageWindow wnd;
            public int attachId; 
        }

        //all locally opened image windows 
        List<ViewerRecord> _openedViewers = new List<ViewerRecord>();
        
        //called when window is closed by any initiator 
        public Action CloseReq;

        //called when window is opened by any initiator 
        public Action<int> OpenReq;

        //called every time a window is closed by any initiator
        public void OnWndClosed(ImageWindow wnd)
        {
            var viewRec = _openedViewers.FirstOrDefault(vr => vr.wnd == wnd);
            if (viewRec != null)
            {
                _openedViewers.Remove(viewRec);
                if (CloseReq != null)
                    CloseReq();
            }
        }

        //called every time a window is opened by any initiator
        public void OnWndOpened(ImageWindow w, int attId)
        {
            if (attId < 0)
                return;

            var prevInstOfAttach= _openedViewers.FirstOrDefault(vr => vr.attachId == attId);
            if (prevInstOfAttach == null)
            {
                _openedViewers.Add(new ViewerRecord { attachId = attId, wnd = w });
                if (OpenReq != null)
                    OpenReq(attId);
            }
        }

        public bool IsViewerOpened(int attId)
        {
            var prevInstOfAttach = _openedViewers.FirstOrDefault(vr => vr.attachId == attId);
            return prevInstOfAttach != null;
        }

        public void EnsureInstanceClosed(int attachmentId)
        {
            var prevInstOfAttach = _openedViewers.FirstOrDefault(vr => vr.attachId == attachmentId);
            if (prevInstOfAttach != null)
            {
                prevInstOfAttach.wnd.Close();
            }
        }
    }
}
