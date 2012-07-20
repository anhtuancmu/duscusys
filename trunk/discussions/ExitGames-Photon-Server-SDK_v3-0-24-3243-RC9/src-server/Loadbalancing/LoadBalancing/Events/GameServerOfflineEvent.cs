// -----------------------------------------------------------------------
// <copyright file="GameServerOfflineEvent.cs" company="">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Photon.LoadBalancing.Events
{
    using Photon.SocketServer.Rpc;

    public class GameServerOfflineEvent : DataContract
    {
        [DataMember(Code = 0, IsOptional=true)]
        public int TimeLeft { get; set; }
    }
}
