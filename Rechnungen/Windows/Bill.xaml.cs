using System.IO;
using iTextSharp.text;
using iTextSharp.text.html.simpleparser;
using iTextSharp.text.pdf;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using DATA;
using System.Linq;
using static Rechnungen.logger;
using static Rechnungen.Binder;
using System.Data;
using Microsoft.EntityFrameworkCore;

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
        private Action Save;

        public Bill()
        {
            InitializeComponent();
        }

        public void Register(Func<Rechnung> NewRechnung,
                             Func<long, Rechnung> GetRechnung,
                             Func<IEnumerable<Rechnung>> GetRechnungen,
                             Action Save,
                             Action<Rechnung> DeleteRechnung,
                             Func<IEnumerable<Rabbat>> GetRabatte)
        {

            this.NewRechnung = NewRechnung;
            this.GetRechnung = GetRechnung;
            this.DeleteRechnung = DeleteRechnung;
            this.GetRechnungen = GetRechnungen;
            this.GetRabatte = GetRabatte;
            this.Save = Save;

            FillRabatte();
            fillList();
        }

        private void fillList()
        {
            Unbind();
            lstBox.Items.Clear();
            lstBox.SelectionMode = SelectionMode.Single;
            lstBox.DisplayMemberPath = "Bezeichnung";
            var items = GetRechnungen().Select(Rechnung => new ListBoxItem($"{Rechnung.Nr} - {Rechnung.Datum.ToShortDateString()}", Rechnung.ID));
            foreach (var item in items)
                lstBox.Items.Add(item);

            if (lstBox.Items.Count < 1)
                return;

            lstBox.SelectedItem = lstBox.Items[0];
        }

        private void BindGrid()
        {
            var Positions = GetSelectedRechnung()?.Positions;
            dgrPositionen.ItemsSource = Positions;

            var col = dgrPositionen.Columns.FirstOrDefault(c => (string)c.Header == "ID");
            if(col != null)
                col.Visibility = Visibility.Hidden;

             col = dgrPositionen.Columns.FirstOrDefault(c => (string)c.Header == "Rechnung");
            if (col != null)
                col.Visibility = Visibility.Hidden;
        }

        private void FillRabatte() 
        {
            var rabatte = GetRabatte();
            foreach (var rabatt in rabatte)
                cboRabatt.Items.Add(rabatt);

           cboRabatt.DisplayMemberPath = "Beschreibung";
        }

        private void bind(Rechnung Rechnung)
        {
            Unbind();
            BindControl(nameof(Rechnung.Nr), Rechnung, txtNummer);
            BindControl(nameof(Rechnung.Umsatzsteuer), Rechnung, txtUmsatz);
            BindControl(nameof(Rechnung.LeistungsDatum), Rechnung, dtpLeistungsdatum);
            BindControl(nameof(Rechnung.Datum), Rechnung, dtpDatum);
            BindControl(nameof(Rechnung.Rabbat), Rechnung, cboRabatt);
            BindGrid();
        }

        private void Unbind()
        {
            Clear(txtNummer);
            Clear(txtUmsatz);
            Clear(dtpLeistungsdatum);
            Clear(dtpDatum);
            Clear(cboRabatt);
        }

        private void btnDrucken_Click(object sender, RoutedEventArgs e)
        {
            var html = File.ReadAllText(@"D:\ss.html");
            var document = new Document(); //PageSize.A4, 15f, 15f, 75f, 75f
            var str = new FileStream(@"D:\htmlDocument.pdf", FileMode.OpenOrCreate);
            PdfWriter.GetInstance(document, str);
            document.Open();
            var hw = new HTMLWorker(document);
            hw.Parse(new StringReader(html));
            document.Close();
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

            var sum = selectedBill.Positions?.Select(p => p.Einzeln_Preis * p.Menge).Sum();
            if (!sum.HasValue)
            { 
                txtGesamt.Text = "0";
                return;
            }

            var SummeMitSteuer = sum.Value + ((selectedBill.Umsatzsteuer/100) * sum.Value);

            var rabattSatz = selectedBill.Rabbat?.satz;

            if (!rabattSatz.HasValue) { 
                txtGesamt.Text = $"{SummeMitSteuer}";
                return;
            }
            var SummeMitRabatt = SummeMitSteuer - (SummeMitSteuer * (rabattSatz.Value/100));
            txtGesamt.Text = $"{SummeMitRabatt}";

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

        private void New_Click(object sender, RoutedEventArgs e)
        {
            var Rechnung = NewRechnung();
            var i = lstBox.Items.Add(new ListBoxItem($"{Rechnung.Nr} - {Rechnung.Datum.ToShortDateString()}", Rechnung.ID));
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
    }
}
