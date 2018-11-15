using MQ.Common.Extensions;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQ.BrokerClient
{
    /// <summary>
    /// 消息参数
    /// </summary>
    public class MessageEventArgs
    {
        /// <summary>
        /// 标签
        /// </summary>
        public string Lable { get; set; }
        /// <summary>
        /// 消息
        /// </summary>
        public object Message { get; set; }
    }

    /// <summary>
    /// 消息
    /// </summary>
    /// <param name="sender">发送者</param>
    /// <param name="model">消息内容</param>
    public delegate void NofityMessageDelegate(object sender, MessageEventArgs model);

    public class MQHelp
    {
        /// <summary>
        /// 日志组件
        /// </summary>
        public static log4net.ILog Logger;
        /// <summary>
        /// 远程服务器地址
        /// </summary>
        public System.Net.IPEndPoint RemoteIPEndPoint { get; protected set; }
        /// <summary>
        /// 客户标识
        /// </summary>
        public string ClientId { get; protected set; }
        /// <summary>
        /// 消息到达通知委托
        /// </summary>
        public event EventHandler<Common.Mode.MQModel> OnMessageArrive;
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
        /// <summary>
        /// 是否连接
        /// </summary>
        public bool IsConnected
        {
            get
            {
                if (easyClient == null)
                    return false;
                else
                    return easyClient.IsConnected;
            }
        }
        /// <summary>
        /// 客户端连接
        /// </summary>
        protected EasyClient easyClient = new EasyClient();


        /// <summary>
        /// 消息编号
        /// </summary>
        protected int _instanceNumber;

        /// <summary>
        /// 取消控制变量
        /// </summary>
        protected System.Threading.CancellationTokenSource cts;
        /// <summary>
        /// 确认消息队列
        /// </summary>
        protected Queue<Common.MQDataInfo> AckMessageQueue = new Queue<Common.MQDataInfo>();

        /// <summary>
        /// 定时处理客户端确认消息队列
        /// </summary>
        protected int _interval = 1000;

        /// <summary>
        /// 控制开关
        /// </summary>
        protected bool IsProcessRun = true;
        /// <summary>
        /// 消息主题字典
        /// </summary>
        protected Common.Mode.TopicMode TopicMode = new Common.Mode.TopicMode()
        {
            TopicDic = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>>()
        };
        /// <summary>
        /// 发送ACK消息开关
        /// </summary>
        protected bool AckFlag = true;

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="clientId">客户端编号</param>
        /// <param name="brokerIp">服务器IP</param>
        /// <param name="brokerPort">服务器端口</param>
        public MQHelp(string clientId, string brokerIp, int brokerPort = 52789)
        {
            if (Logger == null)
            {
                Logger = log4net.LogManager.GetLogger(typeof(MQHelp));
            }
            easyClient.Initialize(new CProtocolReceiveFilter(), ProcessQueue);

            System.Net.IPAddress ip = System.Net.IPAddress.Parse(brokerIp);
            RemoteIPEndPoint = new System.Net.IPEndPoint(ip, brokerPort);
        }
        /// <summary>
        /// 添加机主题
        /// </summary>
        /// <param name="room">房间默认空</param>
        /// <param name="topic">主题</param>
        /// <param name="tag">标签</param>
        public MQHelp Subscribe(string topic, params string[] tag)
        {
            Logger.Debug($"关注主题:【{topic}】");
            string room = "";
            if (TopicMode.TopicDic.Keys.Contains(room))
            {
                if (TopicMode.TopicDic[room].ContainsKey(topic))
                {
                    foreach (var t in tag)
                    {
                        if (!TopicMode.TopicDic[room][topic].Contains(t))
                            TopicMode.TopicDic[room][topic].Add(t);
                    }
                }
                else
                {
                    TopicMode.TopicDic[room].Add(topic, tag.ToList());
                }
            }
            else
            {
                TopicMode.TopicDic.Add(room, new Dictionary<string, List<string>>() {
                    { topic,tag.ToList()}
                });
            }

            return this;
        }

        /// <summary>
        /// 启动
        /// </summary>
        /// <returns>启动结果</returns>
        public bool Start()
        {
            try
            {
                Logger.Debug($"客户端开始启动");

                if (easyClient.IsConnected || (cts != null && !cts.IsCancellationRequested))
                {
                    Logger.Debug($"客户端重复启动已屏蔽");
                    return true;
                }
                cts = new System.Threading.CancellationTokenSource();
                IsProcessRun = true;
                Task task = new Task(() => ProcessConnect(cts.Token), cts.Token);
                task.Start();
                Task taskQueue = new Task(() => ProcessAckQueue(cts.Token), cts.Token);
                taskQueue.Start();
                Logger.Debug($"客户端启动完成");
                return true;
            }
            catch (Exception e)
            {
                Logger.Error("启动消息客户端失败", e);
                return false;
            }
        }

        /// <summary>
        /// 关闭
        /// </summary>
        public void Shutdown()
        {
            Logger.Debug($"客户端开始关闭");
            IsProcessRun = false;
            if (easyClient.IsConnected)
            {
                cts.Cancel();
            }
            else
            {
                easyClient.Close();
            }
            AckMessageQueue.Clear();
            Logger.Debug($"客户端结束关闭");
        }
        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="mq">消息实体内容</param>
        /// <returns></returns>
        public bool SendMessage(Common.Mode.MQModel mq)
        {
            try
            {

                string str = Newtonsoft.Json.JsonConvert.SerializeObject(mq);
                System.Threading.Interlocked.Increment(ref _instanceNumber);
                Logger.Info($"发送【普通】消息 编号:【{_instanceNumber}】内容:【{str}】");
                var msg = Common.MQTools.GetSendMessage((byte)Common.CommanCode.SendMessage, str, _instanceNumber, mqType: 0);

                easyClient.Send(msg);

                return true;
            }
            catch (Exception e)
            {
                System.Threading.Interlocked.Decrement(ref _instanceNumber);
                Logger.Error(mq, e);
                return false;
            }
        }

        /// <summary>
        /// 发送消息
        /// </summary>
        /// <param name="mq">消息实体内容</param>
        /// <param name="messageType">消息类型</param>
        /// <returns></returns>
        public bool SendMessage(Common.Mode.MQModel mq, Common.MessageType messageType)
        {
            try
            {
                System.Threading.Interlocked.Increment(ref _instanceNumber);

                if (messageType == Common.MessageType.Ack)
                {
                    string str = Newtonsoft.Json.JsonConvert.SerializeObject(mq);
                    AckMessageQueue.Enqueue(Common.MQTools.GetMQDataInfoMessage((byte)Common.CommanCode.SendMessage, str, _instanceNumber, mqType: 1));
                    Logger.Debug($"发送【确认】消息 编号:【{_instanceNumber}】内容:【{str}】放入消息队列【{AckMessageQueue.Count}】");
                    //消息队列获取消息
                    //var msg = AckMessageQueue.Peek();
                    //byte[] buffer;
                    //msg.GetSendByte(out buffer);
                    //Logger.Info($"发送【确认】消息 内容:【{msg}】当前队列长度:【{AckMessageQueue.Count}】");
                    //easyClient.Send(buffer);
                }
                else
                {
                    string str = Newtonsoft.Json.JsonConvert.SerializeObject(mq);
                    Logger.Info($"发送【普通】消息 编号:【{_instanceNumber}】内容:【{str}】");
                    var msg = Common.MQTools.GetSendMessage((byte)Common.CommanCode.SendMessage, str, _instanceNumber, mqType: (byte)messageType);
                    easyClient.Send(msg);
                }

                return true;
            }
            catch (Exception e)
            {
                System.Threading.Interlocked.Decrement(ref _instanceNumber);
                Logger.Error(mq, e);
                return false;
            }
        }
        /// <summary>
        /// 处理连接
        /// </summary>
        /// <param name="ct">信号量</param>
        public void ProcessConnect(System.Threading.CancellationToken ct)
        {
            int count = 60;
            Logger.Debug("定时处理客户端连接服务开始运行");
            while (IsProcessRun)
            {
                try
                {
                    if (ct.IsCancellationRequested)
                        break;

                    if (!easyClient.IsConnected)
                    {
                        Logger.Debug($"客户端状态不正常: {easyClient.IsConnected} 即将重启启动 服务地址:【{RemoteIPEndPoint.ToString()}】"); AckMessageQueue.Clear();
                        var t = easyClient.ConnectAsync(RemoteIPEndPoint);
                        Task.WaitAll(t);
                        if (t.Result)
                        {
                            string str = Newtonsoft.Json.JsonConvert.SerializeObject(TopicMode);
                            System.Threading.Interlocked.Increment(ref _instanceNumber);
                            AckMessageQueue.Enqueue(Common.MQTools.GetMQDataInfoMessage((byte)Common.CommanCode.CreateTopic, str, _instanceNumber, mqType: 1));
                            Logger.Info($"客户端重新启动:【成功】添加主题消息队列【{AckMessageQueue.Count}】,编号:【{_instanceNumber}】内容:【{str}】");
                            count = 60;
                            continue;
                        }
                        Logger.Debug($"客户端状态不正常:【重启失败】 服务地址:【{RemoteIPEndPoint.ToString()}】");
                    }
                    else if (count == 60)
                    {
                        if (ct.IsCancellationRequested)
                            break;

                        AckFlag = true;

                        //Logger.Debug("开始定时处理客户端确认消息队列");
                        //if (AckMessageQueue.Count > 0)
                        //{
                        //    var model = AckMessageQueue.Peek();
                        //    Logger.Info($"定时处理:【{easyClient.LocalEndPoint}】确认消息:【{model.ToString()}】");
                        //    easyClient.Send(Common.MQTools.GetSendMessageByte(model));
                        //}
                        //Logger.Debug("结束定时处理客户端确认消息队列");
                    }
                    count--;
                    if (count % 20 == 0 && easyClient.IsConnected)//心跳
                    {
                        Logger.Debug($"客户端:【{easyClient.LocalEndPoint}】开始推送心跳消息");
                        easyClient.Send(Common.MQTools.GetSendMessage((byte)Common.CommanCode.Heartbeat, "", 0));
                        Logger.Debug("结束推送心跳消息");
                    }
                    if (count <= 0)
                    {
                        count = 60;
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("处理确认消息失败", e);
                }
                if (ct.IsCancellationRequested)
                    break;

                System.Threading.Thread.Sleep(this.Interval);
            }
            Logger.Debug("定时处理客户端连接服务结束运行");
        }
        /// <summary>
        /// 处理确认队列
        /// </summary>
        /// <param name="ct">信号量</param>
        public void ProcessAckQueue(System.Threading.CancellationToken ct)
        {
            Logger.Debug("定时处理客户端确认消息服务开始运行");
            while (IsProcessRun)
            {
                try
                {
                    if (ct.IsCancellationRequested)
                        break;

                    if (easyClient.IsConnected)
                    {
                        if (ct.IsCancellationRequested)
                            break;

                        if (AckMessageQueue.Count > 0 && AckFlag)
                        {
                            Logger.Debug("开始定时处理客户端确认消息队列");
                            var model = AckMessageQueue.Peek();
                            Logger.Info($"定时处理:【{easyClient.LocalEndPoint}】确认消息:【{model.ToString()}】");
                            easyClient.Send(Common.MQTools.GetSendMessageByte(model));
                            AckFlag = false;
                            Logger.Debug("结束定时处理客户端确认消息队列");
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error("处理确认消息失败", e);
                }
                if (ct.IsCancellationRequested)
                    break;

                System.Threading.Thread.Sleep(1);
            }
            Logger.Debug("定时处理客户端确认消息服务结束运行");
        }
        /// <summary>
        /// 处理消息队列
        /// </summary>
        /// <param name="requestInfo">消息包</param>
        public void ProcessQueue(Common.MQDataInfo requestInfo)
        {
            //确认应答
            if (requestInfo.Type == (byte)Common.MessageType.ResAck)
            {
                Logger.Info($"客户端:【{easyClient.LocalEndPoint}】接收 {Common.MessageType.ResAck} :【{requestInfo.ToString()}】");

                if (AckMessageQueue.Count > 0)
                {
                    var model = AckMessageQueue.Peek();
                    if (requestInfo.MID == model.MID && requestInfo.Code == model.Code)
                    {
                        //移除对象
                        AckMessageQueue.Dequeue();
                        Logger.Info($"客户端:【{easyClient.LocalEndPoint}】确认回复消息剩余队列:【{AckMessageQueue.Count}】");

                        //if (AckMessageQueue.Count > 0)
                        //{
                        AckFlag = true;
                        //model = AckMessageQueue.Peek();
                        ////立即发送？
                        //easyClient.Send(Common.MQTools.GetSendMessageByte(model));
                        //Logger.Info($"客户端:【{easyClient.LocalEndPoint}】发送需【确认】消息 内容:【{model.ToString()}】");
                        //}
                    }
                }
            }
            else
            {
                if (requestInfo.Type == (byte)Common.MessageType.Ack)
                {
                    try
                    {
                        Logger.Info($"客户端:【{easyClient.LocalEndPoint}】接收{Common.MessageType.Ack} :【{requestInfo.ToString()}】");
                        easyClient.Send(Common.MQTools.GetResAckMessage(requestInfo));
                        Logger.Info("回应确认消息完成");
                    }
                    catch (Exception e)
                    {
                        Logger.Error(requestInfo, new Exception($"【{easyClient.LocalEndPoint}】发送至【{RemoteIPEndPoint}】确认消息失败", e));
                    }
                }

                if (requestInfo.Code == 10)//SendMessage
                {
                    //委托通知
                    string data = requestInfo.Body.DecodeToString();
                    Logger.Info($"客户端:【{easyClient.LocalEndPoint}】接收【普通】消息 内容:【{requestInfo.ToString()}】消息:【{data}】");

                    var model = Newtonsoft.Json.JsonConvert.DeserializeObject<MQ.Common.Mode.MQModel>(data);
                    //通知
                    //OnMessageArrive?.BeginInvoke(this, model, null, null);
                    OnMessageArrive?.Invoke(this, model);
                }
            }
        }
    }
}
