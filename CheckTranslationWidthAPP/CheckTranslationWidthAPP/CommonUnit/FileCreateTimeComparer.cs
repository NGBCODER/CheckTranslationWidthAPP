using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckTranslationWidthAPP.CommonUnit
{
    /// <summary>
    /// 文件创建时间信息比较
    /// </summary>
    public class FileCreateTimeComparer : IComparer
    {
        int IComparer.Compare(object x, object y)
        {
            FileInfo fi1 = x as FileInfo;
            FileInfo fi2 = y as FileInfo;
            if (fi1 != null && fi2 != null)
            {
                return fi1.CreationTime.CompareTo(fi2.CreationTime);
            }

            throw new Exception("Compare Objs One Of Them Is Null");
        }
    }
}
