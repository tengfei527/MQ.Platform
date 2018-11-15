using SuperSocket.ProtoBase;
using SuperSocket.SocketBase.Protocol;
using System;
using System.Text;

namespace MQ.Common
{
    public class MQDataInfo : IRequestInfo, IPackageInfo
    {
        /// <summary>
        /// 标识
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// 开始标识符 L = 2 字节
        /// </summary>
        public char Head { get; set; }
        /// <summary>
        /// 分片包编号 0=默认，0！= 片编号L=1字节
        /// </summary>
        public byte PID { get; set; }
        /// <summary>
        /// 消息类型 L = 1 字节
        /// default	0	普通消息
        /// Ack	1	需要确认消息
        /// ResAck	11	应答确认消息
        /// Sys	2	系统内部消息
        /// </summary>
        public byte Type { get; set; }
        /// <summary>
        /// 消息编码唯一 L = 4 字节
        /// </summary>
        public Int32 MID { get; set; }
        /// <summary>
        /// short指令类别，见指令类别表 L = 2字节
        /// </summary>
        public ushort Code { get; set; }
        /// <summary>
        /// 保留字段 L = 2 字节
        /// </summary>
        public ushort Reserved { get; set; }
        /// <summary>
        /// 数据长度 L = 2 字节
        /// </summary>
        public ushort Length { get; set; }
        /// <summary>
        /// 变长 数据内容 根据业务填充数据
        /// </summary>
        public byte[] Body { get; set; }
        /// <summary>
        /// 数据校验奇偶校验 L = 1 字节
        /// </summary>
        //public byte CS { get; set; }
        /// <summary>
        /// 结束标识 L = 2 字节
        /// </summary>
        public char End { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            if (Length > 0)
                Array.ForEach(Body, d => sb.Append(d.ToString("X0") + " "));
            return $"开始标识符:【{Head}】生产者类型:【{PID}】消息类型:【{Type}】消息编码:【{MID}】指令类别:【{Code}】保留字段:【{Reserved}】数据长度:【{Length}】数据内容:【{sb.ToString()}】结束标识:【{End}】";
        }

        public bool GetSendByte(out byte[] buffer)
        {
            buffer = new byte[14 + Length + 2];
            MQTools.CharToByte(Head, buffer);
            buffer[2] = PID;
            buffer[3] = Type;
            MQTools.ConvertIntToByteArray(MID, buffer, 4);
            MQTools.ConvertUShortToByteArray(Code, buffer, 8);
            MQTools.ConvertUShortToByteArray(Reserved, buffer, 10);
            MQTools.ConvertUShortToByteArray((ushort)Length, buffer, 12);

            if (Length > 0)
            {
                Array.Copy(Body, 0, buffer, 14, Length);
            }
            MQTools.CharToByte(End, buffer, Length + 14);

            return true;
        }
    }
}
