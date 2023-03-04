using System;
using System.Windows;
using SecsI4net;

namespace SecsIExampleWPF
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        ISecsIConnection sss;
        public MainWindow()
        {
            InitializeComponent();
            sss = new SeceIConnection("COM2", ShowMessage);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            sss.SendAsync(new SecsMessage(1,1));
        }

        public void ShowMessage(byte[] c)
        {
            Console.WriteLine(c);
        }

        private void Button_Click1(object sender, RoutedEventArgs e)
        {
            sss.SendAsync(new byte[] { 10, 0, 0, 129, 1, 128, 1, 21, 39, 19, 126, 1, 208 });
        }
    }
}
