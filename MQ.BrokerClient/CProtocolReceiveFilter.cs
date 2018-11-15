using MQ.Common;
using MQ.Common.Extensions;
using SuperSocket.ProtoBase;

namespace MQ.BrokerClient
{
    public class CProtocolReceiveFilter : FixedHeaderReceiveFilter<MQDataInfo>
    {
        public CProtocolReceiveFilter() : base(14)
        {

        }

        public override MQDataInfo ResolvePackage(IBufferStream bufferStream)
        {
            var buffer = bufferStream.Read();

            return MQTools.ResolveMQDataInfo(buffer);
        }

        protected override int GetBodyLengthFromHeader(IBufferStream bufferStream, int length)
        {
            if (length < 14)
                return 0;

            var header = new byte[14];
            int count = bufferStream.Read(header, 0, 14);

            byte high = header[13];
            //获取低位
            byte low = header[12];

            return MQTools.GetBodyLengthFromHeader(high, low);
        }
    }
}
