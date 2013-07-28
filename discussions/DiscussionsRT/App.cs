﻿using LiteLobby;
using Photon.SocketServer;
using Discussions.DbModel;

namespace Discussions.RTModel
{
    public class App : LiteLobbyApplication
    {
        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            return new DiscussionPeer(initRequest.Protocol, initRequest.PhotonPeer);
        }

        protected override void Setup()
        {
            base.Setup();

            var ctx = new DiscCtx(Discussions.ConfigManager.ConnStr);
            foreach (var p in ctx.Person)
                p.Online = false;
            ctx.SaveChanges();
        }
    }
}