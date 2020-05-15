using DATA;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

namespace Angeboten
{
    public static class OfferTools
    {
        private static List<Angebot> Inserted = new List<Angebot>();

        private static IEnumerable<Angebot> GetAll(DbSet<Angebot> AngebotSet)
        {
            return AngebotSet.Include(k => k.Positions).Include(k => k.Rabbat).ToList().Concat(Inserted);
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
    }
}
