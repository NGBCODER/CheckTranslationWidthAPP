using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using System.Drawing;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using CheckTranslationWidthAPP.model;
using ClosedXML.Excel;
using DocumentFormat.OpenXml.Wordprocessing;
using FontFamily = System.Windows.Media.FontFamily;
using FontStyle = System.Windows.FontStyle;
using Brushes = System.Windows.Media.Brushes;

namespace CheckTranslationWidthAPP.Utils
{
    class ExcelUtils
    {
        /// <summary>
        /// 判断是否为译文
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool IsTranslationFile(string filePath, Action ac = null)
        {
            IXLWorkbook wbTranslation = new XLWorkbook(filePath);
            IXLWorksheet sheet = wbTranslation.Worksheet(1);

            string strCheckCode = sheet.Cell(1, 1).GetString();
            string strMachineType = sheet.Cell(2, 1).GetString();
            string strKeyStr = sheet.Cell(2, 2).GetString();
            string strChinese = sheet.Cell(2, 3).GetString();
            string strEnglish = sheet.Cell(2, 4).GetString();

            if (strCheckCode.Equals("校验码")
                && strMachineType.Equals("仪器类型")
                && strKeyStr.Equals("key_str") 
                && strChinese.Equals("中文") 
                && strEnglish.Equals("English"))
            {
                return true;
            }
            else
            {
                return false;

            }
        }

        /// <summary>
        /// 使用FormattedText 返回模拟译文以及宽度
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="beginRow"></param>
        /// <param name="sourceClumn1"></param>
        /// <param name="sourceClumn2"></param>
        /// <param name="translation"></param>
        /// <returns></returns>
        public static Dictionary<string, Translation> GetTranlstionFromExcel(IXLWorksheet sheet,int beginRow, int sourceClumn1, int sourceClumn2,int translation)
        {
            int rows = sheet.RangeUsed().RowCount();
            Dictionary<string, Translation> dicCheckTranlation = new Dictionary<string, Translation>();
            Random random = new Random(6);
            //遍历
            for (int i = beginRow; i <= rows; i++)
            {
                //获取模拟译文
                string strTranlation = sheet.Cell(i, random.Next(3, 5)).Value + GetRandString(10);
                //获取模拟译文长度
                double width = GetStringActualWidthByMethod(strTranlation,
                    new FontFamily("Microsoft YaHei UI"),
                    FontStyles.Normal,
                    FontWeights.Normal,
                    FontStretches.Normal,
                    12);
                //键
                string strKey = sheet.Cell(i, 2).Value.ToString();
                //保存
                dicCheckTranlation.Add(strKey,new Translation{TranslationValue = strTranlation,TranslationWidth = width});
            }
            return dicCheckTranlation;
        }

        /// <summary>
        /// 使用FormattedText获取指定列的译文的宽度*(3中文,4英文，开始行row:3)
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="beginRow"></param>
        /// <param name="sourceClumn"></param>
        /// <returns></returns>
        public static Dictionary<string, double> GetWidthdFromExcel(IXLWorksheet sheet, int beginRow, int sourceClumn)
        {
            int rows = sheet.RangeUsed().RowCount();
            Dictionary<string, double> dicTranslation = new Dictionary<string, double>();
            //遍历所有行
            for (int i = beginRow; i <= rows; i++)
            {
                //获取字符串
                string strTranlation = (string)sheet.Cell(i, sourceClumn).Value;
                string strKey = (string)sheet.Cell(i, 2).Value;
                //计算并加入集合
                dicTranslation.Add(strKey,
                                    GetStringActualWidthByMethod(strTranlation,
                                    new FontFamily("Microsoft YaHei UI"),
                                    FontStyles.Normal,
                                    FontWeights.Normal,
                                    FontStretches.Normal,
                                    12)
                                   );
            }
            return dicTranslation;
        }

        /// <summary>
        /// 返回指定列的译文
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="beginRow"></param>
        /// <param name="sourceClumn"></param>
        /// <returns></returns>
        public static Dictionary<string, string> GetTranslations(IXLWorksheet sheet, int beginRow, int sourceClumn)
        {
            int rows = sheet.RangeUsed().RowCount();
            Dictionary<string, string> dicTranslation = new Dictionary<string, string>();
            for (int i = beginRow; i <= rows; i++)
            {
                //获取字符串
                string strTranlation = (string)sheet.Cell(i, sourceClumn).Value;
                //获取译文的key
                string strKey = (string)sheet.Cell(i, 2).Value;
                //计算并加入集合
                dicTranslation.Add(strKey, strTranlation);
            }
            return dicTranslation;
        }

        /// <summary>
        /// 根据FormattedText 获取标准(中英)宽度
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="beginRow"></param>
        /// <param name="sourceClumn1"></param>
        /// <param name="sourceClumn2"></param>
        /// <returns></returns>
        public static Dictionary<string, double> GetStandardMaxWidth(IXLWorksheet sheet, int beginRow, int sourceClumn1,int sourceClumn2)
        {
            //中英文宽度
            var ChineseWidth = GetWidthdFromExcel(sheet, 3, 3);
            var EnglishWidth = GetWidthdFromExcel(sheet, 3, 4);
            //标准的宽度
            Dictionary<string, double> dicStandardWith = new Dictionary<string, double>();
            foreach (var english in EnglishWidth)
            {
                //比较大小
                if (english.Value >= ChineseWidth[english.Key])
                {
                    //取英文
                    dicStandardWith.Add(english.Key,english.Value);
                }
                else
                {
                    //取中文
                    dicStandardWith.Add(english.Key, ChineseWidth[english.Key]);
                }
            }
            return dicStandardWith;
        }

        /// <summary>
        /// 获取0-length个随机字符串
        /// </summary>
        /// <param name="length"></param>
        /// <returns></returns>
        public static string GetRandString(int length)
        {
            int i=0;
            Random random = new Random(GetRanddomSeed());
            StringBuilder sb = new StringBuilder();
            //随机添加0-9个字符
            int randomLength = random.Next(0, length);
            while (i < randomLength)
            {
                sb.Append((char)random.Next(65, 123));
                ++i;
            }
            return sb.ToString();
        }

        /// <summary>
        /// 获取随机数种子
        /// </summary>
        /// <returns></returns>
        public static int GetRanddomSeed()
        {
            byte[] bytes = new byte[4];
            RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider();
            rng.GetBytes(bytes);
            return BitConverter.ToInt32(bytes, 0);
        }

        public static string GetRandString(string strSource)
        {
            Random random = new Random(GetRanddomSeed());
            StringBuilder sb = new StringBuilder(strSource);
            if (random.Next(0,2)>0)
            {
                //添加
                sb.Append(GetRandString(5));
            }
            else
            {
                //截取
                sb.Remove(sb.Length-1, 1);
            }
            return sb.ToString();
        }

        /// <summary>
        /// 使用 FormattedText 返回译文宽度
        /// </summary>
        /// <param name="strTranlation"></param>
        /// <param name="fontFamily"></param>
        /// <param name="fontStyle"></param>
        /// <param name="fontWeight"></param>
        /// <param name="fontStretch"></param>
        /// <param name="fontSize"></param>
        /// <returns></returns>
        public static double GetStringActualWidthByMethod(String strTranlation, FontFamily fontFamily, FontStyle fontStyle,
            FontWeight fontWeight, FontStretch fontStretch, double fontSize)
        {
            var formattedText = new FormattedText(strTranlation,
                CultureInfo.CurrentUICulture,
                FlowDirection.LeftToRight,
                new Typeface(fontFamily, fontStyle, fontWeight, fontStretch),
                fontSize,
                Brushes.Black);
            return formattedText.Width;
        }


        /// <summary>
        /// 根据key,
        /// </summary>
        /// <param name="sheet"></param>
        /// <param name="strKey"></param>
        /// <returns></returns>
        public static string GetStringBaseOnkey(IXLWorksheet sheet,string strKey, int column)
        {
            int rows = sheet.RangeUsed().RowCount();
            for (int i =3; i <= rows; i++)
            {
                //找到列
                if (sheet.Cell(i,2).Value.ToString().Equals(strKey))
                {
                    return sheet.Cell(i, column).Value.ToString();
                }
            }
            return "";
        }

    }
}
