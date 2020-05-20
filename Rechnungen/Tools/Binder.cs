using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Rechnungen
{
    public static class Binder
    {
        public static void BindControl(string Property, object Source, TextBox ctrl)
        {
            BindControl(Property, Source, ctrl, TextBox.TextProperty);
        }

        public static void BindControl(string Property, object Source, DatePicker ctrl)
        {
            BindControl(Property, Source, ctrl, DatePicker.SelectedDateProperty);
        }

        public static void BindControl(string Property, object Source, ComboBox ctrl)
        {
            BindControl(Property, Source, ctrl, ComboBox.SelectedValueProperty);
        }

        public static void Clear( TextBox ctrl)
        {
            Clear(ctrl, TextBox.TextProperty);
        }

        public static void Clear(DataGrid ctrl)
        {
            ctrl.ItemsSource = null;
            ctrl.IsEnabled = false;
        }


        public static void Clear(DatePicker ctrl)
        {
            Clear(ctrl, DatePicker.SelectedDateProperty);
        }

        public static void Clear(ComboBox ctrl)
        {
            Clear(ctrl, ComboBox.SelectedValueProperty);
            ctrl.SelectedItem = null;
        }
        private static void BindControl(string Property, object Source,Control ctrl, DependencyProperty DependencyProperty)
        {
            ctrl.IsEnabled = true;
            var Binding = new Binding(Property)
            {
                Source = Source
            };

            ctrl.SetBinding(DependencyProperty, Binding);
        }

        private static void Clear(Control ctrl,DependencyProperty DependencyProperty) 
        {
            ctrl.IsEnabled = false;
            BindingOperations.ClearBinding(ctrl, DependencyProperty);
        }
    }
}
