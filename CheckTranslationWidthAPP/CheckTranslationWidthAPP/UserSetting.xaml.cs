using CheckTranslationWidthAPP.model;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;

namespace CheckTranslationWidthAPP
{
    /// <summary>
    /// UserSetting.xaml 的交互逻辑
    /// </summary>
    public partial class UserSetting : Window
    {
        public UserSetting()
        {
            InitializeComponent();
            //更新语言
            if (MyResourceDictionary.resource != null)
            {
                //如果已使用其他语言,先清空
                if (this.Resources.MergedDictionaries.Count > 0)
                {
                    this.Resources.MergedDictionaries.Clear();
                }
                this.Resources.MergedDictionaries.Add(MyResourceDictionary.resource);
            }
            //默认值
            tbFileSavePath.Text = AppDomain.CurrentDomain.BaseDirectory;
        }

        protected override void OnClosed(EventArgs e)
        {
            //保存内容
            Argument.OutPutDiretory = tbFileSavePath.Text;
            if ((bool)rbJSON.IsChecked)
            {
                Argument.OutPutType = "json";
            }
            if ((bool)rbXML.IsChecked)
            {
                Argument.OutPutType = "xml";
            }
            if (TargetColumn.Text.Equals(""))
            {
                //默认译文在第五行
                TargetColumn.Text = "5";
                Argument.TargetColumn = 5;
            }
            base.OnClosed(e);
        }
        /// <summary>
        /// 保存路径
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Find_SavePath(object sender, RoutedEventArgs e)
        {
            var commonDialog = new CommonOpenFileDialog();
            commonDialog.IsFolderPicker = true;
            CommonFileDialogResult  result= commonDialog.ShowDialog();
            if (result == CommonFileDialogResult.Ok && commonDialog.FileName != null)
            {
                tbFileSavePath.Text = commonDialog.FileName;
            }
        }


        /// <summary>
        /// 验证用户输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkUserInput(object sender, RoutedEventArgs e)
        {
            string strTarget = TargetColumn.Text;

            try
            {
                Argument.TargetColumn = Convert.ToInt32(strTarget);
            }
            catch (Exception)
            {
                MessageBox.Show("The number you entered is incorrect." + Environment.NewLine + "你输入的数字不正确");
                TargetColumn.Text = string.Empty;
                TargetColumn.BorderBrush = Brushes.Red;
            }
            try
            {
                if (Directory.Exists(tbFileSavePath.Text)==false)
                {
                    Directory.CreateDirectory(tbFileSavePath.Text);
                }
            }
            catch (Exception)
            {
                MessageBox.Show("The folder you entered does not exist." + Environment.NewLine + "你输入的文件夹不存在");
                tbFileSavePath.Text = string.Empty;
                tbFileSavePath.BorderBrush = Brushes.Red;

            }
        }
    }
}
