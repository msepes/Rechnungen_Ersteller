using Angeboten;
using DATA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Rechnungen.Forms;
using Rechnungen.Tools;
using Rechnungen.Windows;
using System;
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
            Init();
        }

        private void Init()
        {
            try
            {
                InsertData();
                LoadData();
            }
            catch (Exception ex)
            {
                Exception(ex, this.GetType());
                var nl = Environment.NewLine;
                var msg = $"Fehler beim Laden:{nl + nl}{ex.Message}{nl}{ex.InnerException?.Message}";
                MessageBox.Show(this, msg, "Speichern nicht möglich", MessageBoxButton.OK, MessageBoxImage.Error);
                Environment.Exit(1);
            }
        }

        private void InsertData()
        {
            context = new BillsContext(ConfigurationManager.ConnectionStrings["DB"].ConnectionString);
        
            //context.Database.EnsureDeleted();
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

        private void LoadData()
        {
            context.Rabbat.Load();
            context.Kunden.Load();
            context.Benutzer.Load();
            context.Rechnungen.Load();
            context.Angebote.Load();
            context.Rechnungsposition.Load();
            context.Angebotsposition.Load();
        }

        private void OwnCompany_Click(object sender, RoutedEventArgs e)
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
                         (k) => ClientsTools.DeleteKunde(context.Kunden, k));

            frm.RegisterRechnung((k) => RechnungTools.NewRechnung(context.Rechnungen, k),
                                 (ID) => RechnungTools.GetRechnung(context.Rechnungen, ID),
                                 (k) => RechnungTools.GetRechnungen(context.Rechnungen, k),
                                 (r) => RechnungTools.DeleteRechnung(context.Rechnungen, r),
                                 () => context.Rabbat,
                                 (rechnung,path) => RechnungTools.PrintBill(rechnung, GetBenutzer(context.Benutzer, context.Adressen), path));

            frm.RegisterAngebot((k) => OfferTools.NewAngebot(context.Angebote, k),
                                (ID) => OfferTools.GetAngebot(context.Angebote, ID),
                                (k) => OfferTools.GetAngeboten(context.Angebote, k),
                                (r) => OfferTools.DeleteAngebot(context.Angebote, r),
                                () => context.Rabbat,
                                (rechnung, path) => OfferTools.PrintOffer(rechnung, GetBenutzer(context.Benutzer, context.Adressen), path));

            ShowWindow(frm);
        }

        private void Bills_Click(object sender, RoutedEventArgs e)
        {
            var frm = new Bill();
            frm.Register((ID) => RechnungTools.GetRechnung(context.Rechnungen, ID),
                                 () => RechnungTools.GetRechnungen(context.Rechnungen),
                                 () => context.SaveChanges(),
                                 () => context.Rabbat,
                                 (rechnung, path) => RechnungTools.PrintBill(rechnung, GetBenutzer(context.Benutzer, context.Adressen), path));
            ShowWindow(frm);
        }

        private void Offers_Click(object sender, RoutedEventArgs e)
        {
            var frm = new Offer();
            frm.Register((ID) => OfferTools.GetAngebot(context.Angebote, ID),
                                () => OfferTools.GetAngeboten(context.Angebote),
                                () => context.SaveChanges(),
                                () => context.Rabbat,
                                (rechnung, path) => OfferTools.PrintOffer(rechnung, GetBenutzer(context.Benutzer, context.Adressen), path));
            ShowWindow(frm);
        }

        private void Rabatt_Click(object sender, RoutedEventArgs e)
        {

            var frm = new Rabatt();
            frm.Register(() => RabattTools.NewRabatt(context.Rabbat),
                         (ID) => RabattTools.GetRabatt(context.Rabbat, ID),
                         () => RabattTools.GetRabatte(context.Rabbat),
                         (r) => RabattTools.DeleteRabatt(context.Rabbat, r),
                         () => context.SaveChanges());
            ShowWindow(frm);
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
            }
            catch (Exception ex)
            {
                Exception(ex, window.GetType());
                var nl = Environment.NewLine;
                var msg = $"Fehler beim Öffnen vom Formular.{nl + nl}{ex.Message}{nl}{ex.InnerException?.Message}";
                MessageBox.Show(window, msg, "Formular Öffnen", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
