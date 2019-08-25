using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using MessageBox = System.Windows.Forms.MessageBox;

namespace CheckTranslationWidthAPP
{
    /// <summary>
    /// test.xaml 的交互逻辑
    /// </summary>
    public partial class test : Window
    {
        public test()
        {
            InitializeComponent();
            
        }

        private void open(object sender, RoutedEventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            InstalledFontCollection collection = new InstalledFontCollection();
            foreach (var item in collection.Families)
            {
                sb.Append(item.Name + "\t");
            }

            MessageBox.Show(sb.ToString());
            //FontDialog fontDialog = new FontDialog();
            //fontDialog.ShowEffects = false;
            //fontDialog.ShowDialog();
            //var t = fontDialog.Font;

        }
    }
}
