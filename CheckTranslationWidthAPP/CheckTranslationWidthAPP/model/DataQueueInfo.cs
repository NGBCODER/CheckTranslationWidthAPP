using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckTranslationWidthAPP.model
{
    /// <summary>
    /// 数据队列
    /// </summary>
    class DataQueueInfo
    {
        /// <summary>
        /// 序号
        /// </summary>
        public int No { set; get; }

        /// <summary>
        /// str_key
        /// </summary>
        public string StrKey { set; get; }
        /// <summary>
        /// 中文
        /// </summary>
        public string Chinese { set; get; }
        /// <summary>
        /// 英文
        /// </summary>
        public string English { set; get; }

        public string TargtTranslation { set; get; }

        public int Row { set; get; }
        public int Column { set; get; }
    }
}
