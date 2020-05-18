using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace Rechnungen
{
    public static class ExceptionTools
    {
        public static void HandleException(Exception ex, Type SourceType)
        {
            logger.Exception(ex, SourceType);
            var nl = Environment.NewLine;
            var msg = $"Die Operation kann nicht durchgeführt werden, folgende Fehler wurde festgestellt:{nl + nl}{ex.Message}{nl}{ex.InnerException?.Message}";
            MessageBox.Show( msg, "Formular Öffnen", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
