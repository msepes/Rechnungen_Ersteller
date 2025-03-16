using Angeboten;
using DATA;
using DATA.Tools;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Rechnungen.Dialogs;
using Rechnungen.Forms;
using Rechnungen.Tools;
using Rechnungen.Windows;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Windows;
using static Rechnungen.BenutzerTools;
using static Rechnungen.logger;


namespace Rechnungen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static BillsContext context;

        public MainWindow()
        {
            InitializeComponent();

            var conn = ConfigurationManager.ConnectionStrings["DB"]?.ConnectionString;
            if(string.IsNullOrWhiteSpace(conn))
            {
                MessageBox.Show($"Bitte geben Sie Ihre Verbindungsstring (ConnectionString) in der Config-Datei vollständig ein.", "Verbindung", MessageBoxButton.OK, MessageBoxImage.Information);
                Environment.Exit(1);
            }

            Init();

            bool Exsits = BenutzerTools.Exsits(context.Benutzer);
            while (!Exsits) {
                var msgResult = MessageBox.Show($"Bitte geben Sie Ihre Firmadaten vollständig ein, danach einfach auf Speichern drücken.", "Firmadaten", MessageBoxButton.OKCancel, MessageBoxImage.Information);
                if (msgResult == MessageBoxResult.Cancel)
                { 
                    Environment.Exit(1);
                    return;
                }

                ShowOwnCompanyWindow();
                Exsits = BenutzerTools.Exsits(context.Benutzer);
            }
        }

        private void Init()
        {
            try
            {
                InsertData();
            }
            catch (Exception ex)
            {
                ExceptionTools.HandleException(new Exception($"Keine Verbindung zu ConnectionStringSetting:{ConfigurationManager.ConnectionStrings["DB"]?.ConnectionString}", ex), this.GetType());
                Environment.Exit(1);
            }
        }

        private void InsertData()
        {
            context = new BillsContext(ConfigurationManager.ConnectionStrings["DB"].ConnectionString);

            context.Database.EnsureCreated();

            if (context.Rabbat.FirstOrDefault(r => r.Beschreibung == "Kein Rabatt") == null)
            {
                var EmptyRabatt = new Rabbat();
                EmptyRabatt.Beschreibung = "Kein Rabatt";
                EmptyRabatt.satz = 0;
                EmptyRabatt.Nr = 1;
                context.Rabbat.Add(EmptyRabatt);
            }

            if (context.Rabbat.FirstOrDefault(r => r.Beschreibung == "Neue Kunden Rabatt") == null)
            {
                var NeuRabatt = new Rabbat();
                NeuRabatt.Beschreibung = "Neue Kunden Rabatt";
                NeuRabatt.satz = 10;
                NeuRabatt.Nr = 2;
                context.Rabbat.Add(NeuRabatt);
            }

            context.SaveChanges();
        }

        private void OwnCompany_Click(object sender, RoutedEventArgs e)
        {
            ShowOwnCompanyWindow();
        }

        private void ShowOwnCompanyWindow() 
        {
            var frm = new OwnCompany();
            frm.Register(() => GetBenutzer(context.Benutzer, context.Adressen), () => context.SaveChanges());
            ShowWindow(frm);
        }

        private void Clients_Click(object sender, RoutedEventArgs e)
        {
            var frm = new Clients();
            frm.Register(() => ClientsTools.NewKunde(context.Kunden, context.Adressen),
                         (ID) => ClientsTools.GetKunde(context.Kunden, ID),
                         () => ClientsTools.GetKunden(context.Kunden),
                         () => context.SaveChanges(),
                         (k) => ClientsTools.DeleteKunde(context.Kunden, context.Adressen, context.Rechnungen, context.Angebote, k),
                      
                         BenutzerTools.GetBenutzer(context.Benutzer,context.Adressen));

            frm.RegisterRechnung((k) => RechnungTools.NewRechnung(context.Rechnungen, k),
                                 (ID) => RechnungTools.GetRechnung(context.Rechnungen, ID),
                                 (k) => RechnungTools.GetRechnungen(context.Rechnungen, k),
                                 (r) => RechnungTools.DeleteRechnung(context.Rechnungen, context.Rechnungsposition, r),
                                 () => ConfigTools.GetConfig(context.EmailConf, ConfTyp.Rechnung),
                                 () => context.Rabbat,
                                 (rechnung) => RechnungTools.PrintBill(rechnung, GetBenutzer(context.Benutzer, context.Adressen))
                                  );

            frm.RegisterAngebot((k) => OfferTools.NewAngebot(context.Angebote, k),
                                (ID) => OfferTools.GetAngebot(context.Angebote, ID),
                                (k) => OfferTools.GetAngeboten(context.Angebote, k),
                                (r) => OfferTools.DeleteAngebot(context.Angebote, context.Angebotsposition, r),
                                 () => ConfigTools.GetConfig(context.EmailConf, ConfTyp.Angebot),
                                () => context.Rabbat,
                                (rechnung) => OfferTools.PrintOffer(rechnung, GetBenutzer(context.Benutzer, context.Adressen)));

            frm.Init();
            ShowWindow(frm);
        }

        private void Bills_Click(object sender, RoutedEventArgs e)
        {
            var frm = new Bill();
            frm.Register((ID) => RechnungTools.GetRechnung(context.Rechnungen, ID),
                         () => RechnungTools.GetRechnungen(context.Rechnungen),
                         () => context.SaveChanges(),
                         () => context.Rabbat,
                         (rechnung) => RechnungTools.PrintBill(rechnung, GetBenutzer(context.Benutzer, context.Adressen)),
                         () => ConfigTools.GetConfig(context.EmailConf,ConfTyp.Rechnung),
                         BenutzerTools.GetBenutzer(context.Benutzer, context.Adressen));

            ShowWindow(frm);
        }

        private void Offers_Click(object sender, RoutedEventArgs e)
        {
            var frm = new Offer();
            frm.Register((ID) => OfferTools.GetAngebot(context.Angebote, ID),
                         () => OfferTools.GetAngeboten(context.Angebote),
                         () => context.SaveChanges(),
                         () => context.Rabbat,
                         (rechnung) => OfferTools.PrintOffer(rechnung, GetBenutzer(context.Benutzer, context.Adressen)),
                         () => ConfigTools.GetConfig(context.EmailConf, ConfTyp.Angebot),
                         BenutzerTools.GetBenutzer(context.Benutzer, context.Adressen));
            ShowWindow(frm);
        }

        private void Rabatt_Click(object sender, RoutedEventArgs e)
        {

            var frm = new Rabatt();
            frm.Register(() => RabattTools.NewRabatt(context.Rabbat),
                         (ID) => RabattTools.GetRabatt(context.Rabbat, ID),
                         () => RabattTools.GetRabatte(context.Rabbat),
                         (r) => RabattTools.DeleteRabatt(context.Rabbat, context.Rechnungen, context.Angebote, r),
                         () => context.SaveChanges());
            ShowWindow(frm);
        }

        private void Zusatztext_Click(object sender, RoutedEventArgs e)
        {
            Zusatztext zusatztext = new Zusatztext();
            zusatztext.Register(() => ZusatzTexteTools.NewRabatt(MainWindow.context.ZusatzTexte), 
                                ID => ZusatzTexteTools.GetZusatzText(MainWindow.context.ZusatzTexte, ID), 
                                () => ZusatzTexteTools.GetZusatzTexte(MainWindow.context.ZusatzTexte), 
                                r => ZusatzTexteTools.DeleteRabatt(MainWindow.context.ZusatzTexte, MainWindow.context.Rechnungen, r),
                                () => MainWindow.context.SaveChanges());

            MainWindow.ShowWindow((Window)zusatztext);
        }

        public static void ShowWindow(Window window) 
        {
            try
            {
                window.WindowStartupLocation = WindowStartupLocation.CenterScreen;
                window.ShowDialog();
                context.RollBack();
                RabattTools.AcceptChanges();
                RechnungTools.AcceptChanges();
                OfferTools.AcceptChanges();
                ClientsTools.AcceptChanges();
                ZusatzTexteTools.AcceptChanges();
            }
            catch (Exception ex)
            {
                ExceptionTools.HandleException(ex, window.GetType());
            }
        }

        private void Window_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            context.Dispose();
        }
    }
}
