using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CheckTranslationWidthAPP.Utils
{
    public class OpenWindowsUtils
    {
        public static void OpenMyMessageBox(bool isScuuess, string bigMessage, string smallMessage, System.Windows.Window owner)
        {
            MyMessageBox MyMessageBox = new MyMessageBox(isScuuess, bigMessage, smallMessage);
            MyMessageBox.Owner = owner;
            MyMessageBox.ShowDialog();
        }
        public static void OpenMyMessageBox(System.Windows.Window owner)
        {
            MyMessageBox MyMessageBox = new MyMessageBox();
            MyMessageBox.Owner = owner;
            MyMessageBox.ShowDialog();
        }
        public static void OpenMyMessageBox(bool isScuuess, string bigMessage, string smallMessage)
        {
            MyMessageBox MyMessageBox = new MyMessageBox(isScuuess, bigMessage, smallMessage);
            MyMessageBox.ShowDialog();
        }
    }
}
