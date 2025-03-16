using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#nullable disable
namespace DATA.Tools
{
    public class ZusatzTexteTools
    {
        private static List<ZusatzText> Inserted = new List<ZusatzText>();

        private static IEnumerable<ZusatzText> GetAll(DbSet<ZusatzText> TexteSet)
        {
            return TexteSet.ToList().Concat(Inserted);
        }

        public static ZusatzText NewRabatt(DbSet<ZusatzText> TexteSet)
        {
            ZusatzText entity = new ZusatzText();
            long maxDBId;

            if (TexteSet.Count() <= 0)
                maxDBId = 0L;
            else
                maxDBId = TexteSet.Max(r => r.ID);
            
            long maxTempId = Inserted.Count > 0 ? Inserted.Max(o => o.ID) : 0L;
            long maxId = maxTempId > maxDBId ? maxTempId : maxDBId;

            GetZusatzTexte(TexteSet);
            entity.ID = maxId + 1L;
            TexteSet.Add(entity);
            Inserted.Add(entity);
            return entity;
        }

        public static void DeleteRabatt(
          DbSet<ZusatzText> TexteSet,
          DbSet<Rechnung> RechnungSet,
          ZusatzText ZusatzText)
        {
            if (TexteSet.Find((object)ZusatzText.ID) == null)
                throw new Exception(string.Format("DeleteRabatt -> ZusatzText mit dem ID '{0}' wurde nicht gefunden", ZusatzText.ID));
            int num = RechnungSet.Count(r => r.ZusatzText.ID == ZusatzText.ID);
            if (num > 0)
                throw new Exception(string.Format("Rabatt kann nicht gelöscht werden, ({0}) Rechnungen verweisen auf die Rabatt", num));
            TexteSet.Remove(ZusatzText);
            if (Inserted.FirstOrDefault((c => c.ID == ZusatzText.ID)) == null)
                return;
            Inserted.Remove(ZusatzText);
        }

        public static ZusatzText GetZusatzText(DbSet<ZusatzText> TexteSet, long ID)
        {
            return GetZusatzTexte(TexteSet).FirstOrDefault(r => r.ID == ID);
        }

        public static IEnumerable<ZusatzText> GetZusatzTexte(DbSet<ZusatzText> TexteSet)
        {
            return GetAll(TexteSet);
        }

        public static void AcceptChanges() => Inserted.Clear();
    }
}
