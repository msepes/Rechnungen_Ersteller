using DATA;
using DATA.Tools;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Rechnungen.Binder;

namespace Rechnungen.Dialogs
{
    /// <summary>
    /// Interaction logic for EmailServerDialog.xaml
    /// </summary>
    public partial class EmailServerDialog : Window
    {
        private EmailConf Conf;
        private Func<EmailConf> GetConfig;
        private Action Save;

        private Angebot angebot;
        private Rechnung rechnung;
        private Benutzer Benutzer;


        public EmailServerDialog()
        {
            InitializeComponent();
            TextBoxTools.MakeAcceptDigits(txtPort);
        }

        public void Register(Func<EmailConf> GetConfig,Benutzer user, Rechnung rechnung, Action Save)
        {
            this.rechnung = rechnung;
            this.Register(GetConfig, user, Save);
        }

        public void Register(Func<EmailConf> GetConfig, Benutzer user, Angebot angebot, Action Save)
        {
            this.angebot = angebot;
            this.Register(GetConfig, user, Save);
        }

        private void Register(Func<EmailConf> GetConfig, Benutzer user, Action Save)
        {
            this.Save = Save;
            this.GetConfig = GetConfig;
            this.Benutzer = user;
            Conf = GetConfig();
            Bind(Conf);

        }


        private void Bind(EmailConf Conf)
        {
            BindControl(nameof(Conf.Port), Conf, txtPort);
            BindControl(nameof(Conf.UserName), Conf, txtUserName);
            BindControl(nameof(Conf.password), Conf, txtPassword);
            BindControl(nameof(Conf.EmailInhalt), Conf, txtTemplate);
            BindControl(nameof(Conf.EmailServer), Conf, txtMailServer);
            BindControl(nameof(Conf.EmailBetriff), Conf, txtEmailHead);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (!this.TrySave())
                return;

                this.Close();
        }

        private bool TrySave()
        {
            try
            {
                Save();
                return true;
            }
            catch (Exception ex)
            {
                ExceptionTools.HandleException(ex, this.GetType());
                return false;
            }
        }


        private void btnPreview_Click(object sender, RoutedEventArgs e)
        {
            if (!this.TrySave())
                return;

            try
            {
                var Betriff = MailTools.GetBetriff(Conf, Benutzer, rechnung, angebot);
                var Inhalt = MailTools.GetPreview(Conf, Benutzer, rechnung, angebot);

                Action<string, string> sendMail = (Betriff, Inhalt) => MailTools.SendMail(Conf, Benutzer, Betriff, Inhalt, rechnung);

                if(rechnung == null)
                    sendMail = (Betriff, Inhalt) => MailTools.SendMail(Conf, Benutzer, Betriff, Inhalt, angebot);

                var frm = new EmailVersand(Betriff, Inhalt, rechnung==null? angebot.Kunde.Email : rechnung.Kunde.Email, sendMail);
                MainWindow.ShowWindow(frm);
            }
            catch (Exception ex)
            {
                ExceptionTools.HandleException(ex, this.GetType());
            }
        }
    }
}
