using CSuperSocket.Common;
using System;
using System.Text;

namespace MQ.Common
{
    public class MQTools
    {
        public static bool CharToByte(char c, byte[] buffer, int offset = 0)
        {
            if (buffer.Length - offset < 2)
                return false;
            try
            {
                buffer[offset] = (byte)((c & 0xFF00) >> 8);
                buffer[offset + 1] = (byte)(c & 0xFF);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ConvertIntToByteArray(Int32 m, byte[] arry, int offset = 0)
        {
            if (arry == null) return false;
            if (arry.Length - offset < 4) return false;
            try
            {
                arry[offset] = (byte)(m & 0xFF);
                arry[offset + 1] = (byte)((m & 0xFF00) >> 8);
                arry[offset + 2] = (byte)((m & 0xFF0000) >> 16);
                arry[offset + 3] = (byte)((m << 24) & 0xFF);

                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool ConvertUShortToByteArray(ushort m, byte[] arry, int offset = 0)
        {
            if (arry == null) return false;
            if (arry.Length - offset < 4) return false;
            try
            {
                var a = BitConverter.GetBytes(m);
                arry[offset] = (byte)(m & 0xFF);
                arry[offset + 1] = (byte)((m & 0xFF00) >> 8);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static char ByteToChar(byte high, byte low)
        {
            char c = (char)(((high & 0xFF) << 8) | (low & 0xFF));
            return c;
        }

        public static char ByteToChar(byte[] b, int offset = 0)
        {
            char c = (char)(((b[offset] & 0xFF) << 8) | (b[offset + 1] & 0xFF));
            return c;
        }

        public static int GetBodyLengthFromHeader(byte high, byte low)
        {

            int len = (int)high * 256 + low;
            return len + 2;//结尾有2个字节
        }

        public static MQDataInfo ResolveMQDataInfo(byte[] header, byte[] bodyBuffer, int offset, int length)
        {
            MQDataInfo mQData = new MQDataInfo();
            mQData.Head = MQTools.ByteToChar(header[0], header[1]);
            mQData.PID = header[2];
            mQData.Type = header[3];
            mQData.MID = BitConverter.ToInt32(header, 4);
            mQData.Code = BitConverter.ToUInt16(header, 8);
            mQData.Reserved = BitConverter.ToUInt16(header, 10);
            mQData.Length = BitConverter.ToUInt16(header, 12);
            if (mQData.Length > 0)
            {
                mQData.Body = bodyBuffer.CloneRange(offset, mQData.Length);
            }
            //mQData.CS = bodyBuffer[offset + mQData.Length];
            mQData.End = MQTools.ByteToChar(bodyBuffer[offset + mQData.Length], bodyBuffer[offset + mQData.Length + 1]);

            return mQData;
        }

        public static MQDataInfo ResolveMQDataInfo(byte[] buffer)
        {
            MQDataInfo mQData = new MQDataInfo();
            mQData.Head = MQTools.ByteToChar(buffer[0], buffer[1]);
            mQData.PID = buffer[2];
            mQData.Type = buffer[3];
            mQData.MID = BitConverter.ToInt32(buffer, 4);
            mQData.Code = BitConverter.ToUInt16(buffer, 8);
            mQData.Reserved = BitConverter.ToUInt16(buffer, 10);
            mQData.Length = BitConverter.ToUInt16(buffer, 12);
            if (mQData.Length > 0)
            {
                mQData.Body = buffer.CloneRange(14, mQData.Length);
            }
            // mQData.CS = buffer[14 + mQData.Length];
            mQData.End = MQTools.ByteToChar(buffer[14 + mQData.Length], buffer[14 + mQData.Length + 1]);

            return mQData;
        }


        //ArraySegment

        public static ArraySegment<byte> GetResAckMessage(MQDataInfo mQDataInfo)
        {
            byte[] msg = new byte[16];
            MQTools.CharToByte(mQDataInfo.Head, msg);
            msg[2] = mQDataInfo.PID;
            msg[3] = (byte)MessageType.ResAck;
            MQTools.ConvertIntToByteArray(mQDataInfo.MID, msg, 4);
            MQTools.ConvertUShortToByteArray(mQDataInfo.Code, msg, 8);
            MQTools.ConvertUShortToByteArray(mQDataInfo.Reserved, msg, 10);

            MQTools.CharToByte(mQDataInfo.End, msg, 14);

            return new ArraySegment<byte>(msg);
        }

        public static ArraySegment<byte> GetSendMessage(ushort mqCode, string message, int mqId, Encoding encoding = null, byte sendPid = 1, byte mqType = 0, ushort Reserved = 0, char head = '&', char end = '$')
        {
            ArraySegment<byte> arraySegment = new ArraySegment<byte>(GetSendMessageByte(mqCode, message, mqId, encoding, sendPid, mqType, Reserved, head, end));

            return arraySegment;
        }

        public static byte[] GetSendMessageByte(ushort mqCode, string message, int mqId, Encoding encoding = null, byte sendPid = 0, byte mqType = 0, ushort Reserved = 0, char head = '&', char end = '$')
        {
            if (encoding == null)
                encoding = Encoding.UTF8;

            var data = encoding.GetBytes(message);//大包分包方案

            return GetSendMessageByte(mqCode, data, mqId, sendPid, mqType, Reserved, head, end);
        }


        public static byte[] GetSendMessageByte(ushort mqCode, byte[] data, int mqId, byte sendPid, byte mqType, ushort Reserved, char head, char end)
        {
            byte[] msg = new byte[14 + data.Length + 2];
            MQTools.CharToByte(head, msg);
            msg[2] = sendPid;
            msg[3] = mqType;
            MQTools.ConvertIntToByteArray(mqId, msg, 4);
            MQTools.ConvertUShortToByteArray(mqCode, msg, 8);
            MQTools.ConvertUShortToByteArray(Reserved, msg, 10);
            MQTools.ConvertUShortToByteArray((ushort)data.Length, msg, 12);
            Array.Copy(data, 0, msg, 14, data.Length);
            //校验
            //msg[data.Length + 14] = 0;

            MQTools.CharToByte(end, msg, data.Length + 14);

            return msg;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="mqCode">
        /// 登陆      Login = 1,
        /// 发送消息  SendMessage = 10,
        /// 确认消息  AckMessage = 11,
        /// 拉取消息  PullMessage = 12,
        /// 分包消息  Subpackage = 21,
        /// 广播消息  BroadcastMessage = 31,
        /// 中转消息  TransferMessage = 41,
        /// 心跳      Heartbeat = 100,
        /// 获取主题  GetForTopic = 200,
        /// 创建主题  CreateTopic = 201,
        /// 删除主题  DeleteTopic = 202,
        /// 获取队列  GetForQueue = 210,
        /// 新增队列  AddQueue = 211,
        /// 删除队列  DeleteQueue = 212,
        /// 获取房间  GetForRoom = 220,
        /// 创建房间  AddRoom = 221,
        /// 删除房间  DeleteRoom = 222,
        ///</param>
        /// <param name="message">消息</param>
        /// <param name="mqId">消息编码唯一 L = 4 字节</param>
        /// <param name="encoding"></param>
        /// <param name="sendPid">分片包编号 0=默认，0！= 片编号L=1字节</param>
        /// <param name="mqType">
        /// 消息类型 L = 1 字节
        /// default	0	普通消息
        /// Ack	1	需要确认消息
        /// ResAck	11	应答确认消息
        /// Sys	2	系统内部消息</param>
        /// <param name="Reserved"></param>
        /// <param name="head"></param>
        /// <param name="end"></param>
        /// <returns></returns>
        public static MQDataInfo GetMQDataInfoMessage(ushort mqCode, string message, int mqId, Encoding encoding = null, byte sendPid = 0, byte mqType = 0, ushort Reserved = 0, char head = '&', char end = '$')
        {
            if (encoding == null)
                encoding = Encoding.UTF8;
            MQDataInfo mQData = new MQDataInfo();

            mQData.Body = encoding.GetBytes(message);//大包分包方案
            mQData.Head = head;
            mQData.PID = sendPid;
            mQData.Type = mqType;
            mQData.MID = mqId;
            mQData.Code = mqCode;
            mQData.Reserved = Reserved;
            mQData.Length = (ushort)mQData.Body.Length;
            mQData.End = end;

            return mQData;
        }

        public static byte[] GetSendMessageByte(MQDataInfo mQDataInfo)
        {
            byte[] msg = new byte[14 + mQDataInfo.Length + 2];
            MQTools.CharToByte(mQDataInfo.Head, msg);
            msg[2] = mQDataInfo.PID;
            msg[3] = mQDataInfo.Type;
            MQTools.ConvertIntToByteArray(mQDataInfo.MID, msg, 4);
            MQTools.ConvertUShortToByteArray(mQDataInfo.Code, msg, 8);
            MQTools.ConvertUShortToByteArray(mQDataInfo.Reserved, msg, 10);
            MQTools.ConvertUShortToByteArray((ushort)mQDataInfo.Length, msg, 12);
            Array.Copy(mQDataInfo.Body, 0, msg, 14, mQDataInfo.Length);

            MQTools.CharToByte(mQDataInfo.End, msg, mQDataInfo.Length + 14);

            return msg;
        }
    }
}
