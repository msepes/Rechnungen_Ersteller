using DATA;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace Rechnungen.Windows
{
    public partial class Zusatztext : Window, IComponentConnector
    {
        private Func<ZusatzText> NewZusatzText;
        private Func<long, ZusatzText> GetZusatzText;
        private Func<IEnumerable<ZusatzText>> GetZusatzTexte;
        private Action<ZusatzText> DeleteZusatzText;
        private Action Save;

        public void Register(
          Func<ZusatzText> NewZusatzText,
          Func<long, ZusatzText> GetZusatzText,
          Func<IEnumerable<ZusatzText>> GetZusatzTexte,
          Action<ZusatzText> DeleteZusatzText,
          Action Save)
        {
            this.NewZusatzText = NewZusatzText;
            this.GetZusatzText = GetZusatzText;
            this.GetZusatzTexte = GetZusatzTexte;
            this.DeleteZusatzText = DeleteZusatzText;
            this.Save = Save;
            fillList();
        }

        public Zusatztext() => InitializeComponent();

        private void bind(ZusatzText ZusatzText)
        {
            Unbind();
            Binder.BindControl("Beschreibung", ZusatzText, txtBeschreibung);
            Binder.BindControl("position1", ZusatzText,txtpos1);
            Binder.BindControl("position2", ZusatzText,txtpos2);
            Binder.BindControl("position3", ZusatzText,txtpos3);
            Binder.BindControl("position4", ZusatzText,txtpos4);
            Binder.BindControl("position5", ZusatzText,txtpos5);
        }

        private void Unbind()
        {
            Binder.Clear(txtBeschreibung);
            Binder.Clear(txtpos1);
            Binder.Clear(txtpos2);
            Binder.Clear(txtpos3);
            Binder.Clear(txtpos4);
            Binder.Clear(txtpos5);
        }

        private void fillList()
        {
            Unbind();
            lstBox.Items.Clear();
            lstBox.SelectionMode = SelectionMode.Single;
            lstBox.DisplayMemberPath = "Bezeichnung";
            foreach (object newItem in this.GetZusatzTexte().Select(r => new ListBoxItem(r.ToString(), r.ID)))
                lstBox.Items.Add(newItem);
            if (lstBox.Items.Count < 1)
                return;
            lstBox.SelectedItem = this.lstBox.Items[0];
        }

        private long? GetSelectedID()
        {
            return !(lstBox.SelectedItem is ListBoxItem selectedItem) ? new long?() : new long?(selectedItem.EntityID);
        }

        private void lstBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Unbind();
            long? selectedId = GetSelectedID();
            if (!selectedId.HasValue)
                return;
            bind(GetZusatzText(selectedId.Value));
        }

        private void New_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ZusatzText zusatzText = NewZusatzText();
                lstBox.SelectedItem = lstBox.Items[lstBox.Items.Add(new ListBoxItem(zusatzText.ToString(), zusatzText.ID))];
            }
            catch (Exception ex)
            {
                ExceptionTools.HandleException(ex, this.GetType());
            }
        }

        private void Delete_Click(object sender, RoutedEventArgs e)
        {
            long? selectedId = GetSelectedID();
            if (!selectedId.HasValue)
                return;
            try
            {
                DeleteZusatzText(GetZusatzText(selectedId.Value));
                lstBox.Items.Remove(lstBox.SelectedItem);
            }
            catch (Exception ex)
            {
                ExceptionTools.HandleException(ex, this.GetType());
            }
        }

        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                Save();
                Close();
            }
            catch (Exception ex)
            {
                ExceptionTools.HandleException(ex, this.GetType());
            }
        }
    }
}
