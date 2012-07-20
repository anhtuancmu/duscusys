// --------------------------------------------------------------------------------------------------------------------
// <copyright file="GameApplication.cs" company="Exit Games GmbH">
//   Copyright (c) Exit Games GmbH.  All rights reserved.
// </copyright>
// <summary>
//   Defines the GameApplication type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace Photon.LoadBalancing.GameServer
{
    #region using directives

    using System;
    using System.IO;
    using System.Net;
    using System.Threading;

    using ExitGames.Concurrency.Fibers;
    using ExitGames.Logging;
    using ExitGames.Logging.Log4Net;

    using Lite;
    using Lite.Messages;

    using Photon.LoadBalancing.LoadShedding.Diagnostics;

    using log4net;
    using log4net.Config;

    using Photon.LoadBalancing.Common;
    using Photon.LoadBalancing.LoadShedding;
    using Photon.SocketServer;
    using Photon.SocketServer.Diagnostics;
    using Photon.SocketServer.ServerToServer;

    using LogManager = ExitGames.Logging.LogManager;

    #endregion

    public class GameApplication : ApplicationBase
    {
        #region Constants and Fields

        public static readonly Guid ServerId = Guid.NewGuid();

        private static readonly ILogger log = LogManager.GetCurrentClassLogger();

        private static GameApplication instance;

        private static OutgoingMasterServerPeer masterPeer;

        private byte isReconnecting;

        private Timer retry;

        private readonly NodesReader reader;

        private PoolFiber executionFiber;

        #endregion

        #region Constructors and Destructors

        public GameApplication()
        {
            IPAddress masterAddress = IPAddress.Parse(GameServerSettings.Default.MasterIPAddress);
            int masterPort = GameServerSettings.Default.OutgoingMasterServerPeerPort;
            this.MasterEndPoint = new IPEndPoint(masterAddress, masterPort);

            this.GamingTcpPort = GameServerSettings.Default.GamingTcpPort;
            this.GamingUdpPort = GameServerSettings.Default.GamingUdpPort;
            this.GamingWebSocketPort = GameServerSettings.Default.GamingWebSocketPort;

            this.ConnectRetryIntervalSeconds = GameServerSettings.Default.ConnectReytryInterval;
            
            this.reader = new NodesReader(CommonSettings.Default.NodesFilePath, CommonSettings.Default.NodesFileName);
        }
        #endregion

        #region Properties

        public new static GameApplication Instance
        {
            get
            {
                return instance;
            }

            protected set
            {
                Interlocked.Exchange(ref instance, value);
            }
        }

        public int? GamingTcpPort { get; protected set; }

        public int? GamingUdpPort { get; protected set; }

        public int? GamingWebSocketPort { get; protected set; }

        public IPEndPoint MasterEndPoint { get; protected set; }

        public IPAddress PublicIpAddress { get; protected set; }

        public OutgoingMasterServerPeer MasterPeer
        {
            get
            {
                return masterPeer;
            }

            protected set
            {
                Interlocked.Exchange(ref masterPeer, value);
            }
        }

        public WorkloadController WorkloadController { get; protected set; }

        protected int ConnectRetryIntervalSeconds { get; set; }

        #endregion

        #region Public Methods
        
        public void ConnectToMaster(IPEndPoint endPoint)
        {
            if (this.Running == false)
            {
                return;
            }

            if (this.ConnectToServer(endPoint, "Master", endPoint))
            {
                if (log.IsInfoEnabled)
                {
                    log.InfoFormat("Connecting to master at {0}, serverId={1}", endPoint, ServerId);
                }
            }
            else
            {
                log.WarnFormat("master connection refused - is the process shutting down ? {0}", ServerId);
            }
        }

        public void ConnectToMaster()
        {
            if (this.Running == false)
            {
                return;
            }

            if (this.ConnectToServer(this.MasterEndPoint, "Master", this.MasterEndPoint) == false)
            {
                log.WarnFormat("Master connection refused. serverId={0}", ServerId);
                return;
            }

            if (log.IsInfoEnabled)
            {
                log.InfoFormat(this.isReconnecting == 0 ? "Connecting to master at {0}, serverId={1}" : "Reconnecting to master at {0}, serverId={1}", this.MasterEndPoint, ServerId);
            }
        }

        public void ReconnectToMaster()
        {
            if (this.Running == false)
            {
                return;
            }

            Thread.VolatileWrite(ref this.isReconnecting, 1);
            this.retry = new Timer(o => this.ConnectToMaster(), null, this.ConnectRetryIntervalSeconds * 1000, 0);
        }

        public byte GetCurrentNodeId()
        {
            return reader.ReadCurrentNodeId();
        }

        #endregion

        #region Methods

        protected override void OnStopRequested()
        {
            log.InfoFormat("OnStopRequested: serverid={0}", ServerId);
                        
            this.WorkloadController.Stop();
            if (this.MasterPeer != null)
            {
                this.MasterPeer.Disconnect();
            }

            base.OnStopRequested();
        }

        protected virtual PeerBase CreateGamePeer(InitRequest initRequest)
        {
            return new GameClientPeer(initRequest, this);
        }

        protected virtual OutgoingMasterServerPeer CreateMasterPeer(InitResponse initResponse)
        {
            return new OutgoingMasterServerPeer(initResponse.Protocol, initResponse.PhotonPeer, this);
        }

        protected override PeerBase CreatePeer(InitRequest initRequest)
        {
            if (log.IsDebugEnabled)
            {
                log.DebugFormat("CreatePeer for {0}", initRequest.ApplicationId);
            }

            // Game server latency monitor connects to self
            if (initRequest.ApplicationId == "LatencyMonitor")
            {
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat(
                        "incoming latency peer at {0}:{1} from {2}:{3}, serverId={4}", 
                        initRequest.LocalIP, 
                        initRequest.LocalPort, 
                        initRequest.RemoteIP, 
                        initRequest.RemotePort,
                        ServerId);
                }

                return new LatencyPeer(initRequest.Protocol, initRequest.PhotonPeer);
            }

            if (log.IsDebugEnabled)
            {
                log.DebugFormat(
                    "incoming game peer at {0}:{1} from {2}:{3}", initRequest.LocalIP, initRequest.LocalPort, initRequest.RemoteIP, initRequest.RemotePort);
            }

            return this.CreateGamePeer(initRequest);
        }

        protected override ServerPeerBase CreateServerPeer(InitResponse initResponse, object state)
        {
            if (initResponse.ApplicationId == "LatencyMonitor")
            {
                // latency monitor
                LatencyMonitor peer = this.WorkloadController.OnLatencyMonitorPeerConnected(initResponse);
                return peer;
            }

            // master
            Thread.VolatileWrite(ref this.isReconnecting, 0);
            return this.MasterPeer = this.CreateMasterPeer(initResponse);
        }

        protected virtual void InitLogging()
        {
            LogManager.SetLoggerFactory(Log4NetLoggerFactory.Instance);
            GlobalContext.Properties["LogFileName"] = "GS" + this.ApplicationName;
            XmlConfigurator.ConfigureAndWatch(new FileInfo(Path.Combine(this.BinaryPath, "log4net.config")));
        }

        protected override void OnServerConnectionFailed(int errorCode, string errorMessage, object state)
        {
            var ipEndPoint = state as IPEndPoint;
            if (ipEndPoint == null)
            {
                log.ErrorFormat("Unknown connection failed with err {0}: {1}", errorCode, errorMessage);
                return;
            }

            if (ipEndPoint == this.MasterEndPoint)
            {
                if (this.isReconnecting == 0)
                {
                    log.ErrorFormat("Master connection failed with err {0}: {1}, serverId={2}", errorCode, errorMessage, ServerId);
                }
                else if (log.IsWarnEnabled)
                {
                    log.WarnFormat("Master connection failed with err {0}: {1}, serverId={2}", errorCode, errorMessage, ServerId);
                }

                this.ReconnectToMaster();
                return;
            }

            this.WorkloadController.OnLatencyMonitorConnectFailed(ipEndPoint, errorCode, errorMessage);
        }

        protected override void Setup()
        {
            Instance = this;
            this.InitLogging();

            log.InfoFormat("Setup: serverId={0}", ServerId);

            Protocol.AllowRawCustomValues = true;

            IPAddress ipAddress;
            if (PublicIPAddressReader.TryParsePublicIpAddress(GameServerSettings.Default.PublicIPAddress, out ipAddress) == false)
            {
                log.ErrorFormat("Failed to get public ip address: settings={0}", GameServerSettings.Default.PublicIPAddress);
            }

            this.PublicIpAddress = ipAddress;
            this.SetupFeedbackControlSystem();
            this.ConnectToMaster();

            CounterPublisher.DefaultInstance.AddStaticCounterClass(typeof(Lite.Diagnostics.Counter), this.ApplicationName);
            CounterPublisher.DefaultInstance.AddStaticCounterClass(typeof(Counter), this.ApplicationName);

            this.executionFiber = new PoolFiber();
            this.executionFiber.Start();
            this.executionFiber.ScheduleOnInterval(this.CheckGames, 60000, 60000); 
        }

        protected override void TearDown()
        {
            log.InfoFormat("TearDown: serverId={0}", ServerId);

            if (this.WorkloadController != null)
            {
                this.WorkloadController.Stop();
            }

            if (this.MasterPeer != null)
            {
                this.MasterPeer.Disconnect();
            }
        }

        /// <summary>
        ///  Sanity check to verify that game states are cleaned up correctly
        /// </summary>
        protected virtual void CheckGames()
        {
            var roomNames = GameCache.Instance.GetRoomNames(); 

            foreach (var roomName in roomNames)
            {
                Room room; 
                GameCache.Instance.TryGetRoomWithoutReference(roomName, out room);
                room.EnqueueMessage(new RoomMessage((byte)GameMessageCodes.CheckGame)); 
            }
        }

        protected void SetupFeedbackControlSystem()
        {
            IPEndPoint latencyEndpointTcp;
            IPEndPoint latencyEndpointUdp;

            if (string.IsNullOrEmpty(GameServerSettings.Default.LatencyMonitorAddress))
            {
                if (this.GamingTcpPort.HasValue == false)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Error("Could not start latency monitor because no tcp port is specified in the application configuration.");
                    }

                    return;
                }

                if (this.PublicIpAddress == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Error("Could not latency monitor because public ip adress could not be resolved.");
                    }

                    return;
                }

                latencyEndpointTcp = new IPEndPoint(this.PublicIpAddress, this.GamingTcpPort.Value);
            }
            else
            {
                if (Global.TryParseIpEndpoint(GameServerSettings.Default.LatencyMonitorAddress, out latencyEndpointTcp) == false)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.ErrorFormat(
                            "Could not start latency monitor because an invalid endpoint ({0}) is specified in the application configuration.", 
                            GameServerSettings.Default.LatencyMonitorAddress);
                    }

                    return;
                }
            }

            // 

            if (string.IsNullOrEmpty(GameServerSettings.Default.LatencyMonitorAddressUdp))
            {
                if (this.GamingUdpPort.HasValue == false)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Error("Could not latency monitor because no udp port is specified in the application configuration.");
                    }

                    return;
                }

                if (this.PublicIpAddress == null)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.Error("Could not latency monitor because public ip adress could not be resolved.");
                    }

                    return;
                }

                latencyEndpointUdp = new IPEndPoint(this.PublicIpAddress, this.GamingUdpPort.Value);
            }
            else
            {
                if (Global.TryParseIpEndpoint(GameServerSettings.Default.LatencyMonitorAddressUdp, out latencyEndpointUdp) == false)
                {
                    if (log.IsWarnEnabled)
                    {
                        log.ErrorFormat(
                            "Coud not start latency monitor because an invalid endpoint ({0}) is specified in the application configuration.",
                            GameServerSettings.Default.LatencyMonitorAddressUdp);
                    }

                    return;
                }
            }

            this.WorkloadController = new WorkloadController(
                this, this.PhotonInstanceName, "LatencyMonitor", latencyEndpointTcp, latencyEndpointUdp, 1, 1000);

            this.WorkloadController.Start();
        }

        #endregion
    }
}