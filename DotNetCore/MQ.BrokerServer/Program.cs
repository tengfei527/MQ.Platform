using MQ.Common;
using MQ.Common.Extensions;
using MQ.CoreServer;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Configuration;
using CSuperSocket.SocketBase.Config;

namespace MQ.BrokerServer
{
    class Program
    {

        static MQProtocolServer mqServer = new MQProtocolServer();

        static void Main(string[] args)
        {
            var config = ServerConfigManager.Instance;

            mqServer.Setup(new ServerConfig()
            {
                Ip = config.ip,
                Port = config.port,
                MaxRequestLength = config.maxRequestLength,
                SendBufferSize = config.sendBufferSize,
                MaxConnectionNumber = config.maxConnectionNumber,
                ReceiveBufferSize = config.receiveBufferSize,
                TextEncoding = config.textEncoding,
            });
            MqServerOperator mqServerOperator = new MqServerOperator(mqServer);

            mqServer.NewSessionConnected += mqServerOperator.MqServer_NewSessionConnected; //MqServer_NewSessionConnected;
            mqServer.NewRequestReceived += mqServerOperator.MqServer_NewRequestReceived;
            mqServer.SessionClosed += mqServerOperator.MqServer_SessionClosed;
            mqServer.Start();
            mqServer.Logger.Info($"Server is:{mqServer.Config.Ip}:{mqServer.Config.Port} stat is {mqServer.State.ToString()} ");

           
            while (true)
            {
                if (Console.ReadKey().KeyChar == 'q')
                {
                    mqServer.Stop();
                    mqServer.Dispose();
                    mqServer.Logger.Info("server is close");
                    return;
                }
                try
                {
                    mqServer.Logger.Info($"【{mqServer.Name}】当前客户端数量:【{mqServer.SessionCount}】启动时间:【{mqServer.StartedTime.ToString("yyyy-MM-dd HH:mm:ss,fff")}】");

                    System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
                    stringBuilder.Append("------------消息主题客户端列表------------\r\n");
                    foreach (var r in mqServer.TopicMessageQueueDict.Keys)//room
                    {
                        foreach (var t in mqServer.TopicMessageQueueDict[r].Keys)//topic
                        {
                            foreach (var g in mqServer.TopicMessageQueueDict[r][t].Keys)//tag
                            {
                                foreach (var s in mqServer.TopicMessageQueueDict[r][t][g])
                                {
                                    stringBuilder.Append($"房间:【{r}】\t主题:【{t}】    标签:【{g}】\t终端:【{s.RemoteEndPoint}】\t编号:【{s.SessionID}】\t当前消息队列数:【{s.AckMessageQueueCount}】接入时间:【{s.StartTime.ToString("yyyy-MM-dd HH:mm:ss,fff")}】\r\n");

                                }

                            }
                        }
                    }
                    mqServer.Logger.Info(stringBuilder.ToString());
                    mqServer.Logger.Info("------------------------------------");
                }
                catch (Exception e)
                {
                    mqServer.Logger.Error(e);
                }
            }
        }
    }
}
