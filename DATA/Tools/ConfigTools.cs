using DATA;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Rechnungen
{
    public static class ConfigTools
    {
        public static EmailConf GetConfig(DbSet<EmailConf> ConfigSet, ConfTyp typ)
        {
            var Config = ConfigSet.FirstOrDefault(c => c.typ == typ);

            if (Config != null)
                return Config;

            Config = new EmailConf();
            Config.typ = typ;
            ConfigSet.Add(Config);
            return Config;
        }

        public static bool Exsits(DbSet<EmailConf> BenutzerSet) => BenutzerSet.FirstOrDefault() != null;
    }
}
