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
            //是否更新设置值,否则使用默认值
            if (Argument.FilePath!=null)
            {
                tbFileSavePath.Text = Argument.OutPutDiretory;
            }
            else
            {
                tbFileSavePath.Text = AppDomain.CurrentDomain.BaseDirectory;
                Argument.OutPutDiretory = AppDomain.CurrentDomain.BaseDirectory;
            }
            //输出类型
            if (Argument.OutPutType != null)
            {
                if (Argument.OutPutType.ToLower().Equals("json"))
                {
                    rbJSON.IsChecked = true;
                }
                else if (Argument.OutPutType.ToLower().Equals("xml"))
                {
                    rbXML.IsChecked = true;
                }
            }
            else
            {
                rbJSON.IsChecked = true;
                Argument.OutPutType = "json";
            }

            //列
            if (Argument.TargetColumn > 0)
            {
                TargetColumn.Text = Argument.TargetColumn.ToString();
            }
            else
            {
                TargetColumn.Text = "5";
                Argument.TargetColumn = 5;
            }
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

            //验证文件夹位置
            try
            {
                //文件夹不存在
                if (Directory.Exists(tbFileSavePath.Text)==false)
                {
                    //尝试创建
                    Directory.CreateDirectory(tbFileSavePath.Text);
                }
                //文件夹已存在,保存设置值
                Argument.OutPutDiretory = tbFileSavePath.Text;

                //验证列位置
                try
                {
                    //保存列位置
                    Argument.TargetColumn = Convert.ToInt32(strTarget);
                    //成功提示
                    MessageBoxResult result= MessageBox.Show("Save Success" + Environment.NewLine + Environment.NewLine + "保存成功");
                }
                catch (Exception)
                {
                    MessageBox.Show("The number you entered is incorrect." + Environment.NewLine + Environment.NewLine + "你输入的数字不正确");
                    TargetColumn.Text = string.Empty;
                    TargetColumn.BorderBrush = Brushes.Red;
                }
                //保存输出类型
                if ((bool)rbJSON.IsChecked)
                {
                    Argument.OutPutType = "json";
                }
                else
                {
                    Argument.OutPutType = "xml";
                }
            }
            catch (Exception)
            {
                MessageBox.Show("The folder you entered does not exist." + Environment.NewLine + Environment.NewLine + "你输入的文件夹不存在");
                tbFileSavePath.Text = string.Empty;
                tbFileSavePath.BorderBrush = Brushes.Red;
            }
            
        }
    }
}
