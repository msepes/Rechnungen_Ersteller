using DATA;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rechnungen
{
    public static class BenutzerTools
    {
        public static Benutzer GetBenutzer(DbSet<Benutzer> BenutzerSet, DbSet<Adresse> AdresseSet)
        {
            var Benutzer = BenutzerSet.Include(be => be.addresse).FirstOrDefault();

            if (Benutzer != null)
            {
                return Benutzer;
            }

            Benutzer = new Benutzer();
            Benutzer.addresse = new Adresse();
            AdresseSet.Add(Benutzer.addresse);
            BenutzerSet.Add(Benutzer);
            return Benutzer;
        }
    }
}
