using SuperSocket.Common;
using SuperSocket.Facility.Protocol;
using System;

namespace MQ.Common
{
    public class MQProtocolReceiveFilter : FixedHeaderReceiveFilter<MQDataInfo>
    {
        public MQProtocolReceiveFilter() : base(14)
        {

        }

        protected override int GetBodyLengthFromHeader(byte[] header, int offset, int length)
        {
            //length为头部(包含两字节的length)长度
            //获取高位
            byte high = header[offset + length - 1];
            //获取低位
            byte low = header[offset + length - 2];

            return MQTools.GetBodyLengthFromHeader(high, low);
        }

        protected override MQDataInfo ResolveRequestInfo(ArraySegment<byte> header, byte[] bodyBuffer, int offset, int length)
        {
            //MQDataInfo mQData = new MQDataInfo();
            //mQData.Head = MQTools.ByteToChar(header.Array);
            //mQData.PID = header.Array[2];
            //mQData.Type = header.Array[3];
            //mQData.MID = BitConverter.ToInt32(header.Array, 4);
            //mQData.Code = BitConverter.ToUInt16(header.Array, 8);
            //mQData.Reserved = BitConverter.ToUInt16(header.Array, 10);
            //mQData.Length = BitConverter.ToUInt16(header.Array, 12);

            //mQData.Data = bodyBuffer.CloneRange(offset, length - 3);
            //mQData.CS = bodyBuffer[offset + mQData.Length];
            //mQData.End = MQTools.ByteToChar(bodyBuffer, offset + mQData.Length + 1);
            return MQTools.ResolveMQDataInfo(header.Array, bodyBuffer, offset, length);

            //MQProtocolRequestInfo res = new MQProtocolRequestInfo(mQData);
        }


    }
}
