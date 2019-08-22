using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows;
using CheckTranslationWidthAPP.model;
using CheckTranslationWidthAPP.Utils;
using CheckTranslationWidthAPP.view;
using Microsoft.Win32;
using ClosedXML.Excel;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Windows.Media.Imaging;
using NPOI.HSSF.UserModel;
using NPOI.SS.Formula.Functions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using FontFamily = System.Windows.Media.FontFamily;
using HorizontalAlignment = System.Windows.HorizontalAlignment;
using VerticalAlignment = System.Windows.VerticalAlignment;

namespace CheckTranslationWidthAPP
{

    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        #region 全局变量
        private delegate void beginInvokeDelegate();
        private AutoResetEvent[] autoResetEvent;
        private static List<ResultQueueInfo> resultList = new List<ResultQueueInfo>();
        public static int threadCount = 0;
        private Dictionary<int,int> dicOverWidthLocation= new Dictionary<int,int>();
        //原始数据队列数组
        Queue<DataQueueInfo>[] dataQueues;

        //lock
        private Object obj = new object();
        //UI数据
        private ViewData viewData = new ViewData();
        //后台线程
        private BackgroundWorker backgroundWorker;
        #endregion

        public MainWindow()
        {
            InitializeComponent();
            backgroundWorker = (BackgroundWorker) FindResource("backgroundWorker");
            MDataContext.DataContext = viewData;

            //最高开8个线程
            threadCount = Environment.ProcessorCount > 8 ? 8 : Environment.ProcessorCount;
            autoResetEvent = new AutoResetEvent[threadCount];
            dataQueues = new Queue<DataQueueInfo>[threadCount];

            for (int i = 0; i < threadCount; i++)
            {
                autoResetEvent[i] = new AutoResetEvent(false);
                dataQueues[i] = new Queue<DataQueueInfo>();
            }
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
            //控制台方式启动处理
            if (Argument.FilePath!=null)
            {
                //this.ShowInTaskbar = false;
                this.Hide();
                //设置UI的文件位置
                viewData.StrFilePath = Argument.FilePath;
                //格式转换xls-->xlsx
                if (Argument.FilePath.EndsWith(".xls"))
                {
                    Argument.FilePath = ConvertWorkbook(Argument.FilePath);
                }
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
            string filePath = tbFilePath.Text;
            if (File.Exists(filePath))
            {
                //格式转换xls-->xlsx
                if (filePath.EndsWith(".xls"))
                {
                    filePath = ConvertWorkbook(filePath);
                }
                try
                {
                    //2 判断是否规范 
                    if (ExcelUtils.IsTranslationFile(filePath))
                    {
                        //还没设置译文列位置
                        if (Argument.TargetColumn > 0 == false)
                        {
                            User_SetUp(sender, e);
                        }
                        //3 将参数交由后台线程去做主要的事情
                        backgroundWorker.RunWorkerAsync(new InputContainer(filePath));
                        btnHandle.IsEnabled = false;
                        btnOpenFile.IsEnabled = false;
                    }
                }
                catch (Exception)
                {
                    MessageBox.Show("The file has been occupied by other programs. Please close the program that occupies the file." + 
                        Environment.NewLine + 
                        Environment.NewLine + 
                        "文件已被其它程序占用，请关闭占用该文件的程序。");
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
                #region 出队，处理数据
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

                //真正译文
                string strSimulation = info.TargtTranslation;

                //译文UI方式宽度
                double simulationWidth = GetActualWidth(strSimulation);
                //UI方式是否超长
                bool IsOverWidth = simulationWidth > stardandWidth ? true : false;
                //译文方法方式宽度
                double simulationWidthByMethod = GetActualWidthByMethod(strSimulation);
                //方法方式是否超长
                bool IsOverWidthByMethod = simulationWidthByMethod > stardandWidthByMethod ? true : false;
                #endregion

                //封装处理结果
                ResultQueueInfo resultQueueInfo = new ResultQueueInfo
                {
                    No = info.No,
                    Key = info.StrKey,
                    Chinese = info.Chinese,
                    English = info.English,
                    TranslationWidthOfControl = Math.Round(simulationWidth, 2),
                    StardandWidthOfControl = Math.Round(stardandWidth, 2),
                    IsOverWidthOfControl = IsOverWidth,
                    Simulation = strSimulation,
                    StardandWidthOfMethod = Math.Round(stardandWidthByMethod, 2),
                    TranslationWidthOfMethod = Math.Round(simulationWidthByMethod, 2),
                    IsOverWidthOfMethod = IsOverWidthByMethod,
                    Row = info.Row,
                    Column = info.Column
                };

                //获取超标准宽度数据
                if (IsOverWidthByMethod)
                {
                    dicOverWidthLocation.Add(info.Row,info.Column);
                }

                //添加到集合
                lock (obj)
                {
                    resultList.Add(resultQueueInfo);
                }

                //查询是否队列是否已经将数据处理完毕
                for (int i = 0; i < dataQueues.Length; i++)
                {
                    //发出该队列数据处理完毕信号
                    if (queueInfo.Equals(dataQueues[i]) && queueInfo.Count == 0)
                    {
                        autoResetEvent[i].Set();
                    }
                }
            }
        }

        /// <summary>
        /// 处理数据队列线程
        /// </summary>
        /// <param name="mDataQueue"></param>
        private void DataQueueThreadStart(object mDataQueue)
        {
            DealQueueData((Queue<DataQueueInfo>)mDataQueue);
        }

        /// <summary>
        /// 后台主要处理过程
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker_OnDoWork(object sender, DoWorkEventArgs e)
        {
            #region 初始化处理
            //清空容器中上一次的数据
            resultList.Clear();
            dicOverWidthLocation.Clear();

            //UI参数
            InputContainer container = (InputContainer) e.Argument;

            //工作簿
            IXLWorkbook wbTrans = new XLWorkbook(container.FilePath);
            //sheet
            IXLWorksheet wsTrans = wbTrans.Worksheet(1);
            
            #endregion

            #region 数据入队
            try
            {
                QueueUtis.JoinDataQueue(wsTrans, dataQueues, Argument.TargetColumn,threadCount);
            }
            catch (Exception)
            {
                //数据有误，结束本次检查,
                resultList.Clear();
                Dispatcher.Invoke(delegate ()
                {
                    MDataGrid.ItemsSource = null;
                });
                //UI方式 非正常关闭处理
                if (Argument.FilePath==null)
                {
                    return;
                }
                //控制台 非正常关闭处理
                else
                {
                    if (Argument.FilePath != null)
                    {
                        ConsoleClose(false);
                    }
                }
            }
            #endregion

            #region 处理数据
            //显示
            HiddenOrDisplay(btnTest,false);

            //启动线程
            for (int i = 0; i < threadCount; i++)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(DataQueueThreadStart));
                thread.Start(dataQueues[i]);
            }
            #endregion

            #region  等待数据处理完毕

            AutoResetEvent.WaitAll(autoResetEvent);

            #endregion

            #region 数据显示
            //隐藏
            HiddenOrDisplay(btnTest,true);
            //排序
            SortList(resultList);
            Dispatcher.Invoke(delegate ()
            {
                MDataGrid.ItemsSource = null;
                MDataGrid.ItemsSource = resultList;
            });
            #endregion

            #region 数据输出 xml、json  还有excel文件（标记超长宽度的位置）
            //1 输出路径获取
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

            //2 输出
            if (Argument.OutPutType!=null)
            {
                try
                {
                    OutPutOperator.GetJsonOrXmlResult(resultList, baseDiretory, Argument.OutPutType);
                }
                catch (Exception)
                {
                    Console.WriteLine("write error");
                    MessageBox.Show("write error"+Environment.NewLine+ Environment.NewLine + "文件输出出错");
                }
            }
            //若输出类型未指定 默认文件类型json
            else
            {
                OutPutOperator.OutPutToJSON(resultList,Path.Combine(baseDiretory, "result.json"));
            }

            //输出excel 
            PaintCellColorOfDic(dicOverWidthLocation,wsTrans,XLColor.PastelRed);
            
            wbTrans.SaveAs(baseDiretory+"输出结果"+"(Output Result).xlsx");

            #endregion

            #region 控制台正常关闭处理
            if (Argument.FilePath!=null)
            {
                ConsoleClose(true);
            }
            #endregion
        }

        /// <summary>
        /// xls转为xlsx
        /// </summary>
        /// <param name="filePath"></param>
        private string ConvertWorkbook(string filePath)
        {
            //文件信息
            FileInfo fileInfo = new FileInfo(filePath);
            //基本路径
            string baseDiretory = fileInfo.DirectoryName;
            //新文件名称
            string newFileName = fileInfo.Name.Replace(".xls", ".xlsx");
            //新文件全名
            string fullName = Path.Combine(baseDiretory, newFileName);
            //输入流
            FileStream fs = new FileStream(filePath, FileMode.Open,FileAccess.Read);
            //xls工作簿
            HSSFWorkbook wbXls = new HSSFWorkbook(fs);
            //工作表
            HSSFSheet xlsSheet = (HSSFSheet) wbXls.GetSheetAt(0);
            //最后一行
            IRow lastRow = xlsSheet.GetRow(xlsSheet.LastRowNum);
            //最后一列
            var lastColumn = lastRow.LastCellNum;

            //xls文件
            XSSFWorkbook wbXlsx = new XSSFWorkbook();
            ISheet xlsxSheet= wbXlsx.CreateSheet(xlsSheet.SheetName);

            for (int i = 0; i <= xlsSheet.LastRowNum; i++)
            {
                IRow row = xlsxSheet.CreateRow(i);
                for (int j = 0; j <= lastColumn ; j++)
                {
                    if (xlsSheet.GetRow(i).GetCell(j)!=null)
                    {
                        row.CreateCell(j).SetCellValue(xlsSheet.GetRow(i).GetCell(j).ToString());
                    }
                }
            }
            //写入文件
            FileStream sw = File.Create(fullName);
            wbXlsx.Write(sw);
            sw.Close();
            //获取内容，填入新表格
            return fullName;
        }

        /// <summary>
        /// 绘制指定行与列的颜色
        /// </summary>
        /// <param name="row">指定列</param>
        /// <param name="column">指定行</param>
        private void PaintCellColor(IXLWorksheet mSheet, int row, int column, XLColor color)
        {
            //找到指定位置的单元格
            IXLCell cell = mSheet.Row(row).Cell(column);
            //绘色
            cell.Style.Fill.BackgroundColor = color;
        }

        /// <summary>
        /// 绘制字典里的所有单元格的颜色，为了标记
        /// </summary>
        /// <param name="dic"></param>
        /// <param name="mSheet"></param>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <param name="color"></param>
        private void PaintCellColorOfDic(Dictionary<int,int> dic , IXLWorksheet mSheet, XLColor color)
        {
            foreach (var location in dic)
            {
                PaintCellColor(mSheet,location.Key,location.Value, color);
            }
        }
        /// <summary>
        /// 按钮的显示与隐藏
        /// </summary>
        /// <param name="bt"></param>
        /// <param name="isHidden"></param>
        private void HiddenOrDisplay(Button bt,bool isHidden)
        {
            if (isHidden)
            {
                Dispatcher.Invoke(delegate ()
                {
                    bt.Visibility = Visibility.Hidden;
                });
            }
            else
            {
                Dispatcher.Invoke(delegate ()
                {
                    bt.Visibility = Visibility.Visible;
                });
            }
        }

        /// <summary>
        /// 处理集合的排序问题，按N0升序排序
        /// </summary>
        /// <param name="lists"></param>
        private void SortList(List<ResultQueueInfo> lists)
        {
            Dictionary<int, ResultQueueInfo> TransferStation = new Dictionary<int, ResultQueueInfo>();
            int count = 1;
            while (count<=lists.Count)
            {
                //取数据
                foreach (var info in lists)
                {
                    if (info.No==count)
                    {
                        TransferStation.Add(count, info);
                    }
                }
                count++;
            }
            lists.Clear();
            foreach (var info in TransferStation)
            {
                lists.Add(info.Value);    
            }
        }

        /// <summary>
        /// 若是控制台传参，自动关闭程序
        /// </summary>
        private void ConsoleClose(Boolean isNormalClose)
        {
            if (isNormalClose)
            {
                Console.WriteLine("success");
                //Console.ReadKey();
            }
            else
            {
                Console.WriteLine("fail");
                //Console.ReadKey();
            }
            if (Argument.FilePath != null)
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
            //mProgressBar.Value = e.ProgressPercentage;
        }

        /// <summary>
        /// 进度完成时的反馈
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void BackgroundWorker_OnRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            
            //若是控制台传参，自动关闭程序
            if (Argument.FilePath == null)
            {
                MessageBox.Show("Check complete" + Environment.NewLine+ Environment.NewLine + "检查完毕");
            }
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
            fileDialog.Filter = "xlsx file|*.xlsx|xls file|*.xls|json file|*.json";
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
            } //中文
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
