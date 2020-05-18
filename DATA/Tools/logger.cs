using System;
using System.Text;
using log4net;

namespace Rechnungen
{
    public static class logger
    {
        public static string Exception(Exception ex, Type typ)
        {
            ILog log = LogManager.GetLogger(typ);
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
