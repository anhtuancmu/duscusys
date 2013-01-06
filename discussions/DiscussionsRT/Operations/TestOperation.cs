using System.Collections.Generic;
using Photon.SocketServer.Rpc;
using Photon.SocketServer;

namespace Discussions.RTModel.Operations
{
    public class TestOperation : Operation
    {
        public TestOperation(IRpcProtocol protocol, OperationRequest request) : base(protocol, request)
        {
        }

        public OperationResponse GetResponse()
        {
            OperationResponse resp = new OperationResponse((byte) DiscussionOpCode.Test,
                                                           new Dictionary<byte, object>
                                                               {
                                                                   {(byte) DiscussionParamKey.Message, Message}
                                                               });
            return resp;
        }

        [DataMember(Code = (byte) DiscussionParamKey.Message, IsOptional = false)]
        public string Message { get; set; }
    }
}