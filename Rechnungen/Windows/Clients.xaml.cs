using DATA;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using static Rechnungen.logger;
using static Rechnungen.Binder;

namespace Rechnungen.Windows
{
    /// <summary>
    /// Interaction logic for Clients.xaml
    /// </summary>
    public partial class Clients : Window
    {

        private Func<Kunde,Rechnung> NewRechnung;
        private Func<long, Rechnung> GetRechnung;
        private Func<IEnumerable<Rechnung>> GetRechnungen;
        private Action<Rechnung> DeleteRechnung;
        private Func<IEnumerable<Rabbat>> GetRabatte;

        private Func<Kunde> NewClient;
        private Func<long,Kunde> GetClient;
        private Action<Kunde> DeleteClient;
        private Func< IEnumerable<Kunde>> GetClients;
        private Action Save;

        public Clients()
        {
            InitializeComponent();
        }

        public void Register(Func<Kunde> NewClient, 
                             Func<long, Kunde> GetClient, 
                             Func<IEnumerable<Kunde>> GetClients,
                             Action Save,
                             Action<Kunde> DeleteClient)
        {
            this.NewClient = NewClient;
            this.GetClient = GetClient;
            this.DeleteClient = DeleteClient;
            this.GetClients = GetClients;
            this.Save = Save;
            fillList();
        }

        public void RegisterRechnung(Func<Kunde,Rechnung> NewRechnung,
                                     Func<long, Rechnung> GetRechnung,
                                     Func<IEnumerable<Rechnung>> GetRechnungen,
                                     Action<Rechnung> DeleteRechnung,
                                     Func<IEnumerable<Rabbat>> GetRabatte)
        {
            this.NewRechnung = NewRechnung;
            this.GetRechnung = GetRechnung;
            this.GetRechnungen= GetRechnungen;
            this.DeleteRechnung = DeleteRechnung;
            this.GetRabatte = GetRabatte;
        }

        private void fillList()
        {
            Unbind();
            lstBox.Items.Clear();
            lstBox.SelectionMode = SelectionMode.Single;
            lstBox.DisplayMemberPath = "Bezeichnung";
            var items = GetClients().Select(c => new ListBoxItem($"{c.Nr}-{c.FirmaName}", c.ID));
            foreach (var item in items)
                lstBox.Items.Add(item);

            if (lstBox.Items.Count < 1)
                return;

            lstBox.SelectedItem = lstBox.Items[0];

        }

        private void bind(Kunde kunde) 
        {
            Unbind();
            BindControl(nameof(kunde.FirmaName), kunde, txtFirma);
            BindControl(nameof(kunde.Nr), kunde, txtNummer);
            addAdress.Bind(kunde.addresse);
        }

        private void Unbind()
        {
            Clear(txtFirma);
            Clear(txtNummer);
            addAdress.unBind();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var offer = new Offer();

            offer.ShowDialog();
        }

        private void btnRechnung_Click(object sender, RoutedEventArgs e)
        {
            var offer = new Bill();
            offer.Register( () => NewRechnung(GetSelectedKunde()),
                            GetRechnung,
                            GetRechnungen,
                            Save,
                            DeleteRechnung,
                            GetRabatte
                            );
            offer.ShowDialog();
        }

        private void lstBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Unbind();
            var selectedClient = GetSelectedKunde();
            btnRechnung.IsEnabled = selectedClient != null;
            btnOffers.IsEnabled = selectedClient != null;
            btnSave.IsEnabled = selectedClient != null;

            if (selectedClient == null)
                return;

            bind(selectedClient);
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            var client = NewClient();
            var i = lstBox.Items.Add(new ListBoxItem($"{client.Nr}-{client.FirmaName}", client.ID));
            lstBox.SelectedItem = lstBox.Items[i];
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selectedClient = GetSelectedKunde();
            DeleteClient(selectedClient);
            lstBox.Items.Remove(lstBox.SelectedItem);
        }

        private long? GetSelectedID() 
        {
            var item = lstBox.SelectedItem as ListBoxItem;
            return item?.EntityID;
        }

        private Kunde GetSelectedKunde() 
        {
            var ID = GetSelectedID();

            if (!ID.HasValue)
                return null;

          return GetClient(ID.Value);
        }


        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Save();
                fillList();
            }
            catch (Exception ex)
            {
                ex = ex.InnerException;
                var nl = Environment.NewLine;
                var msg = $"Speichern nicht möglich.{nl + nl}{Exception(ex)}";
                MessageBox.Show(this, msg, "Speichern nicht möglich", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void txtFirma_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSelectedItem();
        }

        private void txtNummer_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateSelectedItem();
        }

        private void UpdateSelectedItem() 
        {
            var item = (lstBox.SelectedItem as ListBoxItem);
            if (item == null)
                return;

            var ID = item.EntityID;
            var selectedClient = GetClient(ID);
            if (selectedClient == null)
                return;

            item.Bezeichnung = $"{selectedClient.Nr}-{selectedClient.FirmaName}";
            lstBox.Items.Refresh();
        }

        private void lstBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var item = lstBox.ContextMenu.Items.Cast<MenuItem>().FirstOrDefault(i => i.Name == "DELETE");
            if (item == null)
                return;

            item.IsEnabled = lstBox.SelectedItems?.Count > 0 && lstBox.SelectedItem != null;
        }
    }
}
