using System;
using System.ComponentModel;
using CheckTranslationWidthAPP.Annotations;

namespace CheckTranslationWidthAPP.view
{
    /// <summary>
    /// UI界面层数据
    /// </summary>
    class ViewData : INotifyPropertyChanged
    {
        /// <summary>
        /// 译文文件路径
        /// </summary>
        private string strFilePath; 

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected void OnPropertyChanged(PropertyChangedEventArgs e)
        {
            PropertyChanged?.Invoke(this, e);
        }

        public  string StrFilePath
        {
            get
            {
                return strFilePath;
            }
            set
            {
                if (strFilePath==value)
                {
                }
                else
                {
                    strFilePath = value;
                    OnPropertyChanged(new PropertyChangedEventArgs("StrFilePath"));
                }
            }
        }

    }
}
