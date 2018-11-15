using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQ.Common
{
    /// <summary>
    /// 消息类型
    /// </summary>
    public enum MessageType : byte
    {
        /// <summary>
        /// 普通消息
        /// </summary>
        Default = 0,
        /// <summary>
        /// 需要确认消息
        /// </summary>
        Ack = 1,
        /// <summary>
        /// 应答确认消息
        /// </summary>
        ResAck = 11,
        /// <summary>
        /// 系统内部消息
        /// </summary>
        Sys = 2
    }
}
