using Microsoft.EntityFrameworkCore;
using Rechnungen;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security;
using System.Security.Cryptography;
using System.Text;
using static System.Convert;
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
        public string Email { get; set; }
        public string Telephone { get; set; }
        public string Ansprechpartner { get; set; }
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
        public string Web { get; set; }
        public string LogoPath { get; set; } = @".\FirmLogo.jpg";
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

        public decimal Netto()
        {
            var sum = this.Positions?.Select(p => ToDecimal(p.Einzeln_Preis) * ToDecimal(p.Menge)).Sum();
            if (!sum.HasValue)
                return 0M;

            return sum.Value;
        }

        public decimal Steuer()
        {
            return ToDecimal(this.Umsatzsteuer) / 100m * MitRabatt();
        }

        public decimal MitSteuer()
        {
            return MitRabatt() + Steuer();
        }

        public decimal Rabatt()
        {
            if (!(this.Rabbat?.satz > 1))
                return 0m;

            return ToDecimal(this.Rabbat.satz) / 100m * Netto();
        }

        public decimal MitRabatt()
        {
            return Netto() - Rabatt();
        }

        public decimal Summe()
        {
            return MitSteuer();
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

        public decimal Netto()
        {
            var sum = this.Positions?.Select(p => ToDecimal(p.Einzeln_Preis) * ToDecimal(p.Menge)).Sum();
            if (!sum.HasValue)
                return 0M;

            return sum.Value;
        }

        public decimal Steuer()
        {
            return ToDecimal(this.Umsatzsteuer) / 100m * MitRabatt();
        }

        public decimal MitSteuer()
        {
            return MitRabatt() + Steuer();
        }

        public decimal Rabatt()
        {
            if (!(this.Rabbat?.satz > 1))
                return 0m;

            return ToDecimal(this.Rabbat.satz) / 100m * Netto();
        }

        public decimal MitRabatt()
        {
            return Netto() - Rabatt();
        }

        public decimal Summe()
        {
            return MitSteuer();
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

    public enum ConfTyp
    {
        Rechnung = 0,
        Angebot = 1
    }
    public class EmailConf 
    {

        private string _strPassword;

        public int ID { get; set; }
        public ConfTyp typ { get; set; }
        public string EmailServer { get; set; }
        public string UserName { get; set; }
        public int Port { get; set; } = 25;
        public string EmailInhalt{ get; set; }
        public string EmailBetriff { get; set; }

        public string password
        {
            get
            {
                return EncryptionHelper.Decrypt(_strPassword);
            }
            set
            {
                _strPassword = EncryptionHelper.Encrypt(value);
            }
        }

        public SecureString GetPassword() 
        {
            var ss = new SecureString();
            foreach (var c in password)
                ss.AppendChar(c);

            return ss;
        }

        public bool IsVaild()
        {
            return !((new string[] { EmailServer, UserName, password }).All(string.IsNullOrEmpty));
        }


    }
}


