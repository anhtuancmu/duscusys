using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Photon.SocketServer.Rpc;
using Photon.SocketServer;

namespace Discussions.RTModel.Operations
{
    /// <summary>
    /// client requests badge positions, server reports them 
    /// </summary>
    public class BadgeGeometryOperation : Operation 
    {
        public BadgeGeometryOperation(IRpcProtocol protocol, OperationRequest request)
            : base(protocol, request)
	    {
	    }
    }
}
