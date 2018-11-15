using MQ.Common;
using SuperSocket.SocketBase;
using System;
using System.Collections.Generic;

namespace MQ.CoreServer
{
    [Serializable]
    public class MQProtocolSession : AppSession<MQProtocolSession, MQDataInfo>
    {
        /// <summary>
        /// 消息主题列表
        /// </summary>
        public Common.Mode.TopicMode TopicMode;
        /// <summary>
        /// 确认消息队列
        /// </summary>
        protected Queue<MQDataInfo> AckMessageQueue = new Queue<MQDataInfo>();
        /// <summary>
        /// 队列长度
        /// </summary>
        public int AckMessageQueueCount { get { return AckMessageQueue.Count; } }
        /// <summary>
        /// 是否验证登陆
        /// </summary>
        public bool IsLogin { get; set; }
        /// <summary>
        /// 客户信息
        /// </summary>
        public MQ.Common.Mode.LoginModel LoginModel { get; set; }
        /// <summary>
        /// 窗口大小
        /// </summary>
        public int WindowCount = 100;
        /// <summary>
        /// 最后一次触发ACK消息时间
        /// </summary>
        public DateTime PeekTime { get; protected set; }
        /// <summary>
        /// 当前回复消息编号
        /// </summary>
        public int MID { get; protected set; }
        /// <summary>
        /// 当前回复消息编码
        /// </summary>
        public int Code { get; protected set; }
        /// <summary>
        /// 当前接收确认消息编号
        /// </summary>
        public int ReciveAckMID { get; protected set; }
        /// <summary>
        /// 当前接收确认消息编码
        /// </summary>
        public int ReciveAckCode { get; protected set; }
        public MQProtocolSession()
        {
            TopicMode = new Common.Mode.TopicMode()
            {
                TopicDic = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>>()
            };
            PeekTime = DateTime.Now;
        }
        protected override void OnSessionStarted()
        {
            base.OnSessionStarted();
        }

        protected override void HandleUnknownRequest(MQDataInfo requestInfo)
        {
            base.HandleUnknownRequest(requestInfo);
        }

        protected override void HandleException(Exception e)
        {
            base.HandleException(e);
        }

        protected override void OnSessionClosed(CloseReason reason)
        {
            base.OnSessionClosed(reason);
        }

        /// <summary>
        /// 加入队列
        /// </summary>
        /// <param name="mQDataInfo"></param>
        public void EnqueueAckMessageQueue(MQDataInfo mQDataInfo)
        {
            AckMessageQueue.Enqueue(mQDataInfo);
        }
        /// <summary>
        /// 获取当前消息
        /// </summary>
        /// <param name="mQDataInfo"></param>
        /// <returns></returns>
        public bool PeekAckMessageQueue(out MQDataInfo mQDataInfo)
        {
            if (AckMessageQueue.Count > 0)
            {
                mQDataInfo = AckMessageQueue.Peek();
                return true;
            }
            else
            {
                mQDataInfo = null;
                return false;
            }
        }
        /// <summary>
        /// 移除队列并返回当前队列消息
        /// </summary>
        /// <param name="mQDataInfo">当前消息</param>
        /// <returns></returns>
        public void DequeueAckMessageQueue(MQDataInfo checkInfo)
        {
            if (checkInfo != null && AckMessageQueue.Count > 0)
            {
                var mQDataInfo = AckMessageQueue.Peek();
                if (mQDataInfo.MID == checkInfo.MID && mQDataInfo.Code == checkInfo.Code)
                {
                    AckMessageQueue.Dequeue();
                    TrySendAckMessage();
                }
            }
        }

        public bool TrySendAckMessage()
        {
            bool result = false;
            if (AckMessageQueue.Count == 0)
            {
                this.Logger.Info($"推送确认消息:【{this.RemoteEndPoint}】当前队列:【0】");
                return false;
            }
            var model = AckMessageQueue.Peek();
            if ((DateTime.Now - PeekTime).TotalMilliseconds > WindowCount || !(MID == model.MID && Code == model.Code))//窗口刻度
            {
                var buff = Common.MQTools.GetSendMessageByte(model);
                result = this.TrySend(buff, 0, buff.Length);
                if (result)
                {
                    MID = model.MID;
                    Code = model.Code;
                    PeekTime = DateTime.Now;
                }

                this.Logger.Info($"推送确认消息: 【{this.RemoteEndPoint}】结果:【{result}】确认消息:【{model.ToString()}】");
            }
            else
            {
                this.Logger.Info($"客户端:【{this.RemoteEndPoint}】降低发送频率");
            }

            return result;
        }

        public bool AckMessageFilter(MQDataInfo mQDataInfo)
        {
            if (mQDataInfo.Code == ReciveAckCode && mQDataInfo.MID <= ReciveAckMID)
                return false;
            else
            {
                ReciveAckMID = mQDataInfo.Code;
                ReciveAckMID = mQDataInfo.MID;
            }
            return true;
        }

    }
}
