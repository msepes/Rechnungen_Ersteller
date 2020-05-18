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

            var RechnungenCount = Client.Rechnungen.Count;
            if (RechnungenCount > 0) 
                throw new Exception($"Der Kunde hat {RechnungenCount} Rechnung, daher kann nicht gelöscht werden");

            var AngeboteCount = Client.Angebote.Count;
            if (AngeboteCount > 0)
                throw new Exception($"Der Kunde hat {AngeboteCount} Angebote, daher kann nicht gelöscht werden");

            KundenSet.Remove(Client);
        }

        private static IEnumerable<Kunde> GetAll(DbSet<Kunde> KundenSet) => KundenSet.Include(k => k.addresse).Include(k => k.Rechnungen).Include(k => k.Angebote).ToList().Concat(Inserted);

        public static Kunde GetKunde(DbSet<Kunde> KundenSet, long ID) => GetKunden(KundenSet).FirstOrDefault(k => k.ID == ID);

        public static IEnumerable<Kunde> GetKunden(DbSet<Kunde> KundenSet) => GetAll(KundenSet);

        public static void AcceptChanges() => Inserted.Clear();
    }
}
