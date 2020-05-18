using DATA;
using System;
using System.Linq;
using System.Windows;
using static Rechnungen.logger;
using static Rechnungen.Binder;
using System.Windows.Controls;
using System.Collections.Generic;

namespace Rechnungen.Windows
{
    /// <summary>
    /// Interaction logic for Rabatt.xaml
    /// </summary>
    public partial class Rabatt : Window
    {
        private Func<Rabbat> NewRabatt;
        private Func<long, Rabbat> GetRabatt;
        private Func<IEnumerable<Rabbat>> GetRabatte;
        private Action<Rabbat> DeleteRabatt;
        private Action Save;

        public void Register(Func<Rabbat> NewRabatt,
                             Func<long, Rabbat> GetRabatt,
                             Func<IEnumerable<Rabbat>> GetRabatte,
                             Action<Rabbat> DeleteRabatt,
                             Action Save)
        {
            this.NewRabatt = NewRabatt;
            this.GetRabatt = GetRabatt;
            this.GetRabatte = GetRabatte;
            this.DeleteRabatt = DeleteRabatt;
            this.Save = Save;
            fillList();
        }

        public Rabatt()
        {
            InitializeComponent();
            TextBoxTools.MakeAcceptDigits(txtNr);
            TextBoxTools.MakeAcceptDigits(txtSatz);
        }

        private void bind(Rabbat rabatt)
        {
            Unbind();
            BindControl(nameof(rabatt.Beschreibung), rabatt, txtBeschreibung);
            BindControl(nameof(rabatt.satz), rabatt, txtSatz);
            BindControl(nameof(rabatt.Nr), rabatt, txtNr);
        }

        private void Unbind()
        {
            Clear(txtBeschreibung);
            Clear(txtSatz);
            Clear(txtNr);
        }

        private void fillList()
        {
            Unbind();
            lstBox.Items.Clear();
            lstBox.SelectionMode = SelectionMode.Single;
            lstBox.DisplayMemberPath = "Bezeichnung";
            var items = GetRabatte().Select(r => new ListBoxItem(r.ToString(), r.ID));
            foreach (var item in items)
                lstBox.Items.Add(item);

            if (lstBox.Items.Count < 1)
                return;

            lstBox.SelectedItem = lstBox.Items[0];

        }

        private long? GetSelectedID()
        {
            var item = lstBox.SelectedItem as ListBoxItem;
            return item?.EntityID;
        }

        private void lstBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Unbind();

            var ID = GetSelectedID();

            btnSave.IsEnabled = ID.HasValue;

            if (!ID.HasValue)
                return;
            var selectedClient = GetRabatt(ID.Value);
            bind(selectedClient);
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            var rabatt = NewRabatt();
            var i = lstBox.Items.Add(new ListBoxItem(rabatt.ToString(), rabatt.ID));
            lstBox.SelectedItem = lstBox.Items[i];
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            var ID = GetSelectedID();

            if (!ID.HasValue)
                return;

            var selectedRabatt = GetRabatt(ID.Value);
            DeleteRabatt(selectedRabatt);
            lstBox.Items.Remove(lstBox.SelectedItem);
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
                ex = ex.InnerException ?? ex;
                var nl = Environment.NewLine;
                Exception(ex, this.GetType());
                var msg = $"Speichern nicht möglich.{nl + nl}{ex.Message}";
                MessageBox.Show(this, msg, "Speichern nicht möglich", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}
