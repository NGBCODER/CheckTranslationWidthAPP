using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckTranslationWidthAPP.model
{
    public class ResultQueueInfo
    {
        public int No { set; get; }
        public string Key { set; get; }
        public string Chinese { set; get; }
        public string English { set; get; }
        public double TranslationWidthOfControl { set; get; }
        public double StardandWidthOfControl { set; get; }
        public bool IsOverWidthOfControl { set; get; }
        public string Simulation { set; get; }
        public double TranslationWidthOfMethod { set; get; }
        public double StardandWidthOfMethod { set; get; }
        public bool IsOverWidthOfMethod { set; get; }

        public int Row { set; get; }
        public int Column { set; get; }
    }
}
