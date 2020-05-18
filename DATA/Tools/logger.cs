using System;
using System.Collections.Generic;
using System.Text;

namespace Rechnungen
{
    public static class logger
    {
        public static string Exception(Exception ex, Type typ)
        {
            log4net.ILog log = log4net.LogManager.GetLogger(typ);
            StringBuilder strb = new StringBuilder();

            while (ex != null)
            {
                log.Error($"{ex.Message}{Environment.NewLine}{ex.StackTrace}", ex);
                strb.AppendLine(ex.Message);
                ex = ex.InnerException;
            }

            return strb.ToString();
        }
    }
}
