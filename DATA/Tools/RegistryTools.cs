using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DATA
{
    public static class RegistryTools
    {
        private const int DefaultLimit = 50;
        private const string REG_KEY = "Chalibo_Config";
        private const string REG_KEY_Limit = "Chalibo_Limit";

        public static long GetLimit()
        {

            try
            {
                var Regkey = Registry.CurrentUser.OpenSubKey(REG_KEY);

                if (Regkey == null)
                {
                    Regkey = Registry.CurrentUser.CreateSubKey(REG_KEY);
                    Regkey.SetValue(REG_KEY_Limit, DefaultLimit);
                }

                return Convert.ToInt64(Regkey.GetValue(REG_KEY_Limit, DefaultLimit));
            }
            catch (Exception)
            {
                return DefaultLimit;
            }
        }

        public static void ThrowIFLimitReched(int RechnungCount)
        {
            var lim = GetLimit();
            if (lim > RechnungCount)
                return;

            throw new NotSupportedException("Die Übergrenze von Rechnungenanzahl uwrde erreicht");
        }
    }
}
