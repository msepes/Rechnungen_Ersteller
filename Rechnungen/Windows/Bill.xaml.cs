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
        private Action<Rechnung,string> Print;
        private Action Save;

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
                             Action<Rechnung, string> Print)
        {

            this.NewRechnung = NewRechnung;
            this.GetRechnung = GetRechnung;
            this.DeleteRechnung = DeleteRechnung;
            this.GetRechnungen = GetRechnungen;
            this.GetRabatte = GetRabatte;
            this.Print = Print;
            this.Save = Save;

            FillRabatte();
            fillList();
        }

        public void Register(Func<long, Rechnung> GetRechnung,
                             Func<IEnumerable<Rechnung>> GetRechnungen,
                             Action Save,
                             Func<IEnumerable<Rabbat>> GetRabatte,
                             Action<Rechnung, string> Print)
        {
            this.GetRechnung = GetRechnung;
            this.GetRechnungen = GetRechnungen;
            this.GetRabatte = GetRabatte;
            this.Print = Print;
            this.Save = Save;

            FillRabatte();
            fillList();

            lstBox.ContextMenu.Items.Clear();
        }

        private void fillList()
        {
            Unbind();
            lstBox.Items.Clear();
            lstBox.SelectionMode = SelectionMode.Single;
            lstBox.DisplayMemberPath = "Bezeichnung";
            var items = GetRechnungen().Select(Rechnung => new ListBoxItem(Rechnung.ToString(), Rechnung.ID));
            foreach (var item in items)
                lstBox.Items.Add(item);

            if (lstBox.Items.Count < 1)
                return;

            lstBox.SelectedItem = lstBox.Items[0];
        }

        private void BindGrid(Rechnung Rechnung)
        {
            var Positions = Rechnung?.Positions;
            dgrPositionen.ItemsSource = Positions;

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

                SetColumnsSize();
            };
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
            cboRabatt.SelectedItem = null;
        }

        private void btnDrucken_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var Rechnung = GetSelectedRechnung();
                if (Rechnung == null)
                    return;

                SaveFileDialog saveFileDialog = new SaveFileDialog();
                saveFileDialog.FileName = @$"Rechnung_{Rechnung.Kunde.FirmaName}_{Rechnung.Datum.ToShortDateString()}_{Rechnung.Nr}.pdf";

                if (saveFileDialog.ShowDialog() != true)
                    return;

                Print(Rechnung, saveFileDialog.FileName);

                var startInfo = new ProcessStartInfo(saveFileDialog.FileName);
                startInfo.UseShellExecute = true;
                Process.Start(startInfo);

            }
            catch (Exception ex)
            {
                var nl = Environment.NewLine;
                Exception(ex,this.GetType());
                var msg = $"Fehler beim Rechnung-Drucken.{nl + nl}{ex.Message}{Environment.NewLine}{ex.InnerException?.Message}";
                MessageBox.Show(this, msg, "Rechnung-Drucken", MessageBoxButton.OK, MessageBoxImage.Error);
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
            if (!ID.HasValue)
                return;
            var selectedBill = GetRechnung(ID.Value);

            bind(selectedBill);
            txtGesamt.Text = $"{selectedBill.Summe()} €";
            txtClient.Text = selectedBill.Kunde?.ToString();
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
                ex = ex.InnerException ?? ex;
                var nl = Environment.NewLine;
                Exception(ex, this.GetType());
                var msg = $"Speichern nicht möglich.{nl + nl}{ex.Message}";
                MessageBox.Show(this, msg, "Speichern nicht möglich", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            var Rechnung = NewRechnung();
            var i = lstBox.Items.Add(new ListBoxItem(Rechnung.ToString(), Rechnung.ID));
            lstBox.SelectedItem = lstBox.Items[i];
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var selectedBill = GetSelectedRechnung();
            DeleteRechnung(selectedBill);
            lstBox.Items.Remove(lstBox.SelectedItem);
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
            SetColumnsSize();
        }

        private void SetColumnsSize() 
        {
            foreach (var column in dgrPositionen.Columns) { 
                switch ((string)column.Header)
                {
                    case "ID":
                    case "Rechnung":
                        break;
                    case "Beschreibung":
                        if (dgrPositionen.ActualWidth > 0) column.Width = dgrPositionen.ActualWidth * 0.7;
                        break;
                    case "Menge":
                    case "Einzeln_Preis":
                        if (dgrPositionen.ActualWidth > 0) column.Width = dgrPositionen.ActualWidth * 0.15;
                        break;
                }
            }

        }
    }
}
