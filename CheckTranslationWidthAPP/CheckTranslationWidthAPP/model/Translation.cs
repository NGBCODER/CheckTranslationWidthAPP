using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckTranslationWidthAPP.model
{
    public class Translation
    {
        /// <summary>
        /// 译文字符串
        /// </summary>
        public string TranslationValue { set; get; }
        /// <summary>
        /// 译文宽度
        /// </summary>
        public double TranslationWidth { set; get; }

    }
}
