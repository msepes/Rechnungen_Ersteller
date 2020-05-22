using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Rechnungen.Dialogs
{
    /// <summary>
    /// Interaction logic for EmailVersand.xaml
    /// </summary>
    public partial class EmailVersand : Window
    {
        private Action<string, string> SendMail;
        public EmailVersand(string Head, string Body,string Receiver,Action<string,string> SendMail)
        {
            InitializeComponent();
            txtBetriff.Text = Head;
            txtPreview.Text = Body;
            txtReceiver.Text = Receiver;
            this.SendMail = SendMail;
        }

        private void btnSend_Click(object sender, RoutedEventArgs e)
        {
            SendMail(txtBetriff.Text, txtPreview.Text);
            MessageBox.Show("Email wurde erfolgreich geschickt!", "Formular Öffnen", MessageBoxButton.OK, MessageBoxImage.Information);
            this.Close();
        }
    }
}
