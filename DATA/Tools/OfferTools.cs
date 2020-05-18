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

namespace Angeboten
{
    public static class OfferTools
    {
        private static List<Angebot> Inserted = new List<Angebot>();

        private static IEnumerable<Angebot> GetAll(DbSet<Angebot> AngebotSet)
        {
            return AngebotSet.Include(k => k.Positions).Include(k => k.Rabbat).Include(k => k.Kunde).Include(k => k.Kunde.addresse).ToList().Concat(Inserted);
        }

        public static Angebot NewAngebot(DbSet<Angebot> AngebotSet, Kunde client)
        {
            var Angebot = new Angebot();
            var Angeboten = GetAngeboten(AngebotSet);

            Angebot.ID = Angeboten.Count() > 0 ? Angeboten.Max(k => k.ID) + 1 : 1;
            Angebot.Nr = Angeboten.Count() > 0 ? Angeboten.Max(k => k.Nr) + 1 : 1;
            Angebot.Datum = DateTime.Now;
            Angebot.Umsatzsteuer = 19;
            Angebot.Positions = new ObservableCollection<Angebotsposition>();
            Angebot.Kunde = client;
            AngebotSet.Add(Angebot);
            Inserted.Add(Angebot);

            return Angebot;
        }


        public static void DeleteAngebot(DbSet<Angebot> AngebotSet, Angebot Bill)
        {
            if (AngebotSet.Find(Bill.ID) == null)
                throw new Exception($"DeleteAngebot -> Angebot mit dem ID '{Bill.ID}' wurde nicht gefunden");

            AngebotSet.Remove(Bill);
        }

        public static Angebot GetAngebot(DbSet<Angebot> AngebotSet, long ID)
        {
            return GetAngeboten(AngebotSet).FirstOrDefault(k => k.ID == ID);
        }

        public static IEnumerable<Angebot> GetAngeboten(DbSet<Angebot> AngebotSet)
        {
            return GetAll(AngebotSet);
        }

        public static IEnumerable<Angebot> GetAngeboten(DbSet<Angebot> AngebotSet, Kunde client)
        {
            return GetAll(AngebotSet).Where(r => r.Kunde.ID == client.ID);
        }

        public static void AcceptChanges()
        {
            Inserted.Clear();
        }

        public static void PrintOffer(Angebot Angebot, Benutzer Benutzer, string path)
        {
            if (Angebot == null)
                throw new ArgumentNullException($"{nameof(Angebot)} darf nicht null sein!");

            if (Angebot.Kunde == null)
                throw new ArgumentNullException($"{nameof(Angebot.Kunde)} darf nicht null sein!");

            if (Benutzer == null)
                throw new ArgumentNullException($"{nameof(Benutzer)} darf nicht null sein!");

            if (Angebot.Positions.Count < 1)
                throw new ArgumentException($"{ nameof(Angebot.Positions) } hat keine Elemente!");

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
                prgHeading.Add(new Chunk("MAXCLEAN", fntHead));
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
                prgKunde.Add(new Chunk(Angebot.Kunde.FirmaName, fntKunde));
                prgKunde.Add(lBreak);
                prgKunde.Add(new Chunk(Angebot.Kunde.addresse.ToString(), fntKunde));
                doc.Add(prgKunde);

                PdfPTable pdfTbl = new PdfPTable(3);
                pdfTbl.HorizontalAlignment = Element.ALIGN_RIGHT;
                Font fntColHeader = new Font(baseFont, 6, 1, BaseColor.WHITE);

                PdfPCell RechnungWort = new PdfPCell(new Phrase("Angebot", new Font(baseFont, 14, 1, BaseColor.BLACK)));
                RechnungWort.Chunks.Add(lBreak);
                RechnungWort.Colspan = 3;
                RechnungWort.HorizontalAlignment = 1;

                pdfTbl.AddCell(RechnungWort);

                PdfPCell pdfCell = new PdfPCell();
                pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                pdfCell.Phrase = new Phrase("KundenNr", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                pdfCell.Phrase = new Phrase("Angebotsnummer", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfCell = new PdfPCell();
                pdfCell.BackgroundColor = BaseColor.LIGHT_GRAY;
                pdfCell.Phrase = new Phrase("Datum", fntColHeader);
                pdfTbl.AddCell(pdfCell);

                pdfTbl.AddCell(new Phrase($"{Angebot.Kunde.Nr}"));
                pdfTbl.AddCell(new Phrase($"{Angebot.Nr}"));
                pdfTbl.AddCell(new Phrase(Angebot.Datum.ToShortDateString()));
                pdfTbl.WidthPercentage = 40f;

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

                foreach (var pos in Angebot.Positions)
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
                Netto.Add(new Chunk($"{Angebot.Netto()} ‎€"));

                if (Angebot.Rabbat?.satz > 0)
                {
                    Netto.Add(lBreak);
                    Netto.Add(new Chunk($"{Angebot.Rabbat}"));
                    Netto.Add(new Chunk(spaces));
                    Netto.Add(new Chunk($"- {Angebot.Netto() * (Angebot.Rabbat.satz / 100)} ‎€"));
                }

                Netto.Add(lBreak);
                Netto.Add(new Chunk($"{Angebot.Umsatzsteuer}% Mehrwertsteuer"));
                Netto.Add(new Chunk(spaces));
                Netto.Add(new Chunk($"+ {Angebot.MitRabatt() * (Angebot.Umsatzsteuer / 100)} ‎€"));

                doc.Add(Netto);
                doc.Add(lBreak);
                Paragraph ResultSep = new Paragraph(new Chunk(new LineSeparator(0.0F, 36.0F, BaseColor.BLACK, Element.ALIGN_RIGHT, 1)));
                doc.Add(ResultSep);

                Paragraph Brutto = new Paragraph();
                Brutto.Alignment = Element.ALIGN_RIGHT;
                Brutto.Font = new Font(baseFont, 10, 1, BaseColor.BLACK);
                Brutto.Add(new Chunk("Endsumme"));
                Brutto.Add(new Chunk(spaces));
                Brutto.Add(new Chunk($"{Angebot.Summe()} ‎€"));
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
