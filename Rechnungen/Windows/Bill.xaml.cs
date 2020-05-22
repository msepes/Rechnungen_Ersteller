using DATA;
using System;
using System.IO;
using System.Linq;
using System.Data;
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
using System.Text.RegularExpressions;
using Rechnungen.Dialogs;
using Renci.SshNet.Common;

namespace Rechnungen.Windows
{
    /// <summary>
    /// Interaction logic for Bill.xaml
    /// </summary>
    public partial class Bill : Window
    {
        private Func<Rechnung> NewRechnung;
        private Func<long, Rechnung> GetRechnung;
        private Action<Rechnung> DeleteRechnung;
        private Func<IEnumerable<Rechnung>> GetRechnungen;
        private Func<IEnumerable<Rabbat>> GetRabatte;
        private Func<Rechnung, string> Print;
        private Func<EmailConf> GetConf;
        private Action Save;

        private Benutzer User;

        public Bill()
        {
            InitializeComponent();
            TextBoxTools.MakeAcceptDigits(txtNummer);
            TextBoxTools.MakeAcceptDigits(txtUmsatz);
            GridTools.MakeAcceptDigits(dgrPositionen);
        }

        public void Register(Func<Rechnung> NewRechnung,
                             Func<long, Rechnung> GetRechnung,
                             Func<IEnumerable<Rechnung>> GetRechnungen,
                             Action Save,
                             Action<Rechnung> DeleteRechnung,
                             Func<IEnumerable<Rabbat>> GetRabatte,
                             Func<Rechnung, string> Print,
                             Func<EmailConf> GetConf,
                             Benutzer User)
        {

            this.NewRechnung = NewRechnung;
            this.GetRechnung = GetRechnung;
            this.DeleteRechnung = DeleteRechnung;
            this.GetRechnungen = GetRechnungen;
            this.GetRabatte = GetRabatte;
            this.Print = Print;
            this.Save = Save;
            this.User = User;
            this.GetConf = GetConf;


            FillRabatte();
            fillList(Rechnung => Rechnung.ToString());
        }

        public void Register(Func<long, Rechnung> GetRechnung,
                             Func<IEnumerable<Rechnung>> GetRechnungen,
                             Action Save,
                             Func<IEnumerable<Rabbat>> GetRabatte,
                             Func<Rechnung, string> Print,
                             Func<EmailConf> GetConf,
                             Benutzer User)
        {
            this.GetRechnung = GetRechnung;
            this.GetRechnungen = GetRechnungen;
            this.GetRabatte = GetRabatte;
            this.Print = Print;
            this.Save = Save;
            this.User = User;
            this.GetConf = GetConf;
            FillRabatte();
            fillList(Rechnung => $"{Rechnung.Nr} - {Rechnung.Kunde.FirmaName} - {Rechnung.Datum.ToShortDateString()}");

            lstBox.ContextMenu.Items.Clear();
        }

        private void fillList(Func<Rechnung, string> GetDisplay)
        {
            Unbind();
            lstBox.Items.Clear();
            lstBox.SelectionMode = SelectionMode.Single;
            lstBox.DisplayMemberPath = "Bezeichnung";
            var items = GetRechnungen().Select(Rechnung => new ListBoxItem(GetDisplay(Rechnung), Rechnung.ID));
            foreach (var item in items)
                lstBox.Items.Add(item);

            if (lstBox.Items.Count < 1)
                return;

            lstBox.SelectedItem = lstBox.Items[0];
        }

        private void BindGrid(Rechnung Rechnung)
        {
            dgrPositionen.IsEnabled = true;

            dgrPositionen.Columns.CollectionChanged += (s, e) =>
            {
                if (e.NewItems == null || e.NewItems.Count < 1)
                    return;

                var Newcolumns = e.NewItems.Cast<object>()
                                         .Select(o => o as DataGridColumn)
                                         .Where(o => o != null)
                                         .Where(o => (string)o.Header == "ID" || (string)o.Header == "Rechnung");

                foreach (var column in Newcolumns)
                    column.Visibility = Visibility.Hidden;

                GridTools.SetColumnsSize(dgrPositionen);
            };

            dgrPositionen.ItemsSource = Rechnung?.Positions;
        }

        private void FillRabatte()
        {
            var rabatte = GetRabatte();
            foreach (var rabatt in rabatte)
                cboRabatt.Items.Add(rabatt);
        }

        private void bind(Rechnung Rechnung)
        {
            Unbind();
            BindControl(nameof(Rechnung.Nr), Rechnung, txtNummer);
            BindControl(nameof(Rechnung.Umsatzsteuer), Rechnung, txtUmsatz);
            BindControl(nameof(Rechnung.LeistungsDatum), Rechnung, dtpLeistungsdatum);
            BindControl(nameof(Rechnung.Datum), Rechnung, dtpDatum);
            BindControl(nameof(Rechnung.Rabbat), Rechnung, cboRabatt);
            BindGrid(Rechnung);
        }

        private void Unbind()
        {
            Clear(txtNummer);
            Clear(txtUmsatz);
            Clear(dtpLeistungsdatum);
            Clear(dtpDatum);
            Clear(cboRabatt);
            Clear(dgrPositionen);
        }

        private void btnDrucken_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var Rechnung = GetSelectedRechnung();
                if (Rechnung == null)
                    return;

                Save();

                var path = Print(Rechnung);
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
            btnMail.IsEnabled = ID.HasValue;

            if (!ID.HasValue)
                return;
            var selectedBill = GetRechnung(ID.Value);

            bind(selectedBill);

            SetSummenAnzeige();
            txtClient.Text = selectedBill.Kunde?.ToString();
        }

        private void SetSummenAnzeige()
        {
            var ID = GetSelectedID();

            if (!ID.HasValue)
                return;
            var selectedBill = GetRechnung(ID.Value);

            txtGesamt.Text = $"{selectedBill.Summe().ToString("F2")} €";
            txtNetto.Text = $"+{selectedBill.Netto().ToString("F2")} €";
            txtUmsatzsteuer.Text = $"+{selectedBill.Steuer().ToString("F2")} €";
            txtRabatt.Text = $"-{selectedBill.Rabatt().ToString("F2")} €";
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
                var Rechnung = NewRechnung();
                var i = lstBox.Items.Add(new ListBoxItem(Rechnung.ToString(), Rechnung.ID));
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
                var selectedBill = GetSelectedRechnung();
                DeleteRechnung(selectedBill);
                lstBox.Items.Remove(lstBox.SelectedItem);
            }
            catch (Exception ex)
            {
                ExceptionTools.HandleException(ex, this.GetType());
            }

        }

        private Rechnung GetSelectedRechnung()
        {
            var ID = GetSelectedID();

            if (!ID.HasValue)
                return null;

            return GetRechnung(ID.Value);
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

        private void cboRabatt_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SetSummenAnzeige();
        }


        private void dgrPositionen_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var head = (string)e.Column.Header;

            if (head != "Menge" && head != "Einzeln_Preis")
                return;

            SetSummenAnzeige();
        }

        private void txtUmsatz_LostFocus(object sender, RoutedEventArgs e)
        {
            SetSummenAnzeige();
        }

        private void btnMail_Click(object sender, RoutedEventArgs e)
        {

            if (string.IsNullOrWhiteSpace(User.Email))
            {
                MessageBox.Show("Die Email-Adresse in 'Einge Firma' darf nicht leer sein!", "Email-Senden", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var rechnung = GetSelectedRechnung();

            if (string.IsNullOrWhiteSpace(rechnung.Kunde.Email))
            {
                MessageBox.Show($"Die Kunde '{rechnung.Kunde}' hat keine Email-Adresse!", "Email-Senden", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var frm = new EmailServerDialog();
            frm.Register(GetConf, User, rechnung, Save);
            MainWindow.ShowWindow(frm);

        }
    }
}
