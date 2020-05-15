using DATA;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace Rechnungen
{
    public static class ClientsTools
    {
        private static List<Kunde> Inserted = new List<Kunde>();

        private static IEnumerable<Kunde> GetAll(DbSet<Kunde> KundenSet) 
        {
            return KundenSet.Include(k => k.addresse).Include(k => k.Rechnungen).Include(k => k.Angebote).ToList().Concat(Inserted);
        }

        public static Kunde NewKunde(DbSet<Kunde> KundenSet, DbSet<Adresse> AdresseSet)
        {
            var Kunde = new Kunde();
            Kunde.FirmaName = "unbekannt";
            var kunden = GetKunden(KundenSet);

            Kunde.ID = kunden.Count() > 0 ? kunden.Max(k => k.ID) + 1 : 1;
            Kunde.Nr = kunden.Count() > 0 ? kunden.Max(k => k.Nr) + 1 : 1;
            Kunde.addresse = new Adresse();
            Kunde.Rechnungen = new ObservableCollection<Rechnung>();
            Kunde.Angebote = new ObservableCollection<Angebot>();
            AdresseSet.Add(Kunde.addresse);
            KundenSet.Add(Kunde);
            Inserted.Add(Kunde);

            return Kunde;
        }


        public static void DeleteKunde(DbSet<Kunde> KundenSet, Kunde Client)
        {
            if (KundenSet.Find(Client.ID) == null)
                throw new Exception($"DeleteKunde -> Kunde mit dem ID '{Client.ID}' wurde nicht gefunden");

            KundenSet.Remove(Client);
        }

        public static Kunde GetKunde(DbSet<Kunde> KundenSet, long ID)
        {
            return GetKunden(KundenSet).FirstOrDefault(k => k.ID == ID);
        }

        public static IEnumerable<Kunde> GetKunden(DbSet<Kunde> KundenSet)
        {
            return GetAll(KundenSet);
        }

        public static void AcceptChanges()
        {
            Inserted.Clear();
        }
    }
}
