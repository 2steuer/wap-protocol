using System;
using System.Collections.Generic;
using System.Text;
using Force.Crc32;
using SteuerSoft.Network.Protocol.Message.Interfaces;
using SteuerSoft.Network.Protocol.Message.ValueTypes;
using SteuerSoft.Network.Protocol.Util;

namespace SteuerSoft.Network.Protocol.Message.Base
{
    public abstract class WapMessageBase : IWapMessage
    {
        public MessageType Type { get; protected set; }
        public string EndPoint { get; protected set; }

        protected WapMessageBase(MessageType type, string endPoint)
        {
            Type = type;
            EndPoint = endPoint;
        }

        protected WapMessageBase()
        {
            
        }

        public byte[] GetBytes()
        {
            List<byte> data = new List<byte>();
            data.Add((byte)Type);

            var epBytes = Encoding.UTF8.GetBytes(EndPoint);

            if (epBytes.Length > byte.MaxValue)
            {
                throw new FormatException("EndPoint string is too long.");
            }

            data.Add((byte)epBytes.Length);
            data.AddRange(epBytes);

            var payload = GetPayload();

            if (payload.Length > ushort.MaxValue)
            {
                throw new FormatException("Payload size is too big");
            }

            data.AddRange(NumberConverter.GetBytes((ushort)payload.Length));
            data.AddRange(payload);

            byte[] bytes = new byte[data.Count + 4];
            data.CopyTo(bytes);
            Crc32Algorithm.ComputeAndWriteToEnd(bytes);

            return bytes;
        }

        protected abstract byte[] GetPayload();
    }
}
