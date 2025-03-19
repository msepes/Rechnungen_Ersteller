using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace Rechnungen
{
    public static class TextBoxTools
    {
        public static void MakeAcceptDigits(TextBox txt)
        {

            txt.PreviewTextInput += (s,e) => e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
        }

        public static void MakeAcceptDecimal(TextBox txt)
        {

            txt.PreviewTextInput += (s, e) => e.Handled = new Regex(@"[0 - 9] + (\.[0-9] [0-9]?)?").IsMatch(e.Text);
        }
    }
}
