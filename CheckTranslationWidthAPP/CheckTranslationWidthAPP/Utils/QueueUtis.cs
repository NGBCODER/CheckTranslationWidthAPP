using System;
using System.Collections.Generic;
using System.Windows;
using CheckTranslationWidthAPP.model;
using ClosedXML.Excel;

namespace CheckTranslationWidthAPP.Utils
{
    class QueueUtis
    {
        
        /// <summary>
        /// 将数据传入到数据队列数组中
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="queue1"></param>
        /// <param name="queue2"></param>
        public static void JoinDataQueue(IXLWorksheet sheet, Queue<DataQueueInfo>[] dataQueues,int targetColumn)
        {
            //读取sheet
            int rows = sheet.RangeUsed().RowCount();
            int dataCount = 0;
            for (int i = 3; i <= rows; i++)
            {
                int no = i - 2;
                string strKey = sheet.Cell(i, 2).Value.ToString();
                string strChinese = sheet.Cell(i, 3).Value.ToString();
                string strEnglish = sheet.Cell(i, 4).Value.ToString();
                //获取真正的译文，确保不为空
                string strTargetTranslation = sheet.Cell(i, targetColumn).Value.ToString();
                //检查是否为空,是抛出异常
                if (strTargetTranslation.Equals(""))
                {
                    //清空所有入队信息
                    for (int j = 0; j < dataQueues.Length; j++)
                    {
                        dataQueues[j].Clear();
                    }

                    //若是控制台传参，自动关闭程序
                    if (Argument.FilePath != null)
                    {
                        Console.WriteLine("The translation is incorrect and there is a null value" + Environment.NewLine + "译文不正确，存在空值");
                        Console.ReadKey();
                    }
                    //Ui方式
                    else
                    {
                        MessageBox.Show("The translation is incorrect and there is a null value" + Environment.NewLine + "译文不正确，存在空值");
                    }
                    throw new Exception();
                }
                //数据封装
                DataQueueInfo dataQueueInfo = new DataQueueInfo{No = no,
                    StrKey = strKey,
                    Chinese = strChinese,
                    English = strEnglish,
                    TargtTranslation = strTargetTranslation};

                // 按序入队
                dataQueues[dataCount].Enqueue(dataQueueInfo);
                dataCount++;
                if (dataCount == Environment.ProcessorCount)
                {
                    dataCount = 0;
                }
            }
        }
    }
}
