using MQ.Common;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQ.BrokerClient
{
    class Program
    {
        static log4net.ILog log = log4net.LogManager.GetLogger(typeof(Program));
        static void Main(string[] args)
        {
            string serverip = System.Configuration.ConfigurationManager.AppSettings["ServerIp"] ?? "127.0.0.1";
            Console.WriteLine("server ip is " + serverip.Trim());

            MQHelp mQHelp = new MQHelp(Guid.NewGuid().ToString(), serverip.Trim(), userName: "ftf", password: "527331", append: "cs");
            Console.WriteLine("请输入主题和标签,主题和标签空格分割:");
            var tags = Console.ReadLine().Trim().Split(' ');
            if (tags.Length > 1)
            {
                for (int i = 1; i < tags.Length; i++)
                {
                    mQHelp.Subscribe(tags[0], tags[i]);
                }
            }
            else
            {
                mQHelp.Subscribe(tags[0], "");
            }

            mQHelp.OnMessageArrive += MQHelp_OnMessageArrive;

            bool result = mQHelp.Start();

            log.Debug("客户端启动：" + result);

            while (true)
            {
                Console.WriteLine("请输入消息主题:");
                var topic = Console.ReadLine().Trim();
                Console.WriteLine("请输入标签:");
                var tag = Console.ReadLine().Trim();
                Console.WriteLine("请输入消息内容:");
                var message = Console.ReadLine().Trim();
                message = "{\"ID\":11,\"ParkingId\":\"e37a4d40 - 8dfc - 4e15 - a803 - 5760cbf6b646\",\"TokenNo\":\"粤BM675G\",\"TokenType\":0,\"ParkId\":\"41CF678E - F2EF - 41A6 - A839 - F823671D0E81\",\"TcmId\":\"77258425 - 1621 - 4177 - BBAA - 73E97FD00C3D\",\"TcmName\":\"时租卡A\",\"StaffNo\":\"\",\"StaffName\":\"\",\"RegPlate\":\"\",\"InAutoPlate\":\"粤BM675G\",\"InLaneName\":\"入口车道(一)\",\"InTime\":\"2018 - 09 - 19 13:52:50\",\"InPicture\":\"http://127.0.0.1:60001/20180919/13/20180919135250_in_v_1_粤BM675G.jpg\",\"InPicture2\":\"http://127.0.0.1:60001/20180919/13/20180919135250_in_v_2_粤BM675G.jpg\",\"InPictureStaff\":\"\",\"InOperatorId\":\"b89aafa9-f5c0-4280-b020-f895d234de76\",\"InOperatorName\":\"0001\",\"InType\":0,\"InFlag\":0,\"InLaneId\":\"6A65D465-EF34-45E5-BA43-2672AB9E55C3\",\"LotFullRemark\":\"\",\"GroupLotState\":0,\"ReservationNo\":\"\",\"InTerminalId\":\"A8391951-132F-48DD-B321-B84CDD274A1F\",\"InTerminalName\":\"地面岗亭一\",\"InRemark\":\"\",\"VehicleColor\":\"蓝色\",\"VehicleCategory\":\"\",\"VehicleBand\":\"\",\"PlateColor\":\"蓝色\",\"OutAutoPlate\":\"粤BM675G\",\"OutLaneName\":\"出口车道(一)\",\"OutTime\":\"2018-09-19 13:53:49\",\"OutPicture\":\"http://127.0.0.1:60001/20180919/13/20180919135349_out_v_1_粤BM675G.jpg\",\"OutPicture2\":\"http://127.0.0.1:60001/20180919/13/20180919135349_out_v_2_粤BM675G.jpg\",\"OutPictureStaff\":\"\",\"OutOperatorId\":\"b89aafa9-f5c0-4280-b020-f895d234de76\",\"OutOperatorName\":\"0001\",\"OutType\":0,\"OutFlag\":0,\"OutLaneId\":\"E511E618-C4E6-4FD3-998C-1FFF58EC2AB1\",\"OutRemark\":\"\",\"StayLasts\":\"0时0分59秒\",\"TerminalId\":\"A8391951-132F-48DD-B321-B84CDD274A1F\",\"TerminalName\":\"地面岗亭一\",\"TranAmount\":0.0000,\"AccountPayAmount\":0.0000,\"CashAmount\":0.0000,\"FreeAmount\":0.0000,\"DeductedAmount\":0.0000,\"DeductedHours\":0.0,\"DeductedHoursAmount\":0.0000,\"Province\":\"广东\",\"Gid\":\"FC5A3B65-7FF9-4DC1-AA12-39697F62AC7E\",\"Rid\":\"4bee8c80-10d1-4c69-93fe-67d461070220\"}{\"ID\":11,\"ParkingId\":\"e37a4d40 - 8dfc - 4e15 - a803 - 5760cbf6b646\",\"TokenNo\":\"粤BM675G\",\"TokenType\":0,\"ParkId\":\"41CF678E - F2EF - 41A6 - A839 - F823671D0E81\",\"TcmId\":\"77258425 - 1621 - 4177 - BBAA - 73E97FD00C3D\",\"TcmName\":\"时租卡A\",\"StaffNo\":\"\",\"StaffName\":\"\",\"RegPlate\":\"\",\"InAutoPlate\":\"粤BM675G\",\"InLaneName\":\"入口车道(一)\",\"InTime\":\"2018 - 09 - 19 13:52:50\",\"InPicture\":\"http://127.0.0.1:60001/20180919/13/20180919135250_in_v_1_粤BM675G.jpg\",\"InPicture2\":\"http://127.0.0.1:60001/20180919/13/20180919135250_in_v_2_粤BM675G.jpg\",\"InPictureStaff\":\"\",\"InOperatorId\":\"b89aafa9-f5c0-4280-b020-f895d234de76\",\"InOperatorName\":\"0001\",\"InType\":0,\"InFlag\":0,\"InLaneId\":\"6A65D465-EF34-45E5-BA43-2672AB9E55C3\",\"LotFullRemark\":\"\",\"GroupLotState\":0,\"ReservationNo\":\"\",\"InTerminalId\":\"A8391951-132F-48DD-B321-B84CDD274A1F\",\"InTerminalName\":\"地面岗亭一\",\"InRemark\":\"\",\"VehicleColor\":\"蓝色\",\"VehicleCategory\":\"\",\"VehicleBand\":\"\",\"PlateColor\":\"蓝色\",\"OutAutoPlate\":\"粤BM675G\",\"OutLaneName\":\"出口车道(一)\",\"OutTime\":\"2018-09-19 13:53:49\",\"OutPicture\":\"http://127.0.0.1:60001/20180919/13/20180919135349_out_v_1_粤BM675G.jpg\",\"OutPicture2\":\"http://127.0.0.1:60001/20180919/13/20180919135349_out_v_2_粤BM675G.jpg\",\"OutPictureStaff\":\"\",\"OutOperatorId\":\"b89aafa9-f5c0-4280-b020-f895d234de76\",\"OutOperatorName\":\"0001\",\"OutType\":0,\"OutFlag\":0,\"OutLaneId\":\"E511E618-C4E6-4FD3-998C-1FFF58EC2AB1\",\"OutRemark\":\"\",\"StayLasts\":\"0时0分59秒\",\"TerminalId\":\"A8391951-132F-48DD-B321-B84CDD274A1F\",\"TerminalName\":\"地面岗亭一\",\"TranAmount\":0.0000,\"AccountPayAmount\":0.0000,\"CashAmount\":0.0000,\"FreeAmount\":0.0000,\"DeductedAmount\":0.0000,\"DeductedHours\":0.0,\"DeductedHoursAmount\":0.0000,\"Province\":\"广东\",\"Gid\":\"FC5A3B65-7FF9-4DC1-AA12-39697F62AC7E\",\"Rid\":\"4bee8c80-10d1-4c69-93fe-67d461070220\"}{\"ID\":11,\"ParkingId\":\"e37a4d40 - 8dfc - 4e15 - a803 - 5760cbf6b646\",\"TokenNo\":\"粤BM675G\",\"TokenType\":0,\"ParkId\":\"41CF678E - F2EF - 41A6 - A839 - F823671D0E81\",\"TcmId\":\"77258425 - 1621 - 4177 - BBAA - 73E97FD00C3D\",\"TcmName\":\"时租卡A\",\"StaffNo\":\"\",\"StaffName\":\"\",\"RegPlate\":\"\",\"InAutoPlate\":\"粤BM675G\",\"InLaneName\":\"入口车道(一)\",\"InTime\":\"2018 - 09 - 19 13:52:50\",\"InPicture\":\"http://127.0.0.1:60001/20180919/13/20180919135250_in_v_1_粤BM675G.jpg\",\"InPicture2\":\"http://127.0.0.1:60001/20180919/13/20180919135250_in_v_2_粤BM675G.jpg\",\"InPictureStaff\":\"\",\"InOperatorId\":\"b89aafa9-f5c0-4280-b020-f895d234de76\",\"InOperatorName\":\"0001\",\"InType\":0,\"InFlag\":0,\"InLaneId\":\"6A65D465-EF34-45E5-BA43-2672AB9E55C3\",\"LotFullRemark\":\"\",\"GroupLotState\":0,\"ReservationNo\":\"\",\"InTerminalId\":\"A8391951-132F-48DD-B321-B84CDD274A1F\",\"InTerminalName\":\"地面岗亭一\",\"InRemark\":\"\",\"VehicleColor\":\"蓝色\",\"VehicleCategory\":\"\",\"VehicleBand\":\"\",\"PlateColor\":\"蓝色\",\"OutAutoPlate\":\"粤BM675G\",\"OutLaneName\":\"出口车道(一)\",\"OutTime\":\"2018-09-19 13:53:49\",\"OutPicture\":\"http://127.0.0.1:60001/20180919/13/20180919135349_out_v_1_粤BM675G.jpg\",\"OutPicture2\":\"http://127.0.0.1:60001/20180919/13/20180919135349_out_v_2_粤BM675G.jpg\",\"OutPictureStaff\":\"\",\"OutOperatorId\":\"b89aafa9-f5c0-4280-b020-f895d234de76\",\"OutOperatorName\":\"0001\",\"OutType\":0,\"OutFlag\":0,\"OutLaneId\":\"E511E618-C4E6-4FD3-998C-1FFF58EC2AB1\",\"OutRemark\":\"\",\"StayLasts\":\"0时0分59秒\",\"TerminalId\":\"A8391951-132F-48DD-B321-B84CDD274A1F\",\"TerminalName\":\"地面岗亭一\",\"TranAmount\":0.0000,\"AccountPayAmount\":0.0000,\"CashAmount\":0.0000,\"FreeAmount\":0.0000,\"DeductedAmount\":0.0000,\"DeductedHours\":0.0,\"DeductedHoursAmount\":0.0000,\"Province\":\"广东\",\"Gid\":\"FC5A3B65-7FF9-4DC1-AA12-39697F62AC7E\",\"Rid\":\"4bee8c80-10d1-4c69-93fe-67d461070220\"}{\"ID\":11,\"ParkingId\":\"e37a4d40 - 8dfc - 4e15 - a803 - 5760cbf6b646\",\"TokenNo\":\"粤BM675G\",\"TokenType\":0,\"ParkId\":\"41CF678E - F2EF - 41A6 - A839 - F823671D0E81\",\"TcmId\":\"77258425 - 1621 - 4177 - BBAA - 73E97FD00C3D\",\"TcmName\":\"时租卡A\",\"StaffNo\":\"\",\"StaffName\":\"\",\"RegPlate\":\"\",\"InAutoPlate\":\"粤BM675G\",\"InLaneName\":\"入口车道(一)\",\"InTime\":\"2018 - 09 - 19 13:52:50\",\"InPicture\":\"http://127.0.0.1:60001/20180919/13/20180919135250_in_v_1_粤BM675G.jpg\",\"InPicture2\":\"http://127.0.0.1:60001/20180919/13/20180919135250_in_v_2_粤BM675G.jpg\",\"InPictureStaff\":\"\",\"InOperatorId\":\"b89aafa9-f5c0-4280-b020-f895d234de76\",\"InOperatorName\":\"0001\",\"InType\":0,\"InFlag\":0,\"InLaneId\":\"6A65D465-EF34-45E5-BA43-2672AB9E55C3\",\"LotFullRemark\":\"\",\"GroupLotState\":0,\"ReservationNo\":\"\",\"InTerminalId\":\"A8391951-132F-48DD-B321-B84CDD274A1F\",\"InTerminalName\":\"地面岗亭一\",\"InRemark\":\"\",\"VehicleColor\":\"蓝色\",\"VehicleCategory\":\"\",\"VehicleBand\":\"\",\"PlateColor\":\"蓝色\",\"OutAutoPlate\":\"粤BM675G\",\"OutLaneName\":\"出口车道(一)\",\"OutTime\":\"2018-09-19 13:53:49\",\"OutPicture\":\"http://127.0.0.1:60001/20180919/13/20180919135349_out_v_1_粤BM675G.jpg\",\"OutPicture2\":\"http://127.0.0.1:60001/20180919/13/20180919135349_out_v_2_粤BM675G.jpg\",\"OutPictureStaff\":\"\",\"OutOperatorId\":\"b89aafa9-f5c0-4280-b020-f895d234de76\",\"OutOperatorName\":\"0001\",\"OutType\":0,\"OutFlag\":0,\"OutLaneId\":\"E511E618-C4E6-4FD3-998C-1FFF58EC2AB1\",\"OutRemark\":\"\",\"StayLasts\":\"0时0分59秒\",\"TerminalId\":\"A8391951-132F-48DD-B321-B84CDD274A1F\",\"TerminalName\":\"地面岗亭一\",\"TranAmount\":0.0000,\"AccountPayAmount\":0.0000,\"CashAmount\":0.0000,\"FreeAmount\":0.0000,\"DeductedAmount\":0.0000,\"DeductedHours\":0.0,\"DeductedHoursAmount\":0.0000,\"Province\":\"广东\",\"Gid\":\"FC5A3B65-7FF9-4DC1-AA12-39697F62AC7E\",\"Rid\":\"4bee8c80-10d1-4c69-93fe-67d461070220\"}";
                Console.WriteLine("请输入附加内容:");
                var atta = Console.ReadLine().Trim();
                Console.WriteLine("消息确认？请输入y或n");
                bool ack = Console.ReadLine().Trim().Equals("y");
                Console.WriteLine("循环发送次数:");
                int count = 1;
                try
                {
                    count = Convert.ToInt32(Console.ReadLine().Trim());
                }
                catch
                {

                }
                if (message.Trim().Equals("q"))
                {
                    mQHelp.Shutdown();
                    return;
                }

                log.Debug($"循环发送消息: 【{count}】次");
                for (int i = count; i > 0; i--)
                {
                    bool r = mQHelp.SendMessage(new Common.Mode.MQModel()
                    {
                        Attachment = atta,
                        CreatDate = DateTime.Now,
                        Message = message + i,
                        SenderId = mQHelp.ClientId,
                        Tag = tag,
                        Topic = topic,
                    }, ack ? MessageType.Ack : MessageType.Default);

                    log.Debug($"发送消息结果: {r} 客户端状态: {mQHelp.IsConnected}");

                    System.Threading.Thread.Sleep(10);
                }
            }
        }

        static List<double> list = new List<double>();
        private static void MQHelp_OnMessageArrive(object sender, Common.Mode.MQModel e)
        {

            var t = (DateTime.Now - e.CreatDate).TotalMilliseconds;
            list.Add(t);

            log.Info($"消息终端: {e.SenderId} 主题: {e.Topic} 标签: {e.Tag} 消息内容: {e.Attachment} 时间: {e.CreatDate.ToString("yyyy-MM-dd HH:mm:ss,fff")} 耗时:{(t / 1000)}(毫秒/ms) 平均耗时:{(list.Sum() / list.Count / 1000)}(毫秒/ms) 采样数据个数:{list.Count}");
        }
    }
}
