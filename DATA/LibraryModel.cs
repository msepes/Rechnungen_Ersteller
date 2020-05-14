using System;
using System.Collections.Generic;
using System.Text;

namespace DATA
{
    public class Adresse
    {
        public long ID { get; set; }
        public string Ort { get; set; }
        public string PLZ { get; set; }
        public string Strasse { get; set; }
        public string HasuNr { get; set; }
        public ICollection<Kunde> Kunden { get; set; }
    }

    public class Angebotsposition
    {
        public long ID { get; set; }
        public string Beschreibung { get; set; }
        public double Einzeln_Preis { get; set; }
        public double Menge { get; set; }
        public Angebot Angebot { get; set; }
    }

    public class Rechnungsposition
    {
        public long ID { get; set; }
        public string Beschreibung { get; set; }
        public double Einzeln_Preis { get; set; }
        public double Menge { get; set; }
        public Rechnung Rechnung { get; set; }
    }

    public class Kunde
    {
        public long ID { get; set; }
        public string FirmaName { get; set; }
        public long Nr { get; set; }
        public virtual Adresse addresse { get; set; }
        public ICollection<Rechnung> Rechnungen { get; set; }
        public ICollection<Angebot> Angebote { get; set; }
    }

    public class Benutzer
    {
        public long ID { get; set; }
        public string Name { get; set; }
        public string Vorname { get; set; }
        public string FirmaName { get; set; }
        public string Telefone { get; set; }
        public string Email { get; set; }
        public string SteuerID { get; set; }
        public string BankName { get; set; }
        public string IBAN { get; set; }
        public string BIC { get; set; }
        public virtual Adresse addresse { get; set; }
    }

    public class Rechnung
    {
        public long ID { get; set; }
        public long Nr { get; set; }
        public double Umsatzsteuer { get; set; }
        public Kunde Kunde { get; set; }
        public DateTime Datum { get; set; }
        public DateTime LeistungsDatum { get; set; }
        public Rabbat Rabbat { get; set; }
        public virtual ICollection<Rechnungsposition> Positions { get; set; }
    }

    public class Angebot
    {
        public long ID { get; set; }
        public long Nr { get; set; }
        public double Umsatzsteuer { get; set; }
        public Kunde Kunde { get; set; }
        public DateTime Datum { get; set; }
        public Rabbat Rabbat { get; set; }
        public virtual ICollection<Angebotsposition> Positions { get; set; }
    }

    public class Rabbat
    {
        public long ID { get; set; }
        public double satz { get; set; }
        public string Beschreibung { get; set; }
    }

}
