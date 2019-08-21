using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using CheckTranslationWidthAPP.CommonUnit;
using CheckTranslationWidthAPP.model;
using NBug;


namespace CheckTranslationWidthAPP
{
    /// <summary>
    /// App.xaml 的交互逻辑
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // “ .\” 处理
            if (e.Args.Length >= 1 && e.Args[0].StartsWith("."))
            {
                string strFileName = e.Args[0].Substring(2);
                e.Args[0] = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, strFileName);
            }
            //参数获取
            switch (e.Args.Length)
            {
                case 1:
                    Argument.FilePath = e.Args[0];
                    break;
                case 2:
                    Argument.FilePath = e.Args[0];
                    Argument.OutPutType = e.Args[1].ToLower();
                    break;
                case 3:
                    Argument.FilePath = e.Args[0];
                    Argument.OutPutType = e.Args[1].ToLower();
                    Argument.OutPutDiretory = e.Args[2];
                    break;
                case 4:
                    Argument.FilePath = e.Args[0];
                    Argument.OutPutType = e.Args[1].ToLower();
                    Argument.OutPutDiretory = e.Args[2];
                    try
                    {
                        var t1 = e.Args[3].ToString();
                        Argument.TargetColumn = Convert.ToInt32(e.Args[3].ToString());
                        if (Argument.TargetColumn<=0)
                        {
                            throw new Exception();
                        }
                    }
                    catch (Exception)
                    {
                        Console.WriteLine("Input column position is incorrect");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }
                    break;
            }
            //验证，文件输入有误
            if (e.Args.Length >= 1)
            {
                //文件位置输入有误
                if (File.Exists(Argument.FilePath) == false)
                {
                    Console.WriteLine("This is not a document or the document does not exist");
                    Console.ReadKey();
                    Environment.Exit(0);
                }
                //再判断输出类型，xml json
                if (e.Args.Length >= 2)
                {
                    if ((!Argument.OutPutType.Equals("json") && !Argument.OutPutType.Equals("xml")))
                    {
                        Console.WriteLine("The file type is not current!,only support xml or json");
                        Console.ReadKey();
                        Environment.Exit(0);
                    }

                    //再对文件夹参数判断
                    if (e.Args.Length >= 3 && Directory.Exists(Argument.OutPutDiretory) == false)
                    {
                        try
                        {                            
                            //创建文件夹
                            Console.WriteLine("Creating the Diretory...");
                            Directory.CreateDirectory(Argument.OutPutDiretory);
                            Console.WriteLine("Create Success");
                        }
                        catch (Exception)
                        {
                            Console.WriteLine("Creating the Diretory fail,please check your diretory");
                            Console.ReadKey();
                            Environment.Exit(0);
                        }
                    }
                }
            }



            //日志记录初始化
            LogHelper.Init();
            LogHelper.Logger.Info("Started");
            #region 添加异常处理捕获

            // Sample NBug configuration for WPF applications
            Settings.MaxQueuedReports = 100;
            Settings.StopReportingAfter = 5 * 365;
            Settings.HandleProcessCorruptedStateExceptions = true;
            Settings.UIMode = NBug.Enums.UIMode.Minimal;
            // 异常的时候 Exception_ 文件储存的位置
            Settings.StoragePath = @"Crash";

            // 获取已经存在的 Crash 文件列表，删除旧的，不然达到最大存储值之后就无法继续写入了
            SortHelper sortHelper = new SortHelper();
            var strTmpPath_Crash = Path.Combine(Environment.CurrentDirectory, @"Crash");
            if (Directory.Exists(strTmpPath_Crash) == false)
            {
                Directory.CreateDirectory(strTmpPath_Crash);
            }
            var list = sortHelper.GetFiles_SortOfCreateTime(strTmpPath_Crash);
            if (list.Length >= Settings.MaxQueuedReports - 5)
            {
                for (int i = 0; i < list.Length - 5; i++)
                {
                    try
                    {
                        File.Delete(list[i].FullName);
                    }
                    catch (Exception)
                    {
                        // ignored
                    }
                }
            }

            AppDomain.CurrentDomain.UnhandledException += Handler.UnhandledException;
            Current.DispatcherUnhandledException += Handler.DispatcherUnhandledException;
            TaskScheduler.UnobservedTaskException += Handler.UnobservedTaskException;

            // 对于UI线程的未处理异常
            //Current.DispatcherUnhandledException += Current_DispatcherUnhandledException;

            // 对于非UI线程抛出的未处理异常
            //AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            #endregion
            base.OnStartup(e);
        }
    }
}
