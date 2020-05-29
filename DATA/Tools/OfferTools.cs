using DATA;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.pdf.draw;
using System.IO;
using static System.Convert;

namespace Angeboten
{
    public static class OfferTools
    {
        private static List<Angebot> Inserted = new List<Angebot>();


        public static Angebot NewAngebot(DbSet<Angebot> AngebotSet, Kunde client)
        {
            RegistryTools.ThrowIFLimitReched(AngebotSet.Count());


            var Angebot = new Angebot();

            var maxNr = AngebotSet.Count() > 0 ? AngebotSet.Max(r => r.Nr):0;
            var maxID = AngebotSet.Count() > 0 ? AngebotSet.Max(r => r.ID):0;
            var maxInsertedNr = Inserted.Count > 0 ? Inserted.Max(o => o.Nr) : 0;
            var maxInsertedID = Inserted.Count > 0 ? Inserted.Max(o => o.ID) : 0;

            maxNr = maxInsertedNr > maxNr ? maxInsertedNr : maxNr;
            maxID = maxInsertedID > maxID ? maxInsertedID : maxID;

            Angebot.ID = ++maxID;
            Angebot.Nr = ++maxNr ;
            Angebot.Datum = DateTime.Now;
            Angebot.Umsatzsteuer = 19;
            Angebot.Positions = new ObservableCollection<Angebotsposition>();
            var defa = new Angebotsposition();
            defa.Beschreibung = "Reinigungsstunde";
            Angebot.Positions.Add(defa);
            Angebot.Kunde = client;
            AngebotSet.Add(Angebot);
            Inserted.Add(Angebot);

            return Angebot;
        }

        public static void DeleteAngebot(DbSet<Angebot> AngebotSet, DbSet<Angebotsposition> AngebotspositionSet, Angebot Offer)
        {
            RegistryTools.ThrowIFLimitReched(AngebotSet.Count());


            if (AngebotSet.Find(Offer.ID) == null)
                throw new Exception($"DeleteAngebot -> Angebot mit dem ID '{Offer.ID}' wurde nicht gefunden");

            if (Offer.Positions?.Count > 0)
                AngebotspositionSet.RemoveRange(Offer.Positions);

            AngebotSet.Remove(Offer);
            Offer.Kunde?.Angebote?.Remove(Offer);

            var angebot = Inserted.FirstOrDefault(c => c.ID == Offer.ID);
            if (angebot != null)
                Inserted.Remove(angebot);
        }

        public static void AcceptChanges() => 
               Inserted.Clear();

        public static IEnumerable<Angebot> GetAngeboten(DbSet<Angebot> AngebotSet) => 
               GetAll(AngebotSet);

        public static Angebot GetAngebot(DbSet<Angebot> AngebotSet, long ID) =>
               AngebotSet.Where(a => a.ID == ID)
                         .Include(a => a.Rabbat)
                         .Include(a => a.Positions)
                         .Include(a => a.Kunde)
                         .Include(a => a.Kunde.addresse)
                         .ToList()
                         .Concat(Inserted.Where(a => a.ID == ID))
                         .FirstOrDefault();

        public static IEnumerable<Angebot> GetAngeboten(DbSet<Angebot> AngebotSet, Kunde client) =>
               AngebotSet.Where(r => r.Kunde.ID == client.ID)
                         .Include(a => a.Rabbat)
                         .Include(a => a.Positions)
                         .OrderBy(a => a.Nr)
                         .ToList()
                         .Concat(Inserted.Where(a => a.Kunde.ID == client.ID));

        private static IEnumerable<Angebot> GetAll(DbSet<Angebot> AngebotSet) => 
               AngebotSet.ToList().Concat(Inserted);

        public static int Count(DbSet<Angebot> AngebotSet, Kunde client)
        {
            return AngebotSet.Count(r => r.Kunde.ID == client.ID);
        }

        public static string PrintOffer(Angebot Angebot, Benutzer Benutzer)
        {
            if (Angebot == null)
                throw new ArgumentNullException($"{nameof(Angebot)} darf nicht null sein!");

            if (Angebot.Kunde == null)
                throw new ArgumentNullException($"{nameof(Angebot.Kunde)} darf nicht null sein!");

            if (Benutzer == null)
                throw new ArgumentNullException($"{nameof(Benutzer)} darf nicht null sein!");

            if (Angebot.Positions.Count < 1)
                throw new ArgumentException($"{ nameof(Angebot.Positions) } hat keine Elemente!");

            if (!Directory.Exists(@".\Angebote"))
                Directory.CreateDirectory(@".\Angebote");

            var Pfad = Path.GetFullPath(@$".\Angebote\Angebot_{Angebot.Nr}_{Angebot.Kunde.FirmaName}_{Angebot.Datum.ToShortDateString()}.pdf");

            FileStream fs = new FileStream(Pfad, FileMode.Create, FileAccess.Write, FileShare.None);
            Document doc = new Document(PageSize.A4);
            PdfWriter prw = PdfWriter.GetInstance(doc, fs);

            try
            {
                Paragraph sep = new Paragraph(new Chunk(new LineSeparator(0.0F, 100.0F, BaseColor.BLACK, Element.ALIGN_LEFT, 1)));
                BaseFont baseFont = BaseFont.CreateFont(BaseFont.HELVETICA, BaseFont.CP1252, BaseFont.NOT_EMBEDDED);
                Chunk lBreak = new Chunk("\n");

                doc.Open();

                if (!string.IsNullOrWhiteSpace(Benutzer.LogoPath)) {
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
                prgKunde.Add(new Chunk(Angebot.Kunde.FirmaName, fntKunde));
                prgKunde.Add(lBreak);
                prgKunde.Add(new Chunk(Angebot.Kunde.addresse.ToString(), fntKunde));
                doc.Add(prgKunde);

                PdfPTable pdfTbl = new PdfPTable(3);
                pdfTbl.HorizontalAlignment = Element.ALIGN_RIGHT;
                Font fntColHeader = new Font(baseFont, 8, Font.BOLDITALIC, BaseColor.BLACK);

                PdfPCell AngebotWort = new PdfPCell(new Phrase("Angebot", new Font(baseFont, 14, 1, BaseColor.BLACK)));
                AngebotWort.Colspan = 3;
                AngebotWort.HorizontalAlignment = 1;
                pdfTbl.AddCell(AngebotWort);

                PdfPCell pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase("KundenNr", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase("Angebotsnummer", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.Phrase = new Phrase("Datum", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfTbl.AddCell(new Phrase($"{Angebot.Kunde.Nr}"));
                pdfTbl.AddCell(new Phrase($"{Angebot.Nr}"));
                pdfTbl.AddCell(new Phrase(Angebot.Datum.ToShortDateString()));
                pdfTbl.WidthPercentage = 40f;

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

                foreach (var pos in Angebot.Positions)
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
                Netto.Add(new Chunk($"{Angebot.Netto().ToString("F2")} ‎€"));

                if (Angebot.Rabbat?.satz > 0)
                {
                    Netto.Add(lBreak);
                    Netto.Add(new Chunk($"{Angebot.Rabbat}"));
                    Netto.Add(new Chunk(spaces));
                    Netto.Add(new Chunk($"- {(Angebot.Netto() * (ToDecimal(Angebot.Rabbat.satz) / 100m)).ToString("F2")} ‎€"));
                }

                Netto.Add(lBreak);
                Netto.Add(new Chunk($"{Angebot.Umsatzsteuer}% Mehrwertsteuer"));
                Netto.Add(new Chunk(spaces));
                Netto.Add(new Chunk($"+ {(Angebot.MitRabatt() * (ToDecimal(Angebot.Umsatzsteuer) / 100m)).ToString("F2")} ‎€"));

                doc.Add(Netto);
                doc.Add(lBreak);
                Paragraph ResultSep = new Paragraph(new Chunk(new LineSeparator(0.0F, 36.0F, BaseColor.BLACK, Element.ALIGN_RIGHT, 1)));
                doc.Add(ResultSep);

                Paragraph Brutto = new Paragraph();
                Brutto.Alignment = Element.ALIGN_RIGHT;
                Brutto.Font = new Font(baseFont, 10, 1, BaseColor.BLACK);
                Brutto.Add(new Chunk("Endsumme"));
                Brutto.Add(new Chunk(spaces));
                Brutto.Add(new Chunk($"{Angebot.Summe().ToString("F2")} ‎€"));
                doc.Add(Brutto);
                doc.Add(lBreak);

                doc.Add(sep);

                Paragraph Final = new Paragraph();
                Final.Alignment = Element.ALIGN_LEFT;
                Final.Add(new Chunk("Wir würden uns auf zukünftige Zusammenarbeit sehr freuen."));
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
