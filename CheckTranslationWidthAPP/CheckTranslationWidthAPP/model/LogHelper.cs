using System;
using System.IO;
using log4net;
using log4net.Config;

namespace CheckTranslationWidthAPP.model
{
    public class LogHelper
    {

        private static FileInfo logCfg = null;

        public static ILog Logger
        {
            get
            {
                if (logCfg == null)
                {
                    Init();
                }

                var result = LogManager.GetLogger("DataLabLogger");

                if (result == null)
                {
                    throw new Exception("Can't get DataLabLogger");
                }

                return result;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public static void Init()
        {
            logCfg = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + "log4net.config");
            XmlConfigurator.ConfigureAndWatch(logCfg);
        }

        /// <summary>
        /// 关闭日志
        /// </summary>
        public static void Close()
        {
            Logger.Info("messaGe");
            Logger.Logger.Repository.Shutdown();
        }
    }
}
