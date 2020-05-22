using DATA;
using System;
using System.Windows;
using static Rechnungen.Binder;


namespace Rechnungen.Forms
{
    /// <summary>
    /// Interaction logic for OwnCompany.xaml
    /// </summary>
    public partial class OwnCompany : Window
    {
        private Benutzer Benutzer;
        private Action Save;

        public OwnCompany()
        {
            InitializeComponent();
        }

        public void Register(Func<Benutzer> GetBenutzer, Action Save) 
        {
            this.Save = Save;
            Benutzer = GetBenutzer();
            Bind(Benutzer);
        }

        private void Bind(Benutzer Benutzer) 
        {
            BindControl(nameof(Benutzer.Name), Benutzer, txtName);
            BindControl(nameof(Benutzer.Vorname), Benutzer, txtVorname);
            BindControl(nameof(Benutzer.FirmaName), Benutzer, txtFirma);
            BindControl(nameof(Benutzer.Email), Benutzer, txtEmail);
            BindControl(nameof(Benutzer.Telefone), Benutzer, txtTelefone);
            BindControl(nameof(Benutzer.SteuerID), Benutzer, txtSteuerNr);
            BindControl(nameof(Benutzer.BankName), Benutzer, txtBank);
            BindControl(nameof(Benutzer.IBAN), Benutzer, txtIBAN);
            BindControl(nameof(Benutzer.BIC), Benutzer, txtBIC);
            addAddress.Bind(Benutzer.addresse);
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Save();
                this.Close();
            }
            catch (Exception ex)
            {
                ExceptionTools.HandleException(ex, this.GetType());
            }
        }
    }
}
