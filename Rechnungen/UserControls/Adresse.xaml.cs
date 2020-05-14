using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static Rechnungen.Binder;

namespace Rechnungen.Windows
{
    /// <summary>
    /// Interaction logic for Adresse.xaml
    /// </summary>
    public partial class Adresse : UserControl
    {
        public Adresse()
        {
            InitializeComponent();
        }

        public void Bind(DATA.Adresse Adress) 
        {
            BindControl(nameof(Adress.Ort), Adress, txtOrt);
            BindControl(nameof(Adress.PLZ), Adress, txtPLZ);
            BindControl(nameof(Adress.Strasse), Adress, txtStrasse);
            BindControl(nameof(Adress.HasuNr), Adress, txtHausNr);
        }

        public void unBind() 
        {
            Clear(txtOrt);
            Clear(txtPLZ);
            Clear(txtStrasse);
            Clear(txtHausNr);
        }
    }
}
