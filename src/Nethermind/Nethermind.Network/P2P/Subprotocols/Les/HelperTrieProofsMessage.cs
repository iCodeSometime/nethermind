using System;
using System.Collections.Generic;
using System.Text;

namespace Nethermind.Network.P2P.Subprotocols.Les
{
    public class HelperTrieProofsMessage: P2PMessage
    {
        public override int PacketType { get; } = LesMessageCode.HelperTrieProofs;
        public override string Protocol { get; } = P2P.Protocol.Les;
        public long RequestId;
        public int BufferValue;

        public byte[][] ProofNodes;
        public byte[][] AuxiliaryData;

        public HelperTrieProofsMessage()
        {
        }

        public HelperTrieProofsMessage(byte[][] proofNodes, byte[][] auxiliaryData, long requestId, int bufferValue)
        {
            ProofNodes = proofNodes;
            AuxiliaryData = auxiliaryData;
            RequestId = requestId;
            BufferValue = bufferValue;
        }
    }
}
