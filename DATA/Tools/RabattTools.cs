using DATA;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rechnungen.Tools
{
    public class RabattTools
    {
        private static List<Rabbat> Inserted = new List<Rabbat>();

        private static IEnumerable<Rabbat> GetAll(DbSet<Rabbat> RabattSet)
        {
            return RabattSet.OrderBy(r => r.Nr).ToList().Concat(Inserted);
        }

        public static Rabbat NewRabatt(DbSet<Rabbat> RabattSet)
        {
            var Rabatt = new Rabbat();
            var maxNr = RabattSet.Count() > 0 ? RabattSet.Max(r => r.Nr) : 0;
            var maxID = RabattSet.Count() > 0 ? RabattSet.Max(r => r.ID) : 0;

            var maxInsertedNr = Inserted.Count > 0 ? Inserted.Max(o => o.Nr) : 0;
            var maxInsertedID = Inserted.Count > 0 ? Inserted.Max(o => o.ID) : 0;

            maxNr = maxInsertedNr > maxNr ? maxInsertedNr : maxNr;
            maxID = maxInsertedID > maxID ? maxInsertedID : maxID;

            Rabatt.Beschreibung = $"unbekannt{++maxNr}";
            var Rabatte = GetRabatte(RabattSet);
            Rabatt.ID = ++maxID;
            Rabatt.Nr = maxNr;
            RabattSet.Add(Rabatt);
            Inserted.Add(Rabatt);

            return Rabatt;
        }


        public static void DeleteRabatt(DbSet<Rabbat> RabattSet, DbSet<Rechnung> RechnungSet, DbSet<Angebot> AngebotSet, Rabbat Rabatt)
        {
            if (RabattSet.Find(Rabatt.ID) == null)
                throw new Exception($"DeleteRabatt -> Rabatt mit dem ID '{Rabatt.ID}' wurde nicht gefunden");

            var RechnungCount = RechnungSet.Count(r => r.Rabbat.ID == Rabatt.ID);
            if (RechnungCount > 0)
                throw new Exception($"Rabatt kann nicht gelöscht werden, ({RechnungCount}) Rechnungen verweisen auf die Rabatt");

            var AngebotCount = AngebotSet.Count(r => r.Rabbat.ID == Rabatt.ID);
            if (AngebotCount > 0)
                throw new Exception($"Rabatt kann nicht gelöscht werden, ({AngebotCount}) Agebote verweisen auf die Rabatt");

            RabattSet.Remove(Rabatt);
        }

        public static Rabbat GetRabatt(DbSet<Rabbat> RabattSet, long ID)
        {
            return GetRabatte(RabattSet).FirstOrDefault(r => r.ID == ID);
        }

        public static IEnumerable<Rabbat> GetRabatte(DbSet<Rabbat> RabattSet)
        {
            return GetAll(RabattSet);
        }

        public static void AcceptChanges()
        {
            Inserted.Clear();
        }

    }
}
