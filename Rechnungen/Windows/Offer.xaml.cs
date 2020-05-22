using DATA;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Windows.Controls;
using static Rechnungen.Binder;
using static Rechnungen.logger;
using System.Collections.Generic;
using iTextSharp.text.html.simpleparser;
using Microsoft.Win32;
using System.Diagnostics;
using Rechnungen.Dialogs;

namespace Rechnungen.Windows
{
    /// <summary>
    /// Interaction logic for Offer.xaml
    /// </summary>
    public partial class Offer : Window
    {
        private Func<Angebot> NewAngebot;
        private Func<long, Angebot> GetAngebot;
        private Action<Angebot> DeleteAngebot;
        private Func<IEnumerable<Angebot>> GetAngeboten;
        private Func<IEnumerable<Rabbat>> GetRabatte;
        private Action Save;
        private Func<Angebot, string> Print;
        private Func<EmailConf> GetConf;
        private Benutzer User;

        public Offer()
        {
            InitializeComponent();
            TextBoxTools.MakeAcceptDigits(txtNummer);
            TextBoxTools.MakeAcceptDigits(txtUmsatz);
            GridTools.MakeAcceptDigits(dgrPositionen);
        }

        public void Register(Func<Angebot> NewAngebot,
                             Func<long, Angebot> GetAngebot,
                             Func<IEnumerable<Angebot>> GetAngeboten,
                             Action Save,
                             Action<Angebot> DeleteAngebot,
                             Func<IEnumerable<Rabbat>> GetRabatte,
                             Func<Angebot, string> Print,
                             Func<EmailConf> GetConf,
                             Benutzer User)
        {

            this.NewAngebot = NewAngebot;
            this.GetAngebot = GetAngebot;
            this.DeleteAngebot = DeleteAngebot;
            this.GetAngeboten = GetAngeboten;
            this.GetRabatte = GetRabatte;
            this.Print = Print;
            this.Save = Save;
            this.GetConf = GetConf;
            this.User = User;

            FillRabatte();
            fillList();
        }

        public void Register(Func<long, Angebot> GetAngebot,
                             Func<IEnumerable<Angebot>> GetAngeboten,
                             Action Save,
                             Func<IEnumerable<Rabbat>> GetRabatte,
                             Func<Angebot, string> Print,
                             Func<EmailConf> GetConf,
                             Benutzer User)
        {

            this.GetAngebot = GetAngebot;
            this.GetAngeboten = GetAngeboten;
            this.GetRabatte = GetRabatte;
            this.Print = Print;
            this.Save = Save;
            this.GetConf = GetConf;
            this.User = User;

            FillRabatte();
            fillList();
            lstBox.ContextMenu.Items.Clear();
        }

        private void fillList()
        {
            Unbind();
            lstBox.Items.Clear();
            lstBox.SelectionMode = SelectionMode.Single;

            var items = GetAngeboten().Select(Angebot => new ListBoxItem(Angebot.ToString(), Angebot.ID));
            foreach (var item in items)
                lstBox.Items.Add(item);

            if (lstBox.Items.Count < 1)
                return;

            lstBox.SelectedItem = lstBox.Items[0];
        }

        private void BindGrid()
        {
            dgrPositionen.IsEnabled = true;

            dgrPositionen.Columns.CollectionChanged += (s, e) =>
            {
                if (e.NewItems == null || e.NewItems.Count < 1)
                    return;

                var Newcolumns = e.NewItems.Cast<object>()
                                           .Select(o => o as DataGridColumn)
                                           .Where(o => o != null)
                                           .Where(o => (string)o.Header == "ID" || (string)o.Header == "Angebot");

                foreach (var column in Newcolumns)
                    column.Visibility = Visibility.Hidden;

                GridTools.SetColumnsSize(dgrPositionen);
            };

            dgrPositionen.ItemsSource = GetSelectedAngebot()?.Positions;
        }

        private void FillRabatte()
        {
            var rabatte = GetRabatte();
            foreach (var rabatt in rabatte)
                cboRabatt.Items.Add(rabatt);
        }

        private void bind(Angebot Angebot)
        {
            Unbind();
            BindControl(nameof(Angebot.Nr), Angebot, txtNummer);
            BindControl(nameof(Angebot.Umsatzsteuer), Angebot, txtUmsatz);
            BindControl(nameof(Angebot.Datum), Angebot, dtpDatum);
            BindControl(nameof(Angebot.Rabbat), Angebot, cboRabatt);
            BindGrid();
        }

        private void Unbind()
        {
            Clear(txtNummer);
            Clear(txtUmsatz);
            Clear(dtpDatum);
            Clear(cboRabatt);
            Clear(dgrPositionen);
        }

        private void btnDrucken_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var Angebot = GetSelectedAngebot();
                if (Angebot == null)
                    return;

                Save();

                var path = Print(Angebot);

                var startInfo = new ProcessStartInfo(path);
                startInfo.UseShellExecute = true;
                Process.Start(startInfo);
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

        private void lstBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Unbind();

            var ID = GetSelectedID();
            btnDrucken.IsEnabled = ID.HasValue;
            btnEmail.IsEnabled = ID.HasValue;

            if (!ID.HasValue)
                return;

            var selectedOffer = GetAngebot(ID.Value);
            bind(selectedOffer);
            txtGesamt.Text = $"{selectedOffer.Summe()} €";
            txtClient.Text = selectedOffer.Kunde?.ToString();
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Save();
                this.Close();
            }
            catch (Exception ex)
            {
                ExceptionTools.HandleException(ex, this.GetType());
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var Angebot = NewAngebot();
                var i = lstBox.Items.Add(new ListBoxItem(Angebot.ToString(), Angebot.ID));
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
                var selectedBill = GetSelectedAngebot();
                DeleteAngebot(selectedBill);
                lstBox.Items.Remove(lstBox.SelectedItem);
            }
            catch (Exception ex)
            {
                ExceptionTools.HandleException(ex, this.GetType());
            }

        }

        private Angebot GetSelectedAngebot()
        {
            var ID = GetSelectedID();

            if (!ID.HasValue)
                return null;

            return GetAngebot(ID.Value);
        }

        private void lstBox_ContextMenuOpening(object sender, ContextMenuEventArgs e)
        {
            var item = lstBox.ContextMenu.Items.Cast<MenuItem>().FirstOrDefault(i => i.Name == "DELETE");
            if (item == null)
                return;

            item.IsEnabled = lstBox.SelectedItems?.Count > 0 && lstBox.SelectedItem != null;
        }

        private void dgrPositionen_SizeChanged(object sender, SizeChangedEventArgs e)
        {
          GridTools.SetColumnsSize(dgrPositionen);
        }

        private void dgrPositionen_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
        }

        private void btnEmail_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(User.Email))
            {
                MessageBox.Show("Die Email-Adresse in 'Einge Firma' darf nicht leer sein!", "Email-Senden", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var angebot = GetSelectedAngebot();

            if (string.IsNullOrWhiteSpace(angebot.Kunde.Email))
            {
                MessageBox.Show($"Die Kunde '{angebot.Kunde}' hat keine Email-Adresse!", "Email-Senden", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var frm = new EmailServerDialog();
            frm.Register(GetConf, User, GetSelectedAngebot(), Save);
            MainWindow.ShowWindow(frm);
        }
    }
}
