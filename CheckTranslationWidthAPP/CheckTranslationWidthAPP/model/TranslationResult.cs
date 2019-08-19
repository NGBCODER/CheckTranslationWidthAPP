using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckTranslationWidthAPP.model
{
    /// <summary>
    /// 用于datagrid的数据MODEL
    /// </summary>
    public class TranslationResult
    {
        public int No { set; get; }
        /// <summary>
        /// 控件方式
        /// </summary>
        public string Translation { set; get; }

        public double TranslationWidth { set; get; }
        public double StandardWidth { set; get; }

        public string IsOverlength { set; get; }
        /// <summary>
        /// FormattedText方法得到的结果
        /// </summary>
        public string TranslationWidthOfMethod { set; get; }

        public double StandardWidthOfMethod { set; get; }
        public string IsOverlengthOfMethod { set; get; }
    }
}
