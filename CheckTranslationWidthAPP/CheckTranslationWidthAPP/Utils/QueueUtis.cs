using System.Collections.Generic;
using CheckTranslationWidthAPP.model;
using ClosedXML.Excel;

namespace CheckTranslationWidthAPP.Utils
{
    class QueueUtis
    {
        
        /// <summary>
        /// 将数据传入到两个数据队列中
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="queue1"></param>
        /// <param name="queue2"></param>
        public static void JoinDataQueue(IXLWorksheet sheet, Queue<DataQueueInfo> queue1, Queue<DataQueueInfo> queue2,int targetColumn)
        {
            //读取sheet
            int rows = sheet.RangeUsed().RowCount();
            for (int i = 3; i <= rows; i++)
            {
                int no = i - 2;
                string strKey = sheet.Cell(i, 2).Value.ToString();
                string strChinese = sheet.Cell(i, 3).Value.ToString();
                string strEnglish = sheet.Cell(i, 4).Value.ToString();
                string strTargetTranslation = sheet.Cell(i, targetColumn).Value.ToString();
                DataQueueInfo dataQueueInfo = new DataQueueInfo{No = no,StrKey = strKey,Chinese = strChinese,English = strEnglish,TargtTranslation = strTargetTranslation};
                //加入数据队列
                if (i%2 != 0)
                {
                    queue1.Enqueue(dataQueueInfo);
                }
                else
                {
                    queue2.Enqueue(dataQueueInfo);
                }
            }
        }



    }
}
