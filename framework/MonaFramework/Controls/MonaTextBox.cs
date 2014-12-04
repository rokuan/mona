using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using MonaFramework.Interfaces;
using System.Windows;

namespace MonaFramework.Controls
{
    public class MonaTextBox : TextBox, IAliasAnswerer
    {
        private string alias;

        public static readonly DependencyProperty AliasProperty = DependencyProperty.Register(
            "Alias",
            typeof(string),
            typeof(MonaTextBox),
              new FrameworkPropertyMetadata(
              "",
              FrameworkPropertyMetadataOptions.None,
              new PropertyChangedCallback(aliasChanged),
              new CoerceValueCallback(aliasCoerceValue)
              ),
          new ValidateValueCallback(validateAlias));

        public static bool validateAlias(object o)
        {
            return (o is string);
        }

        public static void aliasChanged(DependencyObject o, DependencyPropertyChangedEventArgs args)
        {
            try
            {
                FrameworkElement element = (FrameworkElement)o;
                IAliasesContainer parent = (IAliasesContainer)element.Parent;
                IAliasAnswerer thisAnswerer = (IAliasAnswerer)o;

                parent.notifyAliasRemove((string)args.OldValue, thisAnswerer);
                parent.notifyAliasAdd((string)args.NewValue, thisAnswerer);
            }
            catch (Exception ex)
            {

            }
        }

        public static object aliasCoerceValue(DependencyObject o, object value)
        {
            return value;
        }

        public static void SetAlias(UIElement element, string value)
        {
            element.SetValue(AliasProperty, value);
            try
            {
                ((IAliasAnswerer)element).setAlias(value);
            }
            catch (Exception e)
            {

            }
        }

        public static string GetAlias(UIElement element)
        {
            return (string)element.GetValue(AliasProperty);
        }

        /*protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            if (e.Property.Name == "Alias")
            {
                try
                {
                    if (e.OldValue != null)
                    {
                        ((IAliasesContainer)Parent).notifyAliasRemove((string)e.OldValue, this);
                    }
                    if (e.NewValue != null)
                    {
                        ((IAliasesContainer)Parent).notifyAliasAdd((string)e.NewValue, this);
                    }
                }
                catch (Exception ex)
                {

                }
            }

            base.OnPropertyChanged(e);
        }*/

        public string getAlias()
        {
            return alias;
        }

        public void setAlias(string alias)
        {
            this.alias = alias;
        }

        public void defaultAction()
        {
            focusComponent();
        }

        public void focusComponent()
        {
            this.Focus();
        }

        public void clickComponent()
        {
            focusComponent();
        }
    }
}
