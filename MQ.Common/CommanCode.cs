using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQ.Common
{
    /// <summary>
    /// 协议类型
    /// </summary>
    public enum CommanCode : ushort
    {
        /// <summary>
        /// 登陆
        /// </summary>
        Login = 1,
        /// <summary>
        /// 发送消息
        /// </summary>
        SendMessage = 10,
        /// <summary>
        /// 确认消息
        /// </summary>
        //AckMessage = 11,
        /// <summary>
        /// 拉取消息
        /// </summary>
        PullMessage = 12,
        /// <summary>
        /// 发送分包消息
        /// </summary>
        Subpackage = 21,
        /// <summary>
        /// 广播消息
        /// </summary>
        BroadcastMessage = 31,
        /// <summary>
        /// 中转消息
        /// </summary>
        TransferMessage = 41,
        /// <summary>
        /// 心跳
        /// </summary>
        Heartbeat = 100,
        /// <summary>
        /// 获取消费主题
        /// </summary>
        GetForTopic = 200,
        /// <summary>
        /// 创建主题
        /// </summary>
        CreateTopic = 201,
        /// <summary>
        /// 删除主题
        /// </summary>
        DeleteTopic = 202,
        /// <summary>
        /// 获取队列
        /// </summary>
        GetForQueue = 210,
        /// <summary>
        /// 新增队列
        /// </summary>
        AddQueue = 211,
        /// <summary>
        /// 删除队列
        /// </summary>
        DeleteQueue = 212,
        /// <summary>
        /// 获取房间
        /// </summary>
        GetForRoom = 220,
        /// <summary>
        /// 创建房间
        /// </summary>
        AddRoom = 221,
        /// <summary>
        /// 删除房间
        /// </summary>
        DeleteRoom = 222,

    }
}
