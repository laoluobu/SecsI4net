﻿using System;
using System.Windows;
using Secs4Net.Sml;
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
            string S2F41UNLOCK = ":'S2F41' W  \n" +
                                        "<L[2]\n" +
                                        "  <A[4] \"UNLOCK\">\n" +
                                        "  <L[0]\n" +
                                        "  >\n" +
                                        ">\n" +
                                        ".";
            sss.SendAsync(S2F41UNLOCK.ToSecsMessage());
        }

        public void ShowMessage(byte[] c)
        {
            Console.WriteLine(c);
        }
    }
}
