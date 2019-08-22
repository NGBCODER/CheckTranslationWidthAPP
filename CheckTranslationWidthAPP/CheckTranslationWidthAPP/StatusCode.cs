using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckTranslationWidthAPP
{
    public class StatusCode
    {
        public enum code
        {
            /// <summary>
            /// 文件不存在
            /// </summary>
            FileNoExist = -1,

            /// <summary>
            /// 文件类型不支持
            /// </summary>
            FileTypeNoSupport = 0,

            /// <summary>
            /// 保存路径不正确
            /// </summary>
            SavePathNoCurrent = 1,
            /// <summary>
            /// 译文列的位置不正确
            /// </summary>
            TargetColumnNoCurrent = 2,
            /// <summary>
            /// 译文中存在空值
            /// </summary>
            ColumnHasNullValue = 3,
        }
    }
}
