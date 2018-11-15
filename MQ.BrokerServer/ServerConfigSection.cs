using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQ.BrokerServer
{
    public class ServerConfigSection : ConfigurationSection
    {
        /// <summary>
        /// 绑定ip地址
        /// </summary>
        [ConfigurationProperty("ip", DefaultValue = "Any", IsRequired = false, IsKey = false)]
        public string ip { get { return (string)this["ip"]; } set { this["ip"] = value; } }
        /// <summary>
        /// 绑定端口 52789
        /// </summary>
        [ConfigurationProperty("port", DefaultValue = "52789", IsRequired = false, IsKey = false)]
        public int port { get { return (int)this["port"]; } set { this["port"] = value; } }
        /// <summary>
        /// utf-8
        /// </summary>
        [ConfigurationProperty("textEncoding", DefaultValue = "utf-8", IsRequired = false, IsKey = false)]
        public string textEncoding { get { return (string)this["textEncoding"]; } set { this["textEncoding"] = value; } }
        /// <summary>
        /// 接收缓冲区
        /// </summary>
        [ConfigurationProperty("receiveBufferSize", DefaultValue = "1024", IsRequired = false, IsKey = false)]
        public int receiveBufferSize { get { return (int)this["receiveBufferSize"]; } set { this["receiveBufferSize"] = value; } }

        /// <summary>
        /// 发送缓冲区
        /// </summary>
        [ConfigurationProperty("sendBufferSize", DefaultValue = "1024", IsRequired = false, IsKey = false)]
        public int sendBufferSize { get { return (int)this["sendBufferSize"]; } set { this["sendBufferSize"] = value; } }

        /// <summary>
        /// 最大连接数
        /// </summary>
        [ConfigurationProperty("maxConnectionNumber", DefaultValue = "1000", IsRequired = false, IsKey = false)]
        public int maxConnectionNumber { get { return (int)this["maxConnectionNumber"]; } set { this["maxConnectionNumber"] = value; } }
        /// <summary>
        /// 最大请求包
        /// </summary>

        [ConfigurationProperty("maxRequestLength", DefaultValue = "1024", IsRequired = false, IsKey = false)]
        public int maxRequestLength { get { return (int)this["maxRequestLength"]; } set { this["maxConnectionNumber"] = value; } }
    }

    /// <summary>
    /// 配置信息工厂
    /// </summary>
    public class ServerConfigManager
    {
        /// <summary>
        /// 配置信息实体
        /// </summary>
        public static readonly ServerConfigSection Instance = GetSection();

        private static ServerConfigSection GetSection()
        {
            ServerConfigSection config = ConfigurationManager.GetSection("brokerServer") as ServerConfigSection;
            if (config == null)
            {
                config = new ServerConfigSection();
            }
            //throw new ConfigurationErrorsException();
            return config;
        }
    }
}
