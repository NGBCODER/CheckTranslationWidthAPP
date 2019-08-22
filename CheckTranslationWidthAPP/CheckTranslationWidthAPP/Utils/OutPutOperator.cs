using CheckTranslationWidthAPP.model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Windows.Threading;
using System.Xml;
using System.Xml.Linq;

namespace CheckTranslationWidthAPP.Utils
{
    public class OutPutOperator
    {
        /// <summary>
        /// 将结果生成json数据
        /// </summary>
        /// <param name="lists"></param>
        /// <returns></returns>
        public static void OutPutToJSON(List<ResultQueueInfo> lists,string path)
        {
            JsonSerializer jsonSerializer = new JsonSerializer();
            StringWriter sw = new StringWriter();
            jsonSerializer.Serialize(new JsonTextWriter(sw),lists);
            File.WriteAllText(path, sw.GetStringBuilder().ToString());
        }



        /// <summary>
        /// 将结果输出为XML数据
        /// </summary>
        /// <param name="lists"></param>
        /// <param name="path"></param>
        public static void OutPutToXML(List<ResultQueueInfo> lists, string path)
        {
            if (File.Exists(path)==false)
            {
                File.Create(path);
            }
            XElement xmlFile = new XElement("results");
            foreach (ResultQueueInfo info in lists)
            {
                var node = new XElement("result",
                        new XElement("No", info.No.ToString()),
                        new XElement("Translation", info.Simulation),
                        new XElement("WidthOfControl", info.TranslationWidthOfControl),
                        new XElement("StardandWidthOfControl", info.StardandWidthOfControl),
                        new XElement("IsOverWidthOfControl", info.IsOverWidthOfControl),
                        new XElement("WidthOfMethod", info.TranslationWidthOfMethod),
                        new XElement("StardandWidthOfMethod", info.StardandWidthOfMethod),
                        new XElement("IsOverWidthOfMethod", info.IsOverWidthOfMethod),
                        new XElement("row", info.Row),
                        new XElement("column", info.Column)
                        );

                xmlFile.Add(node);
            }
            //write
            XmlWriterSettings settings = new XmlWriterSettings();
            settings.Encoding = new UTF8Encoding(true);
            settings.Indent = true;

            XmlWriter xmlWriter = null;
            try
            {
                xmlWriter = XmlWriter.Create(path, settings);
                xmlFile.Save(xmlWriter);
            }
            catch (Exception e)
            {
                LogHelper.Logger.Info("文件写入异常");
                throw e;
            }
            xmlWriter.Flush();
            xmlWriter.Close();
        }

        /// <summary>
        /// 根据文件类型，保存数据
        /// </summary>
        /// <param name="lists"></param>
        /// <param name="baseDiretory">文件夹位置</param>
        /// <param name="fileType"></param>
        public static void GetJsonOrXmlResult(List<ResultQueueInfo> lists, string baseDiretory,string fileType)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("result.").Append(fileType);
            if (fileType.Equals("xml"))
            {
                OutPutToXML(lists,Path.Combine(baseDiretory, sb.ToString()));
            }
            else
            {
                //json
                OutPutToJSON(lists, Path.Combine(baseDiretory, sb.ToString()));
            }
        }
    }
}
