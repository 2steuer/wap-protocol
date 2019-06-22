using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Force.Crc32;
using SteuerSoft.Network.Protocol.Message.Base;
using SteuerSoft.Network.Protocol.Message.ValueTypes;
using SteuerSoft.Network.Protocol.Util;

namespace SteuerSoft.Network.Protocol.Message
{
    public class ReceivedWapMessage : WapMessage
    {
        public byte[] Payload { get; }

        public ReceivedWapMessage(byte[] data)
        {
            if (!Crc32Algorithm.IsValidWithCrcAtEnd(data))
            {
                throw new FormatException("CRC32 is wrong");
            }

            MessageType t = (MessageType)data[0];

            if (!Enum.IsDefined(typeof(MessageType), t))
            {
                throw new FormatException("Unknown MessageType");
            }

            Type = t;

            try
            {
                SequenceNumber = BitConverter.ToUInt64(data, 1);
            }
            catch (Exception e)
            {
                throw new FormatException("Could not parse sequence number");
            }
            
            try
            {
                // Parse Endpoint
                byte endPointLength = data[1];
                EndPoint = Encoding.UTF8.GetString(data, 2, endPointLength);

                // get payload
                ushort payloadLength = NumberConverter.ToUInt16(data, 2 + endPointLength);
                Payload = data
                    .Skip(2) // type byte, endpoint length byte
                    .Skip(sizeof(ulong))    // Sequence number
                    .Skip(endPointLength) // end point length
                    .Skip(2) // payload length bytes
                    .Take(payloadLength).ToArray();
            }
            catch (Exception)
            {
                // TODO: Some more handling maybe?
                throw;
            }

        }

        protected override byte[] GetPayload()
        {
            return Payload;
        }
    }
}
