using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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
        public ObservableCollection<Kunde> Kunden { get; set; }

        public override string ToString()
        {
            return $"{Strasse} {HasuNr}{Environment.NewLine}{PLZ} {Ort}";
        }
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
        public ObservableCollection<Rechnung> Rechnungen { get; set; }
        public ObservableCollection<Angebot> Angebote { get; set; }

        public override string ToString()
        {
            return $"{Nr} - {FirmaName}";
        }
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
        public virtual ObservableCollection<Rechnungsposition> Positions { get; set; }

        public override string ToString()
        {
            return $"{Nr} - {Datum.ToShortDateString()}";
        }

        public double Netto() 
        {
            var sum = this.Positions?.Select(p => p.Einzeln_Preis * p.Menge).Sum();
            if (!sum.HasValue)
                return 0.0;

            return sum.Value;
        }

        public double MitRabatt()
        {
            var nett = Netto();

            if (!(this.Rabbat?.satz > 1))
                return nett;

            return nett - this.Rabbat.satz / 100 * nett;
        }

        public double Summe() 
        {
            var nett = MitRabatt();
            return nett + this.Umsatzsteuer / 100 * nett;
        }
    }

    public class Angebot
    {
        public long ID { get; set; }
        public long Nr { get; set; }
        public double Umsatzsteuer { get; set; }
        public Kunde Kunde { get; set; }
        public DateTime Datum { get; set; }
        public Rabbat Rabbat { get; set; }
        public virtual ObservableCollection<Angebotsposition> Positions { get; set; }

        public override string ToString()
        {
            return $"{Nr} - {Datum.ToShortDateString()}";
        }

        public double Netto()
        {
            var sum = this.Positions?.Select(p => p.Einzeln_Preis * p.Menge).Sum();
            if (!sum.HasValue)
                return 0.0;

            return sum.Value;
        }

        public double MitRabatt()
        {
            var nett = Netto();

            if (!(this.Rabbat?.satz > 1))
                return nett;

            return nett - this.Rabbat.satz / 100 * nett  ;
        }

        public double Summe()
        {
            var nett = MitRabatt();
            return nett + this.Umsatzsteuer / 100 * nett;
        }

    }

    public class Rabbat
    {
        public long ID { get; set; }
        public double satz { get; set; }
        public long Nr { get; set; }
        public string Beschreibung { get; set; }

        public override string ToString()
        {
            return $"{Beschreibung} - {satz}%";
        }


    }

}
