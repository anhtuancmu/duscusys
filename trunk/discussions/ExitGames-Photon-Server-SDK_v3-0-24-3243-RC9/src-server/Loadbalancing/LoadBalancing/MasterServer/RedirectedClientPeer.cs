// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RedirectedClientPeer.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the RedirectedClientPeer type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.LoadBalancing.MasterServer
{
    using System.Net;

    using ExitGames.Logging;

    using Photon.LoadBalancing.Common;
    using Photon.LoadBalancing.Operations;
    using Photon.SocketServer;

    using PhotonHostRuntimeInterfaces;

    public class RedirectedClientPeer : PeerBase
    {
        private static readonly ILogger log = LogManager.GetCurrentClassLogger();
        
        private readonly MasterApplication application;

        #region Constructors and Destructors

        public RedirectedClientPeer(IRpcProtocol protocol, IPhotonPeer unmanagedPeer, MasterApplication application)
            : base(protocol, unmanagedPeer)
        {
            this.application = application;
        }

        #endregion

        #region Methods

        protected override void OnDisconnect(DisconnectReason reasonCode, string reasonDetail)
        {
        }

        protected override void OnOperationRequest(OperationRequest operationRequest, SendParameters sendParameters)
        {
            var contract = new RedirectRepeatResponse();

            byte masterNodeId = 1;

            IPAddress publiIpAddress;
            if (PublicIPAddressReader.TryParsePublicIpAddress(MasterServerSettings.Default.PublicIPAddress, out publiIpAddress) == false)
            {
                // TODO: how to handle ?
                log.WarnFormat("Failed to parse public ipAdress");
                contract.Address = "";
            }
            else
            {
                switch (this.NetworkProtocol)
                {
                    case NetworkProtocolType.Tcp:
                        contract.Address = new IPEndPoint(publiIpAddress, MasterServerSettings.Default.MasterRelayPortTcp + masterNodeId - 1).ToString();
                        break;
                    case NetworkProtocolType.WebSocket:
                        contract.Address =
                            new IPEndPoint(publiIpAddress, MasterServerSettings.Default.MasterRelayPortWebSocket + masterNodeId - 1).ToString();
                        break;
                    case NetworkProtocolType.Udp:
                        // no redirect through relay ports for UDP... how to handle? 
                        contract.Address =
                            new IPEndPoint(publiIpAddress, MasterServerSettings.Default.MasterRelayPortUdp + masterNodeId - 1).ToString();
                        break;
                }
            }

            var response = new OperationResponse(operationRequest.OperationCode, contract);
            response.ReturnCode = (short)ErrorCode.RedirectRepeat;
            response.DebugMessage = "redirect";

            this.SendOperationResponse(response, sendParameters);
        }

        #endregion
    }
}