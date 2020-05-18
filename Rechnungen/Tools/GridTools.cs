using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
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
    }
}
