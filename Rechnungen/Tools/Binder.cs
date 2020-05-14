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

        public static void Clear( TextBox ctrl)
        {
            Clear(ctrl, TextBox.TextProperty);
        }

        private static void BindControl(string Property, object Source,Control ctrl, DependencyProperty DependencyProperty)
        {
            var Binding = new Binding(Property)
            {
                Source = Source
            };

            ctrl.SetBinding(DependencyProperty, Binding);
        }

        private static void Clear(DependencyObject ctrl,DependencyProperty DependencyProperty) 
        {
            BindingOperations.ClearBinding(ctrl, DependencyProperty);
        }
    }
}
