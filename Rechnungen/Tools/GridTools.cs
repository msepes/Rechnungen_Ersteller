using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;

namespace Rechnungen
{
    public static class GridTools
    {
        public static void MakeAcceptDigits(DataGrid Grid)
        {
            Grid.PreviewTextInput += (s, e) =>
            {
                var cel = Grid.CurrentCell;
                if (!cel.IsValid)
                    return;

                var Head = cel.Column?.Header as string;

                if (Head == "Beschreibung")
                    return;

                e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
            };
        }

        public static void SetColumnsSize(DataGrid Grid)
        {
            var ActualWidth = Grid.ActualWidth;
            foreach (var column in Grid.Columns)
            {
                switch ((string)column.Header)
                {
                    case "ID":
                    case "Rechnung":
                    case "Angebot":
                        column.Visibility = Visibility.Hidden;
                        break;
                    case "Beschreibung":
                        if (ActualWidth > 0) column.Width = ActualWidth * 0.7;
                        break;
                    case "Menge":
                    case "Einzeln_Preis":
                        if (ActualWidth > 0) column.Width = ActualWidth * 0.15;
                        break;
                }
            }

        }
    }
}
