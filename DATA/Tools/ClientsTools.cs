using Angeboten;
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

            var maxNr = KundenSet.Count() > 0 ? KundenSet.Max(r => r.Nr):0;
            var maxID = KundenSet.Count() > 0 ? KundenSet.Max(r => r.ID):0;

            var maxInsertedNr = Inserted.Count > 0 ?Inserted.Max(o => o.Nr):0;
            var maxInsertedID = Inserted.Count > 0 ? Inserted.Max(o => o.ID):0;

            maxNr = maxInsertedNr > maxNr ? maxInsertedNr : maxNr;
            maxID = maxInsertedID > maxID ? maxInsertedID : maxID;

            var kunden = GetKunden(KundenSet);
            var Kunde = new Kunde();
            Kunde.FirmaName = $"unbekannt{++maxNr}";
            Kunde.ID = ++maxID;
            Kunde.Nr = maxNr;
            Kunde.addresse = new Adresse();
            Kunde.Rechnungen = new ObservableCollection<Rechnung>();
            Kunde.Angebote = new ObservableCollection<Angebot>();
            AdresseSet.Add(Kunde.addresse);
            KundenSet.Add(Kunde);
            Inserted.Add(Kunde);

            return Kunde;
        }


        public static void DeleteKunde(DbSet<Kunde> KundenSet, DbSet<Adresse> AdresseSet, DbSet<Rechnung> BillSet, DbSet<Angebot> OfferSet, Kunde Client)
        {
            if (KundenSet.Find(Client.ID) == null)
                throw new Exception($"DeleteKunde -> Kunde mit dem ID '{Client.ID}' wurde nicht gefunden");

         
            var RechnungenCount = RechnungTools.Count(BillSet, Client);
            if (RechnungenCount > 0) 
                throw new Exception($"Der Kunde hat {RechnungenCount} Rechnung, daher kann nicht gelöscht werden");

            var AngeboteCount = OfferTools.Count(OfferSet, Client);
            if (AngeboteCount > 0)
                throw new Exception($"Der Kunde hat {AngeboteCount} Angebote, daher kann nicht gelöscht werden");

            KundenSet.Remove(Client);

            if (Client.addresse != null)
                AdresseSet.Remove(Client.addresse);

            var kunde = Inserted.FirstOrDefault(c => c.ID == Client.ID);
            if (kunde != null)
                Inserted.Remove(kunde);

        }

        private static IEnumerable<Kunde> GetAll(DbSet<Kunde> KundenSet) => KundenSet.OrderBy(k => k.Nr).ToList().Concat(Inserted);

        public static Kunde GetKunde(DbSet<Kunde> KundenSet, long ID)
        {
            var o = Inserted.FirstOrDefault(k => k.ID == ID);
            if (o != null)
                return o;

            return KundenSet.Where(k => k.ID == ID).Include(k => k.addresse).FirstOrDefault();
        }

        public static IEnumerable<Kunde> GetKunden(DbSet<Kunde> KundenSet) => GetAll(KundenSet);

        public static void AcceptChanges() => Inserted.Clear();
    }
}
