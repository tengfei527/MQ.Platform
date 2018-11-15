using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MQ.Common.Mode
{
    [Serializable]
    public class TopicMode
    {
        public Dictionary<string, Dictionary<string, List<string>>> TopicDic { get; set; }
    }
}
