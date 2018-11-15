using MQ.Common;
using SuperSocket.SocketBase;
using SuperSocket.SocketBase.Protocol;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace MQ.CoreServer
{
    public class MQProtocolServer : AppServer<MQProtocolSession, MQDataInfo>
    {
        /// <summary>
        /// 登陆信息表
        /// </summary>
        public readonly ConcurrentDictionary<string, string> LoginInfoDic = new ConcurrentDictionary<string, string>();
        /// <summary>
        /// 当前消息主题字典表
        /// </summary>
        public readonly ConcurrentDictionary<string /*room*/, ConcurrentDictionary<string /*topic*/, ConcurrentDictionary<string /*tag*/, List<MQProtocolSession>>>> TopicMessageQueueDict = new ConcurrentDictionary<string, ConcurrentDictionary<string, ConcurrentDictionary<string, List<MQProtocolSession>>>>();
        /// <summary>
        /// 定时处理客户端确认消息队列
        /// </summary>
        private int _interval = 1000;
        /// <summary>
        /// 定时处理客户端确认消息队列 1000
        /// </summary>
        public int Interval
        {
            get
            {
                return _interval;
            }
            set
            {
                if (value < 10)
                    return;
                _interval = 1000;
            }
        }

        private bool IsProcessRun = true;
        /// <summary>
        /// 取消控制变量
        /// </summary>
        private System.Threading.CancellationTokenSource cts;
        public MQProtocolServer() : base(new DefaultReceiveFilterFactory<MQProtocolReceiveFilter, MQDataInfo>())
        {

        }

        public override bool Start()
        {
            if (this.State == ServerState.Running || (cts != null && !cts.IsCancellationRequested))
                return true;

            cts = new System.Threading.CancellationTokenSource();

            bool rs = base.Start();
            IsProcessRun = true;
            Task task = new Task(() => ProcessAckQueue(cts.Token), cts.Token);
            task.Start();

            return rs;
        }

        public override void Stop()
        {
            IsProcessRun = false;
            if (cts != null)
            {
                cts.Cancel();
            }

            base.Stop();
        }

        public void ProcessAckQueue(System.Threading.CancellationToken ct)
        {
            int count = 60;
            this.Logger.Debug("定时处理客户端确认消息服务开始运行");
            while (IsProcessRun)
            {
                try
                {
                    if (ct.IsCancellationRequested)
                        break;

                    if (this.State != SuperSocket.SocketBase.ServerState.Running)
                    {
                        this.Logger.Debug($"消息服务状态不正常: {this.State} 即将重启启动");
                        this.TopicMessageQueueDict.Clear();
                        if (base.Start())
                        {
                            this.Logger.Debug($"消息服务重新启动: {this.State}");
                            count = 60;
                            continue;
                        }
                        this.Logger.Debug($"消息服务重新启动失败: {this.State}");
                    }
                    else if (count == 60)
                    {
                        if (ct.IsCancellationRequested)
                            break;
                        this.Logger.Debug("开始定时处理客户端确认消息队列");
                        foreach (var s in this.GetSessions(d => d.AckMessageQueueCount > 0))
                        {
                            try
                            {
                                //MQDataInfo model;
                                //if (s.PeekAckMessageQueue(out model))
                                //{
                                //    var buff = Common.MQTools.GetSendMessageByte(model);
                                //    bool r = s.TrySend(buff, 0, buff.Length);
                                //    this.Logger.Info($"定时处理: 【{s.RemoteEndPoint}】结果:【{r}】确认消息:【{model.ToString()}】");
                                //}

                                var r = s.TrySendAckMessage();

                            }
                            catch (System.Exception e)
                            {
                                this.Logger.Error(s, new System.Exception("定时客户端确认消息处理失败", e));
                            }
                        }
                        this.Logger.Debug("结束定时处理客户端确认消息队列");
                    }
                    count--;
                    if (count % 20 == 0)//心跳
                    {
                        // easyClient.Send(Common.MQTools.GetSendMessage((byte)Common.CommanCode.Heartbeat, "", 0));
                    }
                    if (count <= 0)
                    {
                        count = 60;
                    }
                }
                catch (System.Exception e)
                {
                    this.Logger.Error(this, new System.Exception("定时处理客户端确认消息队列异常", e));
                }
                if (ct.IsCancellationRequested)
                    break;

                System.Threading.Thread.Sleep(Interval);
            }

            this.Logger.Debug("定时处理客户端确认消息服务结束运行");
        }
    }
}
