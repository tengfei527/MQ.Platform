using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQ.Common.Mode
{
    /// <summary>
    /// 登陆信息
    /// </summary>
    public class LoginModel
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }
        /// <summary>
        /// 密钥
        /// </summary>
        public string Password { get; set; }
        /// <summary>
        /// 客户端标识
        /// </summary>
        public string ClientId { get; set; }
        /// <summary>
        /// 附加信息
        /// </summary>
        public string Append { get; set; }
    }
}
