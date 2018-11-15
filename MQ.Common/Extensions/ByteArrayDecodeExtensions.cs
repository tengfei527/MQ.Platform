using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQ.Common.Extensions
{
    public static class ByteArrayDecodeExtensions
    {
        public static string DecodeToString(this byte[] data, Encoding encoding = null)
        {
            if (data == null || data.Length == 0)
                return string.Empty;

            if (encoding == null)
                return Encoding.UTF8.GetString(data);
            else
                return encoding.GetString(data);
        }
    }
}
