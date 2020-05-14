using System;
using System.Collections.Generic;
using System.Text;

namespace Rechnungen
{
    public class ListBoxItem
    {
        public ListBoxItem(string Bezeichnung, long EntityID) 
        {
            this.Bezeichnung = Bezeichnung;
            this.EntityID = EntityID;
        }

        public string Bezeichnung { get;  set; }
        public long EntityID { get; private set; }

        public override string ToString()
        {
            return Bezeichnung;
        }
    }
}
