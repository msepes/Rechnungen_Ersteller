using DATA;
using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using static Rechnungen.logger;
using static Rechnungen.Binder;
using System.Collections.Generic;

namespace Rechnungen.Windows
{
    /// <summary>
    /// Interaction logic for Clients.xaml
    /// </summary>
    public partial class Clients : Window
    {
        private Func<Kunde, Angebot> NewAngebot;
        private Func<long, Angebot> GetAngebot;
        private Func<Kunde, IEnumerable<Angebot>> GetAngebote;
        private Action<Angebot> DeleteAngebot;
        private Action<Angebot, string> PrintOffer;


        private Func<Kunde, Rechnung> NewRechnung;
        private Func<long, Rechnung> GetRechnung;
        private Func<Kunde, IEnumerable<Rechnung>> GetRechnungen;
        private Action<Rechnung> DeleteRechnung;
        private Action<Rechnung, string> PrintBill;
        private Func<IEnumerable<Rabbat>> GetRabatte;

        private Func<Kunde> NewClient;
        private Func<long, Kunde> GetClient;
        private Action<Kunde> DeleteClient;
        private Func<IEnumerable<Kunde>> GetClients;
        private Action Save;

        public Clients()
        {
            InitializeComponent();
            TextBoxTools.MakeAcceptDigits(txtNummer);
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
        }

        public void RegisterRechnung(Func<Kunde, Rechnung> NewRechnung,
                                     Func<long, Rechnung> GetRechnung,
                                     Func<Kunde, IEnumerable<Rechnung>> GetRechnungen,
                                     Action<Rechnung> DeleteRechnung,
                                     Func<IEnumerable<Rabbat>> GetRabatte,
                                     Action<Rechnung, string> PrintBill)
        {
            this.NewRechnung = NewRechnung;
            this.GetRechnung = GetRechnung;
            this.GetRechnungen = GetRechnungen;
            this.DeleteRechnung = DeleteRechnung;
            this.GetRabatte = GetRabatte;
            this.PrintBill = PrintBill;
        }

        public void RegisterAngebot(Func<Kunde, Angebot> NewAngebot,
                                    Func<long, Angebot> GetAngebot,
                                    Func<Kunde, IEnumerable<Angebot>> GetAngebote,
                                    Action<Angebot> DeleteAngebot,
                                    Func<IEnumerable<Rabbat>> GetRabatte,
                                    Action<Angebot, string> PrintOffer)
        {
            this.NewAngebot = NewAngebot;
            this.GetAngebot = GetAngebot;
            this.GetAngebote = GetAngebote;
            this.DeleteAngebot = DeleteAngebot;
            this.PrintOffer = PrintOffer;
            this.GetRabatte = GetRabatte;
        }

        public void Init()
        {
            fillList();
        }

        private void fillList()
        {
            Unbind();
            lstBox.Items.Clear();
            lstBox.SelectionMode = SelectionMode.Single;
            lstBox.DisplayMemberPath = "Bezeichnung";
            var items = GetClients().Select(c => new ListBoxItem(c.ToString(), c.ID));
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
            if (!TrySave())
                return;

            var offer = new Offer();
            var kunde = GetSelectedKunde();
            offer.Register(() => NewAngebot(kunde),
                           GetAngebot,
                           () => GetAngebote(kunde),
                           Save,
                           DeleteAngebot,
                           GetRabatte,
                           PrintOffer
                           );
            MainWindow.ShowWindow(offer);
        }

        private void btnRechnung_Click(object sender, RoutedEventArgs e)
        {

            if (!TrySave())
                return;

            var bill = new Bill();
            var kunde = GetSelectedKunde();
            bill.Register(() => NewRechnung(kunde),
                            GetRechnung,
                            () => GetRechnungen(kunde),
                            Save,
                            DeleteRechnung,
                            GetRabatte,
                            PrintBill
                            );
            MainWindow.ShowWindow(bill);
            SetGesamt();
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


        private void lstBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Unbind();
            var selectedClient = GetSelectedKunde();
            btnRechnung.IsEnabled = selectedClient != null;
            btnOffers.IsEnabled = selectedClient != null;

            if (selectedClient == null)
                return;

            bind(selectedClient);
            SetGesamt();
        }

        private void SetGesamt()
        {
            var selectedClient = GetSelectedKunde();
            if (selectedClient == null)
            {
                txtGesamt.Text = "0 €";
                return;
            }
            txtGesamt.Text = $"{GetRechnungen(selectedClient)?.Select(r => r.Summe()).Sum()} €";

        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var client = NewClient();
                var i = lstBox.Items.Add(new ListBoxItem(client.ToString(), client.ID));
                lstBox.SelectedItem = lstBox.Items[i];
            }
            catch (Exception ex)
            {
                ExceptionTools.HandleException(ex, this.GetType());
            }

        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var selectedClient = GetSelectedKunde();
                DeleteClient(selectedClient);
                lstBox.Items.Remove(lstBox.SelectedItem);
            }
            catch (Exception ex)
            {
                ExceptionTools.HandleException(ex, this.GetType());
            }

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
            if (!TrySave())
                return;

            this.Close();
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

            item.Bezeichnung = selectedClient.ToString();
            lstBox.Items.Refresh();
        }

        private void lstBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var item = lstBox.ContextMenu.Items.Cast<MenuItem>().FirstOrDefault(i => i.Name == "DELETE");
            if (item == null)
                return;

            item.IsEnabled = lstBox.SelectedItems?.Count > 0 && lstBox.SelectedItem != null;
        }

        private void txtFirma_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateSelectedItem();
        }

        private void txtNummer_LostFocus(object sender, RoutedEventArgs e)
        {
            UpdateSelectedItem();
        }

      
    }
}
