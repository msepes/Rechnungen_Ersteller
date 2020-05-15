using DATA;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using MySqlX.XDevAPI;
using Rechnungen.Forms;
using Rechnungen.Tools;
using Rechnungen.Windows;
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
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Rechnungen.BenutzerTools;

namespace Rechnungen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private BillsContext context;
        public MainWindow()
        {
            InitializeComponent();
            InsertData();
            LoadData();
        }

        private void InsertData()
        {
            context = new BillsContext();
            //context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
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
            frm.ShowDialog();
        }

        private void Clients_Click(object sender, RoutedEventArgs e)
        {
            var frm = new Clients();
            frm.Register(() => ClientsTools.NewKunde(context.Kunden, context.Adressen),
                         (ID) => ClientsTools.GetKunde(context.Kunden, ID),
                          () => ClientsTools.GetKunden(context.Kunden),
                          () =>
                          {
                              context.SaveChanges();
                              ClientsTools.AcceptChanges();
                              RechnungTools.AcceptChanges();
                          },
                          (k) => ClientsTools.DeleteKunde(context.Kunden, k));

            frm.RegisterRechnung((k) => RechnungTools.NewRechnung(context.Rechnungen,k),
                          (ID) => RechnungTools.GetRechnung(context.Rechnungen, ID),
                           () => RechnungTools.GetRechnungen(context.Rechnungen),
                           (r) => RechnungTools.DeleteRechnung(context.Rechnungen, r),
                           () => context.Rabbat);

            frm.ShowDialog();
        }

        private void Bills_Click(object sender, RoutedEventArgs e)
        {
            var frm = new Bill();
            frm.ShowDialog();
        }

        private void Offers_Click(object sender, RoutedEventArgs e)
        {
            var frm = new Offer();
            frm.ShowDialog();
        }

        private void Rabatt_Click(object sender, RoutedEventArgs e)
        {
            var frm = new Rabatt();
            frm.Register(() => RabattTools.NewRabatt(context.Rabbat),
                        (ID) => RabattTools.GetRabatt(context.Rabbat, ID),
                        () => RabattTools.GetRabatte(context.Rabbat),
                        (r) => RabattTools.DeleteRabatt(context.Rabbat, r),
                        () => 
                             { 
                                 context.SaveChanges();
                                 RabattTools.AcceptChanges(); 
                             });
            frm.ShowDialog();
        }


    }
}
