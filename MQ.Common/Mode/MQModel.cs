using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQ.Common.Mode
{
    /// <summary>
    /// MQ消息实体
    /// </summary>
    public class MQModel
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public MQModel()
        {
            CreatDate = DateTime.Now;
        }
        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="topic">主题</param>
        /// <param name="tag">标签</param>
        /// <param name="message">消息</param>
        /// <param name="attachment">附加内容</param>
        public MQModel(string topic, string tag, string message, string attachment = "")
        {
            this.Topic = topic;
            this.Tag = tag;
            this.Message = message;
            this.Attachment = attachment;
            CreatDate = DateTime.Now;
        }
        /// <summary>
        /// 发送终端
        /// </summary>
        public string SenderId { get; set; }
        /// <summary>
        /// 主题
        /// </summary>
        public string Topic { get; set; }
        /// <summary>
        /// 标签
        /// </summary>
        public string Tag { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; }
        /// <summary>
        /// 附件内容
        /// </summary>
        public string Attachment { get; set; }
        /// <summary>
        /// 消息创建时间
        /// </summary>
        public DateTime CreatDate { get; set; }
    }
}
