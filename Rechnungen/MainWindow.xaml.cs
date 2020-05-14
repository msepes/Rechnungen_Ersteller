using DATA;
using Microsoft.EntityFrameworkCore;
using MySqlX.XDevAPI;
using Rechnungen.Forms;
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
        private LibraryContext context;
        public MainWindow()
        {
            InitializeComponent();
            InsertData();
        }

        private void InsertData()
        {
            context = new LibraryContext();
          // context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
            context.SaveChanges();
        }

        private void OwnCompany_Click(object sender, RoutedEventArgs e)
        {
            var frm = new OwnCompany();
            frm.Register(() => GetBenutzer(context.Benutzer, context.Adressen), () => context.SaveChanges());
            frm.Show();
        }

        private void Clients_Click(object sender, RoutedEventArgs e)
        {
            var frm = new Clients();
            frm.Register(() => ClientsTools.NewKunde(context.Kunden, context.Adressen),
                         (ID) => ClientsTools.GetKunde(context.Kunden, ID),
                          () => ClientsTools.GetKunden(context.Kunden),
                          () => { context.SaveChanges(); 
                                  ClientsTools.AcceptChanges(); } ,
                          (k) => ClientsTools.DeleteKunde(context.Kunden, k)) ;
            frm.Show();
        }

        private void Bills_Click(object sender, RoutedEventArgs e)
        {
            var frm = new Bill();
            frm.Show();
        }

        private void Offers_Click(object sender, RoutedEventArgs e)
        {
            var frm = new Offer();
            frm.Show();
        }

        private void Rabatt_Click(object sender, RoutedEventArgs e)
        {
            var frm = new Rabatt();
            frm.Show();
        }
    }
}
