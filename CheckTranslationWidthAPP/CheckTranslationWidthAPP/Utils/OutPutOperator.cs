using CheckTranslationWidthAPP.model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckTranslationWidthAPP.Utils
{
    public class OutPutOperator
    {
        /// <summary>
        /// 将结果生成json数据
        /// </summary>
        /// <param name="lists"></param>
        /// <returns></returns>
        public static void OutPutToJSON(List<ResultQueueInfo> lists,string path)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            StringWriter sw = new StringWriter();
            jsonSerializer.Serialize(new JsonTextWriter(sw),lists);
            File.WriteAllText(path, sw.GetStringBuilder().ToString());
        }
    }
}
