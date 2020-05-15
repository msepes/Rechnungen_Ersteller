using DATA;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Rechnungen
{
    public static class RechnungTools
    {
        private static List<Rechnung> Inserted = new List<Rechnung>();

        private static IEnumerable<Rechnung> GetAll(DbSet<Rechnung> RechnungSet)
        {
            return RechnungSet.Include(k => k.Positions).Include(k => k.Rabbat).ToList().Concat(Inserted);
        }

        public static Rechnung NewRechnung(DbSet<Rechnung> RechnungSet, Kunde client)
        {
            var Rechnung = new Rechnung();
            var Rechnungen = GetRechnungen(RechnungSet);

            Rechnung.ID = Rechnungen.Count() > 0 ? Rechnungen.Max(k => k.ID) + 1 : 1;
            Rechnung.Nr = Rechnungen.Count() > 0 ? Rechnungen.Max(k => k.Nr) + 1 : 1;
            Rechnung.Datum = DateTime.Now;
            Rechnung.Umsatzsteuer = 19;
            Rechnung.LeistungsDatum = DateTime.Now;
            Rechnung.Positions = new ObservableCollection<Rechnungsposition>();
            Rechnung.Kunde = client;
            RechnungSet.Add(Rechnung);
            Inserted.Add(Rechnung);

            return Rechnung;
        }


        public static void DeleteRechnung(DbSet<Rechnung> RechnungSet, Rechnung Bill)
        {
            if (RechnungSet.Find(Bill.ID) == null)
                throw new Exception($"DeleteRechnung -> Rechnung mit dem ID '{Bill.ID}' wurde nicht gefunden");

            RechnungSet.Remove(Bill);
        }

        public static Rechnung GetRechnung(DbSet<Rechnung> RechnungSet, long ID)
        {
            return GetRechnungen(RechnungSet).FirstOrDefault(k => k.ID == ID);
        }

        public static IEnumerable<Rechnung> GetRechnungen(DbSet<Rechnung> RechnungSet)
        {
            return GetAll(RechnungSet);
        }

        public static IEnumerable<Rechnung> GetRechnungen(DbSet<Rechnung> RechnungSet, Kunde client)
        {
            return GetAll(RechnungSet).Where(r => r.Kunde.ID == client.ID);
        }

        public static void AcceptChanges()
        {
            Inserted.Clear();
        }
    }
}
