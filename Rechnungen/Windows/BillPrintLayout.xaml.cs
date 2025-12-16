using DATA;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Rechnungen.Windows
{
    public partial class BillPrintLayout : Window
    {
        private readonly Rechnung rechnung;
        private readonly Benutzer benutzer;
        private readonly ObservableCollection<LayoutPosition> layoutPositions = new ObservableCollection<LayoutPosition>();

        public BillPrintLayout(Rechnung rechnung, Benutzer benutzer)
        {
            InitializeComponent();
            this.rechnung = rechnung ?? throw new ArgumentNullException(nameof(rechnung));
            this.benutzer = benutzer ?? new Benutzer();

            BuildHeader();
            BuildClientSection();
            BuildPositions();
            BuildTotals();
            BuildTexts();
            BuildFooter();
        }

        private void BuildHeader()
        {
            txtRechnungsNummer.Text = $"{rechnung.Nr}";
            txtRechnungsDatum.Text = rechnung.Datum.ToShortDateString();
            txtLeistungsdatum.Text = rechnung.LeistungsDatum.ToShortDateString();

            txtFirma.Text = !string.IsNullOrWhiteSpace(benutzer.FirmaName) ? benutzer.FirmaName : "Eigene Firma";
            if (benutzer.addresse != null)
                txtAdresse.Text = benutzer.addresse.ToString();

            var kontakt = string.Join(" | ", new[] { benutzer.Telefone, benutzer.Email }.Where(v => !string.IsNullOrWhiteSpace(v)));
            txtKontakt.Text = kontakt;
            var bank = string.Join(" | ", new[] { benutzer.BankName, $"IBAN: {benutzer.IBAN}", $"BIC: {benutzer.BIC}" }
                .Where(v => !string.IsNullOrWhiteSpace(v)));
            txtBank.Text = bank;
        }

        private void BuildClientSection()
        {
            txtKunde.Text = rechnung.Kunde?.FirmaName;
            txtKundenAdresse.Text = rechnung.Kunde?.addresse?.ToString();
            var kontakt = string.Join(" | ", new[] { rechnung.Kunde?.Telephone, rechnung.Kunde?.Email }.Where(v => !string.IsNullOrWhiteSpace(v)));
            txtKundenKontakt.Text = kontakt;
        }

        private void BuildPositions()
        {
            layoutPositions.Clear();

            var rows = rechnung.Positions?
                .Select((p, index) => new LayoutPosition
                {
                    Nummer = (index + 1).ToString(),
                    Beschreibung = p.Beschreibung,
                    Menge = p.Menge.ToString(),
                    Einzelpreis = $"{p.Einzeln_Preis:F2} €",
                    Gesamt = $"{(p.Einzeln_Preis * p.Menge):F2} €"
                }).ToList();

            if (rows != null)
            {
                foreach (var row in rows)
                {
                    layoutPositions.Add(row);
                }
            }

            dgPositionen.ItemsSource = layoutPositions;
        }

        private void BuildTotals()
        {
            txtNetto.Text = $"{rechnung.Netto():F2} €";
            txtRabatt.Text = $"-{rechnung.Rabatt():F2} €";
            txtSteuer.Text = $"+{rechnung.Steuer():F2} €";
            txtGesamt.Text = $"{rechnung.Summe():F2} €";
        }

        private void BuildTexts()
        {
            txtFreitextOben.Text = rechnung.ZusatzText?.position1 ?? rechnung.ZusatzText?.position2 ?? string.Empty;
            txtFreitextUnten.Text = rechnung.ZusatzText?.position4 ?? rechnung.ZusatzText?.position5 ?? string.Empty;
        }

        private void BuildFooter()
        {
            txtFooterBank.Text = string.Join(" | ", new[] { benutzer.BankName, $"IBAN: {benutzer.IBAN}", $"BIC: {benutzer.BIC}" }
                .Where(v => !string.IsNullOrWhiteSpace(v)));
            txtFooterKontakt.Text = string.Join(" | ", new[] { benutzer.Email, benutzer.Web }
                .Where(v => !string.IsNullOrWhiteSpace(v)));
        }

        private void dgPositionen_RowEditEnding(object sender, DataGridRowEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit && e.Row?.Item is LayoutPosition position)
            {
                if (string.IsNullOrWhiteSpace(position.Nummer))
                {
                    position.Nummer = (layoutPositions.IndexOf(position) + 1).ToString();
                }
            }
        }

        private void btnPrint_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new PrintDialog();
            if (dialog.ShowDialog() == true)
            {
                dialog.PrintVisual(LayoutRoot, $"Rechnung {rechnung.Nr}");
            }
        }

        private void btnClose_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private class LayoutPosition
        {
            public string Nummer { get; set; }
            public string Beschreibung { get; set; }
            public string Menge { get; set; }
            public string Einzelpreis { get; set; }
            public string Gesamt { get; set; }
        }
    }
}
