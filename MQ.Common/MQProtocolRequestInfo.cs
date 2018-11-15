using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Protocol;

namespace MQ.Common
{
    public class MQProtocolRequestInfo : RequestInfo<MQData>, IPackageInfo
    {

        public MQProtocolRequestInfo(MQData mQData)
        {
            Initialize(mQData.Key, mQData);
        }
    }
}
