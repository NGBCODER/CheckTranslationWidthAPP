using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace CheckTranslationWidthAPP.model
{
    class InputContainer
    {
        public string FilePath { get; set; }

        public InputContainer(string filePath)
        {
            FilePath = filePath;
        }
    }
}
