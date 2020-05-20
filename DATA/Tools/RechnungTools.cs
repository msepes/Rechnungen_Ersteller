using DATA;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace Rechnungen
{
    public static class RechnungTools
    {
        private static List<Rechnung> Inserted = new List<Rechnung>();

        private static IEnumerable<Rechnung> GetAll(DbSet<Rechnung> RechnungSet)
        {
            return RechnungSet.Include(k => k.Kunde)
                              .OrderBy(r => r.Nr)
                              .ToList()
                              .Concat(Inserted);
        }

        public static Rechnung NewRechnung(DbSet<Rechnung> RechnungSet, Kunde client)
        {
            var Rechnung = new Rechnung();
            var maxID = RechnungSet.Max(r => r.ID);
            var maxNr = RechnungSet.Max(r => r.Nr);

            Rechnung.ID = ++maxID;
            Rechnung.Nr = ++maxNr;
            Rechnung.Datum = DateTime.Now;
            Rechnung.Umsatzsteuer = 19;
            Rechnung.LeistungsDatum = DateTime.Now;
            Rechnung.Positions = new ObservableCollection<Rechnungsposition>();
            Rechnung.Kunde = client;
            RechnungSet.Add(Rechnung);
            Inserted.Add(Rechnung);

            return Rechnung;
        }


        public static void DeleteRechnung(DbSet<Rechnung> RechnungSet, DbSet<Rechnungsposition> RechnungspositionSet, Rechnung Bill)
        {
            if (RechnungSet.Find(Bill.ID) == null)
                throw new Exception($"DeleteRechnung -> Rechnung mit dem ID '{Bill.ID}' wurde nicht gefunden");

            if(Bill.Positions?.Count > 0)
             RechnungspositionSet.RemoveRange(Bill.Positions);

            RechnungSet.Remove(Bill);
        }

        public static Rechnung GetRechnung(DbSet<Rechnung> RechnungSet, long ID)
        {

            return RechnungSet.Where(r => r.ID == ID)
                              .Include(r => r.Rabbat)
                              .Include(r => r.Positions)
                              .Include(r => r.Kunde)
                              .Include(r => r.Kunde.addresse)
                              .ToList()
                              .Concat(Inserted.Where(r => r.ID == ID))
                              .FirstOrDefault();
        }

        public static IEnumerable<Rechnung> GetRechnungen(DbSet<Rechnung> RechnungSet)
        {
            return GetAll(RechnungSet);
        }

        public static IEnumerable<Rechnung> GetRechnungen(DbSet<Rechnung> RechnungSet, Kunde client)
        {
            return RechnungSet.Where(r => r.Kunde.ID == client.ID)
                              .Include(r => r.Rabbat)
                              .Include(r => r.Positions)
                              .Include(r => r.Kunde)
                              .OrderBy(r => r.Nr)
                              .ToList()
                              .Concat(Inserted.Where(r => r.Kunde.ID == client.ID));
        }

        public static void AcceptChanges()
        {
            Inserted.Clear();
        }

        public static void PrintBill(Rechnung Rechnung, Benutzer Benutzer, string path)
        {
            if (Rechnung == null)
                throw new ArgumentNullException($"{nameof(Rechnung)} darf nicht null sein!");

            if (Rechnung.Kunde == null)
                throw new ArgumentNullException($"{nameof(Rechnung.Kunde)} darf nicht null sein!");

            if (Benutzer == null)
                throw new ArgumentNullException($"{nameof(Benutzer)} darf nicht null sein!");

            if (Rechnung.Positions.Count < 1)
                throw new ArgumentException($"{ nameof(Rechnung.Positions) } hat keine Elemente!");

            FileStream fs = new FileStream(path, FileMode.Create, FileAccess.Write, FileShare.None);
            Document doc = new Document(PageSize.A4);
            PdfWriter prw = PdfWriter.GetInstance(doc, fs);

            try
            {
                Paragraph sep = new Paragraph(new Chunk(new LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Chunk lBreak = new Chunk("\n");

                doc.Open();

                Paragraph prgHeading = new Paragraph();
                Font fntHead = new Font(baseFont, 20, 1, BaseColor.BLACK);
                prgHeading.Alignment = Element.ALIGN_CENTER;
                prgHeading.Add(new Chunk(Benutzer.FirmaName, fntHead));
                doc.Add(prgHeading);

                Paragraph OwnCompany = new Paragraph();
                OwnCompany.Alignment = Element.ALIGN_RIGHT;
                OwnCompany.Add(new Chunk(Benutzer.addresse.ToString()));
                OwnCompany.Add(lBreak);
                OwnCompany.Add(lBreak);
                OwnCompany.Add(new Chunk($"Telefone: {Benutzer.Telefone}"));
                OwnCompany.Add(lBreak);
                OwnCompany.Add(new Chunk($"Email: {Benutzer.Email}"));
                OwnCompany.Add(lBreak);
                OwnCompany.Add(new Chunk($"Steuernr/UID: {Benutzer.SteuerID}"));
                OwnCompany.Add(lBreak);
                OwnCompany.Add(lBreak);

                OwnCompany.Add(new Chunk($"Bank: {Benutzer.BankName}"));
                OwnCompany.Add(lBreak);
                OwnCompany.Add(new Chunk($"IBAN: {Benutzer.IBAN}"));
                OwnCompany.Add(lBreak);
                OwnCompany.Add(new Chunk($"BIC: {Benutzer.BIC}"));
                doc.Add(OwnCompany);

                Font fntKunde = new Font(baseFont, 12, 1, BaseColor.BLACK);
                Font fntHeader = new Font(baseFont, 6, 1, BaseColor.BLACK);
                Paragraph prgKunde = new Paragraph();
                prgKunde.Alignment = Element.ALIGN_LEFT;

                var header = new Chunk($"{Benutzer.FirmaName} {Benutzer.addresse.Strasse} {Benutzer.addresse.HasuNr} {Benutzer.addresse.PLZ} {Benutzer.addresse.Ort}", fntHeader);
                header.SetUnderline(0.4f, -3);
                prgKunde.Add(header);
                prgKunde.Add(lBreak);
                prgKunde.Add(new Chunk(Rechnung.Kunde.FirmaName, fntKunde));
                prgKunde.Add(lBreak);
                prgKunde.Add(new Chunk(Rechnung.Kunde.addresse.ToString(), fntKunde));
                doc.Add(prgKunde);

                PdfPTable pdfTbl = new PdfPTable(4);
                pdfTbl.HorizontalAlignment = Element.ALIGN_RIGHT;
                Font fntColHeader = new Font(baseFont, 6, 1, BaseColor.WHITE);

                PdfPCell RechnungWort = new PdfPCell(new Phrase("Rechnung", new Font(baseFont, 14, 1, BaseColor.BLACK)));
                RechnungWort.Chunks.Add(lBreak);
                RechnungWort.Colspan = 4;
                RechnungWort.HorizontalAlignment = 1;
               
                pdfTbl.AddCell(RechnungWort);

                PdfPCell pdfCell = new PdfPCell();
                pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                pdfCell.Phrase = new Phrase("KundenNr", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                pdfCell.Phrase = new Phrase("Rechnungsnummer", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                pdfCell.Phrase = new Phrase("Leistungsdatum", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                pdfCell.Phrase = new Phrase("Datum", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfTbl.AddCell(new Phrase($"{Rechnung.Kunde.Nr}"));
                pdfTbl.AddCell(new Phrase($"{Rechnung.Nr}"));
                pdfTbl.AddCell(new Phrase(Rechnung.LeistungsDatum.ToShortDateString()));
                pdfTbl.AddCell(new Phrase(Rechnung.Datum.ToShortDateString()));

                PdfPCell cellBlankRow = new PdfPCell(new Phrase("Bei Zahlung bitte angeben"));
                cellBlankRow.Colspan = 4;
                cellBlankRow.HorizontalAlignment = 1;
                pdfTbl.AddCell(cellBlankRow);
                pdfTbl.WidthPercentage = 50f;

                doc.Add(pdfTbl);
                doc.Add(sep);
                doc.Add(lBreak);

                Paragraph Begruessung = new Paragraph();
                Begruessung.Alignment = Element.ALIGN_LEFT;
                Begruessung.Add(new Chunk("Sehr geehrte Damen und Herren,"));
                doc.Add(Begruessung);
                doc.Add(lBreak);

                pdfTbl = new PdfPTable(5);
                pdfTbl.HorizontalAlignment = Element.ALIGN_JUSTIFIED;

                pdfCell = new PdfPCell();
                pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                pdfCell.Phrase = new Phrase("Position", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                pdfCell.Phrase = new Phrase("Leistungsbeschreibung", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                pdfCell.Phrase = new Phrase("Menge", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                pdfCell.Phrase = new Phrase("EINZELPR.", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                pdfCell.Phrase = new Phrase("Gesamt", fntColHeader);
                pdfTbl.SetTotalWidth(new float[] { 10f, 48f, 14f, 14f, 14f });
                pdfTbl.WidthPercentage = 100f;
                pdfTbl.AddCell(pdfCell);

                int i = 1;
                Func<int> Counter = () => i++; ;

                foreach (var pos in Rechnung.Positions)
                {
                    pdfTbl.AddCell(new Phrase($"{Counter()}"));
                    pdfTbl.AddCell(new Phrase($"{pos.Beschreibung}"));
                    pdfTbl.AddCell(new Phrase($"{pos.Menge}"));
                    pdfTbl.AddCell(new Phrase($"{pos.Einzeln_Preis} ‎€"));
                    pdfTbl.AddCell(new Phrase($"{(pos.Einzeln_Preis * pos.Menge)} ‎€"));
                }
                doc.Add(pdfTbl);
                doc.Add(lBreak);

                var spaces = new Chunk("            ");
                Paragraph Netto = new Paragraph();
                Netto.Font = new Font(baseFont, 10, 1, BaseColor.BLACK);
                Netto.Alignment = Element.ALIGN_RIGHT;
                Netto.Add(new Chunk("Gesamtsumme Netto"));
                Netto.Add(new Chunk(spaces));
                Netto.Add(new Chunk($"{Rechnung.Netto().ToString("F2")} ‎€"));

                if (Rechnung.Rabbat?.satz > 0)
                {
                    Netto.Add(lBreak);
                    Netto.Add(new Chunk($"{Rechnung.Rabbat}"));
                    Netto.Add(new Chunk(spaces));
                    Netto.Add(new Chunk($"- {(Rechnung.Netto() * (Rechnung.Rabbat.satz / 100)).ToString("F2")} ‎€"));
                }

                Netto.Add(lBreak);
                Netto.Add(new Chunk($"{Rechnung.Umsatzsteuer}% Mehrwertsteuer"));
                Netto.Add(new Chunk(spaces));
                Netto.Add(new Chunk($"+ {(Rechnung.MitRabatt() * (Rechnung.Umsatzsteuer / 100)).ToString("F2")} ‎€"));

                doc.Add(Netto);
                doc.Add(lBreak);
                Paragraph ResultSep = new Paragraph(new Chunk(new LineSeparator(0.0F, 36.0F, BaseColor.BLACK, Element.ALIGN_RIGHT, 1)));
                doc.Add(ResultSep);

                Paragraph Brutto = new Paragraph();
                Brutto.Alignment = Element.ALIGN_RIGHT;
                Brutto.Font = new Font(baseFont, 10, 1, BaseColor.BLACK);
                Brutto.Add(new Chunk("Endsumme"));
                Brutto.Add(new Chunk(spaces));
                Brutto.Add(new Chunk($"{Rechnung.Summe().ToString("F2")} ‎€"));
                doc.Add(Brutto);
                doc.Add(lBreak);
                doc.Add(sep);

                Paragraph Final = new Paragraph();
                Final.Alignment = Element.ALIGN_LEFT;
                Final.Add(new Chunk("Bitte überweisen Sie den Betrag innerhalb von 7 Tagen auf unser Konto."));
                Final.Add(lBreak);
                Final.Add(lBreak);
                Final.Add(new Chunk("Mit freundlichen Grüßen."));
                Final.Add(lBreak);
                Final.Add(lBreak);
                Final.Add(new Chunk("MaxClean"));
                Final.Add(lBreak);
                Final.Add(new Chunk("Besuchen Sie uns auf www.max--clean.de"));
                doc.Add(Final);
            }
            finally
            {
                doc.Close();
                prw.Close();
                fs.Close();
            }
        }

    }
}
