using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using MonaFramework.Interfaces;
using System.Windows;

namespace MonaFramework.Controls
{
    public class MonaTextBlock : TextBlock, IAliasAnswerer, IAliasReferencer
    {
        private string alias;
        private IAliasAnswerer reference;

        public MonaTextBlock()
        {

        }

        public static readonly DependencyProperty AliasProperty = DependencyProperty.Register(
          "Alias",
          typeof(string),
          typeof(MonaTextBlock),
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
            if (reference != null)
            {
                reference.defaultAction();
            }
        }

        public void focusComponent()
        {
            if (reference != null)
            {
                reference.focusComponent();
            }
        }

        public void clickComponent()
        {
            if (reference != null)
            {
                reference.focusComponent();
            }
        }

        public IAliasAnswerer getReference()
        {
            return reference;
        }

        public void setReference(IAliasAnswerer answerer)
        {
            reference = answerer;
        }
    }
}
