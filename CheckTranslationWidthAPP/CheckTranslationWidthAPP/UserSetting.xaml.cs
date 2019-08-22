using CheckTranslationWidthAPP.model;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.IO;
using System.Windows;
using System.Windows.Media;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using CheckTranslationWidthAPP.Utils;


namespace CheckTranslationWidthAPP
{
    /// <summary>
    /// UserSetting.xaml 的交互逻辑
    /// </summary>
    public partial class UserSetting : Window
    {
        Configura configura = new Configura();
        string jsonSetFile = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "configura.json");

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
            //读取json文件
            if (File.Exists(jsonSetFile)==false)
            {
                //创建初始json文件
                
                configura.OutPutDiretory = AppDomain.CurrentDomain.BaseDirectory;
                configura.OutPutType = "json";
                configura.TargetColumn = 5;
                SetConfiguraToJSON(configura, jsonSetFile);
            }
            else
            {
                //读取json参数
                StreamReader file = File.OpenText(jsonSetFile);
                JsonTextReader reader = new JsonTextReader(file);
                JObject jsonObject = (JObject)JToken.ReadFrom(reader);
                //配置参数
                Argument.OutPutDiretory = (string) jsonObject["OutPutDiretory"];
                Argument.OutPutType = (string)jsonObject["OutPutType"];
                Argument.TargetColumn = (int)jsonObject["TargetColumn"];
                file.Close();
            }

            //保存路径
            tbFileSavePath.Text = Argument.OutPutDiretory;

            //文件类型
            if (Argument.OutPutType.ToLower().Equals("json"))
            {
                rbJSON.IsChecked = true;
            }
            else if (Argument.OutPutType.ToLower().Equals("xml"))
            {
                rbXML.IsChecked = true;
            }

            //第几列
            TargetColumn.Text = Argument.TargetColumn.ToString();

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
        /// 将配置结果生成json数据
        /// </summary>
        /// <returns></returns>
        public static void SetConfiguraToJSON(Configura configura, string path)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            StringWriter sw = new StringWriter();
            jsonSerializer.Serialize(new JsonTextWriter(sw), configura);
            File.WriteAllText(path, sw.GetStringBuilder().ToString());
        }

        /// <summary>
        /// 验证用户输入
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void checkUserInput(object sender, RoutedEventArgs e)
        {

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
                    string strTarget = TargetColumn.Text;
                    Argument.TargetColumn = Convert.ToInt32(strTarget);
                    //成功提示
                    MessageBoxResult result= MessageBox.Show("Save Success" + Environment.NewLine + Environment.NewLine + "保存成功");
                }
                catch (Exception)
                {
                    MessageBox.Show("The number you entered is incorrect." + Environment.NewLine + Environment.NewLine + "你输入的数字不正确");
                    TargetColumn.Text = String.Empty;
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
                tbFileSavePath.Text = String.Empty;
                tbFileSavePath.BorderBrush = Brushes.Red;
            }

            //保存用户设置
            configura.OutPutDiretory = Argument.OutPutDiretory;
            configura.OutPutType = Argument.OutPutType;
            configura.TargetColumn = Argument.TargetColumn;
            SetConfiguraToJSON(configura, jsonSetFile);
        }
    }
}
