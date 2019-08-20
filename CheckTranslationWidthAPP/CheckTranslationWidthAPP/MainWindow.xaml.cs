using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using CheckTranslationWidthAPP.model;
using CheckTranslationWidthAPP.Utils;
using CheckTranslationWidthAPP.view;
using Microsoft.Win32;
using ClosedXML.Excel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Windows.Media.Imaging;

namespace CheckTranslationWidthAPP
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        private delegate void beginInvokeDelegate();
        private  static AutoResetEvent[] autoResetEvent= new AutoResetEvent[]
        {
            new AutoResetEvent(false),
            new AutoResetEvent(false),
        };
        private static List<ResultQueueInfo> resultList = new List<ResultQueueInfo>();
        private static int rows = 0;

        //原始数据队列
        Queue<DataQueueInfo> dataQueue1 = new Queue<DataQueueInfo>();
        Queue<DataQueueInfo> dataQueue2 = new Queue<DataQueueInfo>();
        //结果队列
        Queue<ResultQueueInfo> resultQueue = new Queue<ResultQueueInfo>();
        //lock
        private Object obj = new object();
        //UI数据
        private ViewData viewData = new ViewData();      
        //后台线程
        private BackgroundWorker backgroundWorker;

        public MainWindow()
        {
            InitializeComponent();
            backgroundWorker = (BackgroundWorker) FindResource("backgroundWorker");
            MDataContext.DataContext = viewData;
        }

        /// <summary>
        /// 打开对话框
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnOpenFile_Click(object sender, RoutedEventArgs e)
        {
            MOpenFileDialog();
        }

        /// <summary>
        /// 初次加载页面
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            //设置属性
            viewData.StrFilePath = Argument.FilePath;
            if (Argument.FilePath!=null)
            {
                // 将参数交由后台线程去做主要的事情
                backgroundWorker.RunWorkerAsync(new InputContainer(Argument.FilePath));       
            }
        }

        /// <summary>
        /// 开始检查译文
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void click_BeginCheckFile(object sender, RoutedEventArgs e)
        {
            //1 首先要有译文
            if (File.Exists(tbFilePath.Text))
            {
                //2 判断是否规范 
                if (ExcelUtils.IsTranslationFile(tbFilePath.Text))
                {
                    //3 将参数交由后台线程去做主要的事情
                    backgroundWorker.RunWorkerAsync(new InputContainer(tbFilePath.Text));
                    btnHandle.IsEnabled = false;
                    btnOpenFile.IsEnabled = false;
                }
            }
            else
            {
                MessageBox.Show("请您选择待检测的译文文件");
            }
        }

        /// <summary>
        /// 线程处理过程
        /// </summary>
        /// <param name="queueInfo"></param>
        private void DealQueueData(Queue<DataQueueInfo> queueInfo)
        {
            while (queueInfo.Count > 0)
            {
                //出队，处理数据
                DataQueueInfo info = queueInfo.Dequeue();
                //UI方式中英宽度
                double chineseWidth = GetActualWidth(info.Chinese);
                double englishWidth = GetActualWidth(info.English);
                //UI方式标准宽度
                double stardandWidth = chineseWidth >= englishWidth ? chineseWidth : englishWidth;
                //方法方式中英宽度
                double chineseWidthByMethod = GetActualWidthByMethod(info.Chinese);
                double englishWidthByMethod = GetActualWidthByMethod(info.English);
                //方法方式标准宽度
                double stardandWidthByMethod = chineseWidthByMethod >= englishWidthByMethod ? chineseWidthByMethod : englishWidthByMethod;
                //基本字符串
                string strBase = chineseWidth >= englishWidth ? info.Chinese : info.English;

                //模拟译文
                string strSimulation = GetSimulation(strBase);

                //模拟译文UI方式宽度
                double simulationWidth = GetActualWidth(strSimulation);
                //UI方式是否超长
                bool IsOverWidth = simulationWidth > stardandWidth ? true : false;
                //模拟译文方法方式宽度
                double simulationWidthByMethod = GetActualWidthByMethod(strSimulation);
                //方法方式是否超长
                bool IsOverWidthByMethod = simulationWidthByMethod > stardandWidthByMethod ? true : false;

                //封装处理结果
                ResultQueueInfo resultQueueInfo = new ResultQueueInfo
                {
                    No = info.No,
                    Key = info.StrKey,
                    Chinese = info.Chinese,
                    English = info.English,
                    TranslationWidthOfControl = Math.Round(simulationWidth,2),
                    StardandWidthOfControl = Math.Round(stardandWidth, 2),
                    IsOverWidthOfControl = IsOverWidth,
                    Simulation = strSimulation,
                    StardandWidthOfMethod = Math.Round(stardandWidthByMethod, 2),
                    TranslationWidthOfMethod = Math.Round(simulationWidthByMethod, 2),
                    IsOverWidthOfMethod = IsOverWidthByMethod,
                };
                //UI显示、进度条更新
                lock (obj)
                {
                    resultList.Add(resultQueueInfo);
                    int progress = resultList.Count;
                    Dispatcher.Invoke(delegate ()
                    {
                        MDataGrid.ItemsSource = null;
                        MDataGrid.ItemsSource = resultList;
                        mProgressBar.Value = progress;
                    });
                    //效果
                    Thread.Sleep(100);
                }
                //队列1处理完毕
                if (queueInfo.Equals(dataQueue1)&& queueInfo.Count==0)
                {
                    autoResetEvent[0].Set();
                }
                //队列2处理完毕
                else if (queueInfo.Equals(dataQueue2) && queueInfo.Count == 0)
                {
                    autoResetEvent[1].Set();
                }
            }
        }

        /// <summary>
        /// 数据队列1
        /// </summary>
        private void DataQueue1ThreadStart()
        {
            DealQueueData(dataQueue1);
        }

        /// <summary>
        /// 数据队列2
        /// </summary>
        private void DataQueue2ThreadStart()
        {
            DealQueueData(dataQueue2);
        }

        /// <summary>
        /// 后台主要方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker_OnDoWork(object sender, DoWorkEventArgs e)
        {
            //清空容器中上一次的数据
            resultList.Clear();
            //UI参数
            InputContainer container = (InputContainer) e.Argument;
            //工作簿
            IXLWorkbook wbTrans = new XLWorkbook(container.FilePath);
            //sheet
            IXLWorksheet wsTrans = wbTrans.Worksheet(1);
            //数据条数（为了设置进度条的最大值）
            rows = wsTrans.RangeUsed().RowCount()-2;

            //设置精度条的最大值
            this.Dispatcher.Invoke((Action)delegate () {
                mProgressBar.Maximum = rows;
            });

            //数据入队
            QueueUtis.JoinDataQueue(wsTrans, dataQueue1, dataQueue2,5);
            //处理数据
            Thread dataQueue1Thread = new Thread(new ThreadStart(DataQueue1ThreadStart));
            dataQueue1Thread.Start();
            Thread dataQueue2Thread = new Thread(new ThreadStart(DataQueue2ThreadStart));
            dataQueue2Thread.Start();

            //等待数据处理完毕
            AutoResetEvent.WaitAll(autoResetEvent);
            //输出
            string baseDiretory = string.Empty;
            if (Argument.OutPutDiretory!=null && Directory.Exists(Argument.OutPutDiretory))
            {
                baseDiretory = Argument.OutPutDiretory;
            }
            //默认文件输出路径
            else
            {
                baseDiretory = AppDomain.CurrentDomain.BaseDirectory;
            }

            if (Argument.OutPutType!=null)
            {
                try
                {
                    OutPutOperator.GetJsonOrXmlResult(resultList, baseDiretory, Argument.OutPutType);
                }
                catch (Exception)
                {
                    Console.WriteLine("write error");
                    MessageBox.Show("write error"+Environment.NewLine+"文件输出出错");
                }
            }
            //默认文件类型 json
            else
            {
                OutPutOperator.OutPutToJSON(resultList,Path.Combine(baseDiretory, "result.json"));
            }

            //若是传参，自动关闭程序
            if (Argument.FilePath!=null)
            {
                Dispatcher.Invoke((Action)delegate ()
                {
                    Application.Current.Shutdown();
                });
                
            }
        }

        /// <summary>
        /// 通过内置方法返回宽度
        /// </summary>
        /// <param name="strTranslation"></param>
        /// <returns></returns>
        private double GetActualWidthByMethod(string strTranslation)
        {
            double width = ExcelUtils.GetStringActualWidthByMethod(strTranslation,
                new FontFamily("Microsoft YaHei UI"),
                FontStyles.Normal,
                FontWeights.Normal,
                FontStretches.Normal,
                12);

            return width;
        }

        /// <summary>
        /// 内存里创建一个button,获取宽度
        /// </summary>
        /// <param name="strTranslation"></param>
        /// <returns></returns>
        private double GetActualWidthByMemory(string strTranslation)
        {
            //创建button
            //设置内容
            //获取宽度

            double width = 0.0;
            Button button = new Button();
            button.Content = strTranslation;
            button.VerticalAlignment = VerticalAlignment.Center;
            button.HorizontalAlignment = HorizontalAlignment.Center;
            button.Visibility = Visibility.Hidden;
            testStackPanel.Children.Add(button);
            Dispatcher.Invoke(new Action(() =>
            {
                width = button.ActualWidth;
            }), DispatcherPriority.Loaded);
            return width;
        }

        /// <summary>
        /// UI方式获取width
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        private double GetActualWidth(string strStardand)
        {
            //锁住获取UI方式的width
            lock (obj)
            {
                Dispatcher.Invoke((Action)delegate()
                {
                    btnTest.Content = strStardand;
                });
                Thread.Sleep(5);
                return btnTest.ActualWidth;
            }

        }

        /// <summary>
        /// 根据标准（中英），产生模拟译文
        /// </summary>
        /// <param name="dicStardand"></param>
        /// <returns></returns>
        private string GetSimulation(string simulation)
        {
  
             string strRandom = ExcelUtils.GetRandString(simulation);
             return strRandom;
        }
        /// <summary>
        /// 进度改变的方法
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker_OnProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            mProgressBar.Value = e.ProgressPercentage;
        }

        /// <summary>
        /// 进度完成时的反馈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker_OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            //MessageBox.Show("检查完毕");
            btnHandle.IsEnabled = true;
            btnOpenFile.IsEnabled = true;
        }

        /// <summary>
        /// 打开文件对话框
        /// </summary>
        public void MOpenFileDialog()
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            fileDialog.DefaultExt = "xlsx";
            fileDialog.Filter = "xlsx file|*.xlsx|json file|*.json";
            if ((bool)fileDialog.ShowDialog())
            {
                string strFilepath = fileDialog.FileName;
                //设置属性
                viewData.StrFilePath = strFilepath;
            }
        }

        /// <summary>
        /// 语言选择
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            BitmapImage bitmapImage = null;
            //英文
            if (cmbSelectLanguage.SelectedIndex == 0)
            {
                try
                {
                    //根据名字载入语言文件//相对路径
                    MyResourceDictionary.resource = Application.LoadComponent(new Uri(@"language\" + "en_US" + ".xaml", UriKind.Relative)) as ResourceDictionary;
                    bitmapImage = new BitmapImage(new Uri(@"image/us.jpg", UriKind.RelativeOrAbsolute));
                }
                catch (Exception e2)
                {
                    MessageBox.Show(e2.Message);
                }
            }
           //中文
            else if(cmbSelectLanguage.SelectedIndex == 1)
            {
                try
                {
                    //根据名字载入语言文件//相对路径
                    MyResourceDictionary.resource = Application.LoadComponent(new Uri(@"language\" + "zh_CN" + ".xaml", UriKind.Relative)) as ResourceDictionary;
                    bitmapImage = new BitmapImage(new Uri(@"image/china.jpg", UriKind.RelativeOrAbsolute));
                }
                catch (Exception e2)
                {
                    MessageBox.Show(e2.Message);
                }
            }
            //更新
            if (MyResourceDictionary.resource != null)
            {
                //如果已使用其他语言,先清空
                if (this.Resources.MergedDictionaries.Count > 0)
                {
                    this.Resources.MergedDictionaries.Clear();
                }
                this.Resources.MergedDictionaries.Add(MyResourceDictionary.resource);
            }
            if (imCountry!=null)
            {
                imCountry.Source = bitmapImage;
            }
        }

        /// <summary>
        /// 用户设置、检查的列、输出路径、输出类型
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void User_SetUp(object sender, RoutedEventArgs e)
        {
            UserSetting userSetting = new UserSetting();
            userSetting.Owner = this;
            userSetting.WindowStartupLocation = System.Windows.WindowStartupLocation.CenterOwner;
            userSetting.ShowDialog();
        }
    }
}
