using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckTranslationWidthAPP.CommonUnit
{
    /// <summary>
    /// 排序的辅助类
    /// </summary>
    public class SortHelper
    {
        /// <summary>
        /// 对于 FileInfo 进行排序
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public FileInfo[] GetFiles_SortOfCreateTime(string path)
        {
            DirectoryInfo di = new DirectoryInfo(path);
            FileInfo[] files = di.GetFiles();
            FileCreateTimeComparer fc = new FileCreateTimeComparer();
            Array.Sort(files, fc);
            return files;
        }
    }
}
