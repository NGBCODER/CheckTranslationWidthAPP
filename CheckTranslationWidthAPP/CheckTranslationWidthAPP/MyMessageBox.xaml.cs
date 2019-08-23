using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace CheckTranslationWidthAPP
{
    /// <summary>
    /// MyMessageBox.xaml 的交互逻辑
    /// </summary>
    public partial class MyMessageBox : Window
    {
        public MyMessageBox()
        {
            InitializeComponent();
        }
        public MyMessageBox(bool isScuuess, string bigMessage, string smallMessage)
        {
            InitializeComponent();

            if (isScuuess == false)
            {
                imFail.Source = new BitmapImage(new Uri(@"image/error.png", UriKind.RelativeOrAbsolute));
                if (bigMessage.Length > 15)
                {
                    lbBigText.FontSize = 10;
                    lbSmallText.FontSize = 10;
                }
                lbBigText.Content = bigMessage;
                lbSmallText.Content = smallMessage;
            }
            else
            {
                lbBigText.Content = bigMessage;
                lbSmallText.Content = smallMessage;
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void btCheck_MouseEnter(object sender, MouseEventArgs e)
        {
            btCheck.Foreground = Brushes.Black;
            btCheck.Background = Brushes.LightGreen;
            bdCheck.Background = Brushes.LightGreen;
        }

        private void btCheck_MouseLeave(object sender, MouseEventArgs e)
        {
            btCheck.Foreground = Brushes.White;
            btCheck.Background = Brushes.DodgerBlue;
            bdCheck.Background = Brushes.DodgerBlue;
        }
    }
}
