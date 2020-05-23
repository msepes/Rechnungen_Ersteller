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
            RegistryTools.ThrowIFLimitReched(RechnungSet.Count());

            var Rechnung = new Rechnung();

            var maxID = RechnungSet.Count() > 0 ? RechnungSet.Max(r => r.ID):0;
            var maxNr = RechnungSet.Count() > 0 ? RechnungSet.Max(r => r.Nr):0;

            var maxInsertedNr = Inserted.Count > 0 ? Inserted.Max(o => o.Nr) : 0;
            var maxInsertedID = Inserted.Count > 0 ? Inserted.Max(o => o.ID) : 0;

            maxNr = maxInsertedNr > maxNr ? maxInsertedNr : maxNr;
            maxID = maxInsertedID > maxID ? maxInsertedID : maxID;

            Rechnung.ID = ++maxID;
            Rechnung.Nr = ++maxNr;
            Rechnung.Datum = DateTime.Now;
            Rechnung.Umsatzsteuer = 19;
            Rechnung.LeistungsDatum = DateTime.Now;
            Rechnung.Positions = new ObservableCollection<Rechnungsposition>();
            var defa = new Rechnungsposition();
            defa.Beschreibung = "Reinigungsstunde";
            Rechnung.Positions.Add(defa);
            Rechnung.Kunde = client;
            RechnungSet.Add(Rechnung);
            Inserted.Add(Rechnung);

            return Rechnung;
        }


        public static void DeleteRechnung(DbSet<Rechnung> RechnungSet, DbSet<Rechnungsposition> RechnungspositionSet, Rechnung Bill)
        {
            RegistryTools.ThrowIFLimitReched(RechnungSet.Count());


            if (RechnungSet.Find(Bill.ID) == null)
                throw new Exception($"DeleteRechnung -> Rechnung mit dem ID '{Bill.ID}' wurde nicht gefunden");

            if(Bill.Positions?.Count > 0)
             RechnungspositionSet.RemoveRange(Bill.Positions);

            RechnungSet.Remove(Bill);

            Bill.Kunde?.Rechnungen?.Remove(Bill);

            var rechnung = Inserted.FirstOrDefault(c => c.ID == Bill.ID);
            if (rechnung != null)
                Inserted.Remove(rechnung);

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

        public static int Count(DbSet<Rechnung> RechnungSet, Kunde client)
        {
            return RechnungSet.Count(r => r.Kunde.ID == client.ID);
        }

        public static void AcceptChanges()
        {
            Inserted.Clear();
        }

        public static string PrintBill(Rechnung Rechnung, Benutzer Benutzer)
        {
            if (Rechnung == null)
                throw new ArgumentNullException($"{nameof(Rechnung)} darf nicht null sein!");

            if (Rechnung.Kunde == null)
                throw new ArgumentNullException($"{nameof(Rechnung.Kunde)} darf nicht null sein!");

            if (Benutzer == null)
                throw new ArgumentNullException($"{nameof(Benutzer)} darf nicht null sein!");

            if (Rechnung.Positions.Count < 1)
                throw new ArgumentException($"{ nameof(Rechnung.Positions) } hat keine Elemente!");

            if(!Directory.Exists(@".\Rechnungen"))
            Directory.CreateDirectory(@".\Rechnungen");

            var Pfad = Path.GetFullPath(@$".\Rechnungen\Rechnung_{Rechnung.Kunde.FirmaName}_{Rechnung.Nr}_{Rechnung.Datum.ToShortDateString()}.pdf");

            FileStream fs = new FileStream(Pfad, FileMode.Create, FileAccess.Write, FileShare.None);
            Document doc = new Document(PageSize.A4);
            PdfWriter prw = PdfWriter.GetInstance(doc, fs);


            try
            {
                Paragraph sep = new Paragraph(new Chunk(new LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Chunk lBreak = new Chunk("\n");

                doc.Open();

                if (!string.IsNullOrWhiteSpace(Benutzer.LogoPath))
                {
                    var jpg = Image.GetInstance(Path.GetFullPath(Benutzer.LogoPath));
                    jpg.ScaleToFit(100f, 72f);
                    jpg.SpacingBefore = 4f;
                    jpg.SpacingAfter = 1f;
                    jpg.Alignment = Element.ALIGN_RIGHT;
                    doc.Add(jpg);
                }

                Paragraph prgHeading = new Paragraph();
                Font fntHead = new Font(baseFont, 20, 1, BaseColor.BLACK);
                prgHeading.Alignment = Element.ALIGN_CENTER;
                prgHeading.Add(new Chunk(Benutzer.FirmaName, fntHead));
                doc.Add(prgHeading);

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
                Font fntColHeader = new Font(baseFont, 8, Font.BOLDITALIC, BaseColor.BLACK);

                PdfPCell RechnungWort = new PdfPCell(new Phrase("Rechnung", new Font(baseFont, 14, 1, BaseColor.BLACK)));
                RechnungWort.Chunks.Add(lBreak);
                RechnungWort.Colspan = 4;
                RechnungWort.HorizontalAlignment = 1;
               
                pdfTbl.AddCell(RechnungWort);

                PdfPCell pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase("KundenNr", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase("RechnungsNr", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase("Leistungsdatum", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
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
                doc.Add(lBreak);

                Paragraph Begruessung = new Paragraph();
                Begruessung.Alignment = Element.ALIGN_LEFT;
                Begruessung.Add(new Chunk("Sehr geehrte Damen und Herren,"));
                doc.Add(Begruessung);
                doc.Add(lBreak);

                pdfTbl = new PdfPTable(5);
                pdfTbl.HorizontalAlignment = Element.ALIGN_JUSTIFIED;

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase("Position", fntColHeader);
                pdfCell.Border = Rectangle.BOTTOM_BORDER;
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase("Leistungsbeschreibung", fntColHeader);
                pdfCell.Border = Rectangle.BOTTOM_BORDER;
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase("Menge", fntColHeader);
                pdfCell.Border = Rectangle.BOTTOM_BORDER;
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase("EINZELPR.", fntColHeader);
                pdfCell.Border = Rectangle.BOTTOM_BORDER;
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase("Gesamt", fntColHeader);
                pdfTbl.SetTotalWidth(new float[] { 10f, 48f, 14f, 14f, 14f });
                pdfCell.Border = Rectangle.BOTTOM_BORDER;
                pdfTbl.WidthPercentage = 100f;
                pdfTbl.AddCell(pdfCell);

                int i = 1;
                Func<int> Counter = () => i++; ;

                foreach (var pos in Rechnung.Positions)
                {
                    pdfCell = new PdfPCell();
                    pdfCell.Phrase = new Phrase($"{Counter()}");
                    pdfCell.Border = Rectangle.NO_BORDER;
                    pdfTbl.AddCell(pdfCell);

                    pdfCell = new PdfPCell();
                    pdfCell.Phrase = new Phrase($"{pos.Beschreibung}");
                    pdfCell.Border = Rectangle.NO_BORDER;
                    pdfTbl.AddCell(pdfCell);

                    pdfCell = new PdfPCell();
                    pdfCell.Phrase = new Phrase($"{pos.Menge}");
                    pdfCell.Border = Rectangle.NO_BORDER;
                    pdfTbl.AddCell(pdfCell);

                    pdfCell = new PdfPCell();
                    pdfCell.Phrase = new Phrase($"{pos.Einzeln_Preis} ‎€");
                    pdfCell.Border = Rectangle.NO_BORDER;
                    pdfTbl.AddCell(pdfCell);

                    pdfCell = new PdfPCell();
                    pdfCell.Phrase = new Phrase($"{(pos.Einzeln_Preis * pos.Menge)} ‎€");
                    pdfCell.Border = Rectangle.NO_BORDER;
                    pdfTbl.AddCell(pdfCell);
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

                if (Rechnung.Umsatzsteuer < 1)
                {
                    doc.Add(lBreak);
                    var fntHinweis = new Font(baseFont, 8, 0, BaseColor.BLACK);
                    Paragraph Umsatz = new Paragraph();
                    Umsatz.Alignment = Element.ALIGN_LEFT;
                    Umsatz.Add(new Chunk("Hinweis:", fntHinweis));
                    Umsatz.Add(lBreak);
                    Umsatz.Add(new Chunk("kein Ausweis der Umsatzsteuer §13b schuldet der Leistungsempfänger die Umsatzsteuer", fntHinweis));
                    doc.Add(Umsatz);
                    doc.Add(lBreak);
                }

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
                Final.Add(new Chunk($"Besuchen Sie uns auf {Benutzer.Web}"));
                doc.Add(Final);
                doc.Add(lBreak);
                doc.Add(lBreak);
                doc.Add(sep);

                pdfTbl = new PdfPTable(3);
                pdfTbl.WidthPercentage = 100f;

                pdfTbl.HorizontalAlignment = Element.ALIGN_LEFT;

                Font infFont = new Font(baseFont, 8, 0, BaseColor.BLACK);
                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase(Benutzer.FirmaName, infFont);
                pdfCell.Border = Rectangle.NO_BORDER;
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase($"Telefone: {Benutzer.Telefone}", infFont);
                pdfCell.Border = Rectangle.NO_BORDER;
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase(Benutzer.BankName, infFont);
                pdfCell.Border = Rectangle.NO_BORDER;
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase($"Steuernr/UID: {Benutzer.SteuerID}", infFont);
                pdfCell.Border = Rectangle.NO_BORDER;
                pdfTbl.AddCell(pdfCell);


                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase($"Email: {Benutzer.Email}", infFont);
                pdfCell.Border = Rectangle.NO_BORDER;
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase($"IBAN: {Benutzer.IBAN}", infFont);
                pdfCell.Border = Rectangle.NO_BORDER;
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase(Benutzer.addresse.ToString(), infFont);
                pdfCell.Border = Rectangle.NO_BORDER;
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase($"Web: {Benutzer.Web}", infFont);
                pdfCell.Border = Rectangle.NO_BORDER;
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase($"BIC: {Benutzer.BIC}", infFont);
                pdfCell.Border = Rectangle.NO_BORDER;
                pdfTbl.AddCell(pdfCell);

                doc.Add(pdfTbl);

                return Pfad;
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
