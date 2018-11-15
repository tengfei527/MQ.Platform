using MQ.Common;
using MQ.Common.Extensions;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MQ.CoreServer
{
    public class MqServerOperator
    {
        protected MQProtocolServer mqServer { get; set; }

        public MqServerOperator(MQProtocolServer server)
        {
            this.mqServer = server;
        }
        public void MqServer_NewSessionConnected(MQProtocolSession session)
        {
            Console.WriteLine(session.RemoteEndPoint.ToString() + " connected.");
        }

        public void MqServer_NewRequestReceived(MQProtocolSession session, MQDataInfo requestInfo)
        {
            try
            {
                if (requestInfo.Type == (byte)MessageType.ResAck)
                {
                    try
                    {
                        mqServer.Logger.Info($"客户端:【{session.RemoteEndPoint}】 接收{MessageType.ResAck} :【{requestInfo.ToString()}】");

                        mqServer.Logger.Info($"客户端:【{session.RemoteEndPoint}】确认回复消息剩余队列:【{session.AckMessageQueueCount}】");
                        //验证出队列
                        session.DequeueAckMessageQueue(requestInfo);
                    }
                    catch (Exception e)
                    {
                        mqServer.Logger.Error(requestInfo, new Exception("处理确认消息失败", e));
                    }
                }
                else  //确认应答
                {
                    if (requestInfo.Type == (byte)MessageType.Ack)
                    {
                        try
                        {
                            mqServer.Logger.Info($"客户端:【{session.RemoteEndPoint}】接收 {MessageType.Ack} :【{requestInfo.ToString()}】");
                            session.Send(MQTools.GetResAckMessage(requestInfo));
                            mqServer.Logger.Info("回应确认消息完成");
                        }
                        catch (Exception e)
                        {
                            mqServer.Logger.Error(requestInfo, new Exception($"发送至 【{session.RemoteEndPoint}】确认消息失败", e));
                        }
                    }

                    //解析消息类型
                    switch (requestInfo.Code)
                    {
                        case 1://Login
                            ProcessLogin(session, requestInfo);
                            break;
                        case 10://SendMessage
                            if (requestInfo.Type == (byte)MessageType.Ack)
                            {
                                if (session.AckMessageFilter(requestInfo))//过滤重复消息
                                    ProcessAckMessage(session, requestInfo);
                            }
                            else
                            {
                                ProcessMessage(session, requestInfo);
                            }
                            break;
                        //case 11://AckMessage
                        //消息队列处理
                        //break;
                        case 12://PullMessage
                            ProcessPullMessage(session, requestInfo);
                            break;
                        case 21://Subpackage
                            break;
                        case 31://BroadcastMessage
                            break;
                        case 41://TransferMessage
                            break;
                        case 100://Heartbeat
                            ProcessHeartbeat(session, requestInfo);
                            break;
                        case 200://GetForTopic
                            ProcessGetForTopic(session, requestInfo);
                            break;
                        case 201://CreateTopic
                            ProcessCreateTopic(session, requestInfo);
                            break;
                        case 202://DeleteTopic
                            break;
                        case 210:// GetForQueue
                            ProcessGetForQueue(session, requestInfo);
                            break;
                        case 211://AddQueue
                            break;
                        case 212://DeleteQueue
                            break;
                        case 220://GetForRoom
                            break;
                        case 221://AddRoom
                            break;
                        case 222://DeleteRoom
                            break;
                        default:
                            break;
                    }
                }

                Console.WriteLine("Receive From: " + session.RemoteEndPoint.ToString());
                Console.WriteLine("MessageKey:" + requestInfo.Key);
                Console.WriteLine("Data:" + requestInfo.ToString());
                Console.WriteLine("-------------------------------------------------------------");
            }
            catch (Exception e)
            {
                mqServer.Logger.Error(requestInfo, new Exception("消息处理流程失败", e));
            }
        }

        public void MqServer_SessionClosed(MQProtocolSession session, CSuperSocket.SocketBase.CloseReason value)
        {
            try
            {
                if (session.TopicMode.TopicDic.Count > 0)
                {
                    foreach (var r in session.TopicMode.TopicDic.Keys)//room
                    {
                        foreach (var t in session.TopicMode.TopicDic[r].Keys)//topic
                        {
                            foreach (var g in session.TopicMode.TopicDic[r][t])//tag
                            {
                                if (mqServer.TopicMessageQueueDict.ContainsKey(r) && mqServer.TopicMessageQueueDict[r].ContainsKey(t) && mqServer.TopicMessageQueueDict[r][t].ContainsKey(g))
                                    mqServer.TopicMessageQueueDict[r][t][g].Remove(session);
                                mqServer.TopicMessageQueueDict[r][t][g].TrimExcess();
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                mqServer.Logger.Error(session, new Exception("移除主题字典失败", e));
            }
            mqServer.Logger.Info(session.RemoteEndPoint.ToString() + " closed. reason:" + value);
        }
        #region 消息处理
        protected bool ProcessLogin(MQProtocolSession session, MQDataInfo mQDataInfo)
        {
            try
            {
                string data = mQDataInfo.Body.DecodeToString();
                mqServer.Logger.Info($"客户端:【{session.RemoteEndPoint}】{CommanCode.Login} :【{mQDataInfo}】登陆信息:【{data}】");
                //解析认证
                session.IsLogin = true;

                return true;
            }
            catch (Exception e)
            {
                mqServer.Logger.Error(mQDataInfo, new Exception("处理登陆消息失败", e));
                session.Close(CSuperSocket.SocketBase.CloseReason.InternalError);
                return false;
            }
        }

        protected bool ProcessMessage(MQProtocolSession session, MQDataInfo mQDataInfo)
        {
            try
            {
                string data = mQDataInfo.Body.DecodeToString();
                mqServer.Logger.Info($"客户端:【{session.RemoteEndPoint}】消息类型:【{mQDataInfo.Type}】{CommanCode.SendMessage}:【{mQDataInfo}】消息内容:【{data}】");

                var model = Newtonsoft.Json.JsonConvert.DeserializeObject<MQ.Common.Mode.MQModel>(data);
                //自己
                if (!(mqServer.TopicMessageQueueDict.ContainsKey("") && mqServer.TopicMessageQueueDict[""].ContainsKey(model.Topic) && mqServer.TopicMessageQueueDict[""][model.Topic].ContainsKey(model.Tag)))
                    return false;

                var tt = mqServer.TopicMessageQueueDict[""][model.Topic][model.Tag];

                byte[] buffer;
                mQDataInfo.GetSendByte(out buffer);
                List<MQProtocolSession> remove = new List<MQProtocolSession>();
                foreach (var t in tt.Where(d => d != session))
                {
                    bool r = t.TrySend(buffer, 0, buffer.Length);
                    //if (!r)
                    //{
                    //    //移除队列
                    //    remove.Add(t);
                    //}
                    mqServer.Logger.Info($"客户端:【{t.RemoteEndPoint}】房间:【默认】主题:【{model.Topic}】标签:【{model.Tag}】推送【普通】消息结果:【{r}】");
                }
                //if (remove.Count > 0)
                //{
                //    mqServer.Logger.Warn($"开始清理僵尸客户端关注主题……");

                //    remove.ForEach(d =>
                //    {
                //        mqServer.Logger.Warn($"客户端:【{d.RemoteEndPoint}】房间:【默认】主题:【{model.Topic}】标签:【{model.Tag}】推送【普通】消息失败，将被移除队列！");
                //        tt.Remove(d);
                //    });
                //}

                mqServer.TopicMessageQueueDict[""][model.Topic].TryUpdate(model.Tag, tt, tt);

                return true;
            }
            catch (Exception e)
            {
                mqServer.Logger.Error(mQDataInfo, new Exception("处理队列消息失败", e));

                session.Close(CSuperSocket.SocketBase.CloseReason.InternalError);

                return false;
            }
        }
        protected bool ProcessAckMessage(MQProtocolSession session, MQDataInfo mQDataInfo)
        {
            try
            {
                string data = mQDataInfo.Body.DecodeToString();

                mqServer.Logger.Info($"客户端:【{session.RemoteEndPoint}】消息类型:【{mQDataInfo.Type}】{CommanCode.SendMessage} :【{mQDataInfo}】消息内容:【{data}】");

                var model = Newtonsoft.Json.JsonConvert.DeserializeObject<MQ.Common.Mode.MQModel>(data);
                if (!(mqServer.TopicMessageQueueDict.ContainsKey("") && mqServer.TopicMessageQueueDict[""].ContainsKey(model.Topic) && mqServer.TopicMessageQueueDict[""][model.Topic].ContainsKey(model.Tag)))
                {
                    mqServer.Logger.Debug($"客户端:【{session.RemoteEndPoint}】消息房间【默认】主题:【{model.Topic}】标签:【{model.Tag}】没有消费者");
                    return false;
                }
                var tt = mqServer.TopicMessageQueueDict[""][model.Topic][model.Tag];

                List<MQProtocolSession> remove = new List<MQProtocolSession>();
                foreach (var t in tt.Where(d => d != session))
                {
                    t.EnqueueAckMessageQueue(mQDataInfo);
                    var r = t.TrySendAckMessage();
                    //if (!r)
                    //{
                    //    //移除队列
                    //    remove.Add(t);
                    //}
                    mqServer.Logger.Info($"客户端:【{t.RemoteEndPoint}】房间:【默认】主题:【{model.Topic}】 标签:【{model.Tag}】 推送【确认】消息结果:【{r}】");
                }
                //if (remove.Count > 0)
                //{
                //    mqServer.Logger.Warn($"开始清理僵尸客户端关注主题……");
                //    remove.ForEach(d =>
                //    {
                //        mqServer.Logger.Warn($"客户端:【{d.RemoteEndPoint}】房间:【默认】主题:【{model.Topic}】标签:【{model.Tag}】推送【确认】消息失败，将被移除队列！");
                //        tt.Remove(d);
                //    });
                //}
                mqServer.TopicMessageQueueDict[""][model.Topic].TryUpdate(model.Tag, tt, tt);

                return true;
            }
            catch (Exception e)
            {
                mqServer.Logger.Error(mQDataInfo, new Exception("处理队列确认消息失败", e));

                session.Close(CSuperSocket.SocketBase.CloseReason.InternalError);

                return false;
            }
        }
        protected bool ProcessHeartbeat(MQProtocolSession session, MQDataInfo mQDataInfo)
        {
            try
            {
                mqServer.Logger.Warn($"客户端:【{session.RemoteEndPoint}】{CommanCode.Heartbeat} : {mQDataInfo}】心跳信息");
                //byte[] buffer;
                //mQDataInfo.GetSendByte(out buffer);

                //session.TrySend(buffer, 0, buffer.Length);

                return true;
            }
            catch (Exception e)
            {
                mqServer.Logger.Error(mQDataInfo, new Exception("处理心跳消息失败", e));

                session.Close(CSuperSocket.SocketBase.CloseReason.InternalError);
                return false;
            }
        }

        protected bool ProcessCreateTopic(MQProtocolSession session, MQDataInfo mQDataInfo)
        {
            try
            {
                string data = mQDataInfo.Body.DecodeToString();

                mqServer.Logger.Info($"客户端:【{session.RemoteEndPoint}】消息类型:【{mQDataInfo.Type}】{CommanCode.CreateTopic} :【{mQDataInfo}】消息内容:【{data}】");

                var model = Newtonsoft.Json.JsonConvert.DeserializeObject<MQ.Common.Mode.TopicMode>(data);

                foreach (var r in model.TopicDic.Keys)//房间
                {
                    if (mqServer.TopicMessageQueueDict.ContainsKey(r))//存在房间
                    {
                        foreach (var t in model.TopicDic[r].Keys)
                        {
                            if (mqServer.TopicMessageQueueDict[r].ContainsKey(t))//存在主题
                            {
                                foreach (var g in model.TopicDic[r][t])//tag
                                {
                                    if (mqServer.TopicMessageQueueDict[r][t].Keys.Contains(g))//存在标签
                                    {
                                        if (!mqServer.TopicMessageQueueDict[r][t][g].Contains(session))
                                        {
                                            mqServer.TopicMessageQueueDict[r][t][g].Add(session);
                                        }
                                    }
                                    else //不存在标签
                                    {
                                        mqServer.TopicMessageQueueDict[r][t].TryAdd(g, new List<MQProtocolSession>() {
                                            session
                                        });
                                    }
                                }
                            }
                            else//不存在主题
                            {
                                ConcurrentDictionary<string, List<MQProtocolSession>> concurrentDictionaryq = new ConcurrentDictionary<string, List<MQProtocolSession>>();

                                foreach (var d in model.TopicDic[r][t])//tag
                                {
                                    concurrentDictionaryq.TryAdd(d, new List<MQProtocolSession>() {
                                            session
                                        });
                                }

                                mqServer.TopicMessageQueueDict[r].TryAdd(t, concurrentDictionaryq);
                            }
                        }
                    }
                    else//不存在房间
                    {
                        ConcurrentDictionary<string, ConcurrentDictionary<string, List<MQProtocolSession>>> concurrentDictionary = new ConcurrentDictionary<string, ConcurrentDictionary<string, List<MQProtocolSession>>>();
                        foreach (var k in model.TopicDic[r].Keys)
                        {
                            ConcurrentDictionary<string, List<MQProtocolSession>> concurrentDictionaryq = new ConcurrentDictionary<string, List<MQProtocolSession>>();
                            foreach (var d in model.TopicDic[r][k])
                            {
                                concurrentDictionaryq.TryAdd(d, new List<MQProtocolSession>() {
                                    session
                                });
                            }

                            concurrentDictionary.TryAdd(k, concurrentDictionaryq);
                        }

                        mqServer.TopicMessageQueueDict.TryAdd(r, concurrentDictionary);
                    }
                    //注意处理线程同步
                    if (session.TopicMode.TopicDic.ContainsKey(r))
                    {
                        foreach (var t in model.TopicDic[r].Keys)
                        {
                            if (session.TopicMode.TopicDic[r].ContainsKey(t))
                            {
                                foreach (var g in model.TopicDic[r][t])//tag
                                {
                                    if (!session.TopicMode.TopicDic[r][t].Contains(g))
                                    {
                                        session.TopicMode.TopicDic[r][t].Add(g);
                                    }
                                }
                            }
                            else
                            {
                                session.TopicMode.TopicDic[r].Add(t, model.TopicDic[r][t]);
                            }
                        }
                    }
                    else
                    {
                        session.TopicMode.TopicDic.Add(r, model.TopicDic[r]);
                    }

                }

                return true;
            }
            catch (Exception e)
            {
                mqServer.Logger.Error(mQDataInfo, new Exception("更新字典失败", e));

                session.Close(CSuperSocket.SocketBase.CloseReason.InternalError);
                return false;
            }
        }

        protected bool ProcessPullMessage(MQProtocolSession session, MQDataInfo mQDataInfo)
        {
            try
            {
                string data = mQDataInfo.Body.DecodeToString();

                mqServer.Logger.Info($"客户端:【{session.RemoteEndPoint}】消息类型:【{mQDataInfo.Type}】{CommanCode.PullMessage} :【{mQDataInfo}】消息内容:【{data}】");
                MQDataInfo msg;
                if (session.PeekAckMessageQueue(out msg))
                {
                    byte[] buffer;
                    msg.GetSendByte(out buffer);
                    bool r = session.TrySend(buffer, 0, buffer.Length);

                    mqServer.Logger.Info($"客户端:【{session.RemoteEndPoint}】拉取 【{msg}】【确认】消息结果:【{r}】队列:【{session.AckMessageQueueCount}】");
                }

                return true;
            }
            catch (Exception e)
            {
                mqServer.Logger.Error(mQDataInfo, new Exception("处理队列确认消息失败", e));

                session.Close(CSuperSocket.SocketBase.CloseReason.InternalError);

                return false;
            }
        }

        protected bool ProcessGetForTopic(MQProtocolSession session, MQDataInfo mQDataInfo)
        {
            try
            {
                string data = mQDataInfo.Body.DecodeToString();

                mqServer.Logger.Info($"客户端:【{session.RemoteEndPoint}】消息类型:【{mQDataInfo.Type}】{CommanCode.GetForTopic} :【{mQDataInfo}】消息内容:【{data}】");

                string msgs = Newtonsoft.Json.JsonConvert.SerializeObject(session.TopicMode);
                var msg = MQTools.GetSendMessage((ushort)CommanCode.GetForTopic, msgs, 0);
                var r = session.TrySend(msgs);

                mqServer.Logger.Info($"客户端:【{session.RemoteEndPoint}】请求主题 【{msgs}】【确认】消息结果:【{r}】");

                return true;
            }
            catch (Exception e)
            {
                mqServer.Logger.Error(mQDataInfo, new Exception("处理队列确认消息失败", e));

                session.Close(CSuperSocket.SocketBase.CloseReason.InternalError);

                return false;
            }
        }
        protected bool ProcessGetForQueue(MQProtocolSession session, MQDataInfo mQDataInfo)
        {
            try
            {
                string data = mQDataInfo.Body.DecodeToString();

                mqServer.Logger.Info($"客户端:【{session.RemoteEndPoint}】消息类型:【{mQDataInfo.Type}】{CommanCode.GetForQueue} :【{mQDataInfo}】消息内容:【{data}】");


                string msgs = ""; //Newtonsoft.Json.JsonConvert.SerializeObject(mqServer.TopicMessageQueueDict);
                var msg = MQTools.GetSendMessage((ushort)CommanCode.GetForQueue, msgs, 0);

                var r = session.TrySend(msg);

                mqServer.Logger.Info($"客户端:【{session.RemoteEndPoint}】请求队列 【{msgs}】【确认】消息结果:【{r}】");

                return true;
            }
            catch (Exception e)
            {
                mqServer.Logger.Error(mQDataInfo, new Exception("处理队列确认消息失败", e));

                session.Close(CSuperSocket.SocketBase.CloseReason.InternalError);

                return false;
            }
        }
        #endregion
    }
}
