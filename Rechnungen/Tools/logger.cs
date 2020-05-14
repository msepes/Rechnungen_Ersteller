using System;
using System.Collections.Generic;
using System.Text;

namespace Rechnungen
{
    public static class logger
    {
        private static readonly log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static string Exception(Exception ex) 
        {
            var nl = Environment.NewLine;
            var msg = $"{ex.Message}{nl + nl}{ex.StackTrace}";
            log.Error(msg, ex);
            return msg;
        }
    }
}
