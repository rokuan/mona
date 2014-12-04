using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Linq.Expressions;
using System.Windows;
using System.Reflection;
using System.Windows.Data;

namespace MonaFramework.Collections
{
    public static class ObservableEx
    {
        /*public static IObservable<TResult> FromDependencyPropertyChanged<TType, TResult>(
            TType target, Expression<Func<TType, TResult>> property)
            where TType : DependencyObject
        {
            if (target == null)
            {
                throw new ArgumentNullException("target", "target cannot be null");
            }

            var body = property.Body as MemberExpression;

            if (body == null)
                throw new ArgumentException("Invalid property expression", "property");

            string propertyName = body.Member.Name;

            if (body.Member is FieldInfo)
            {
                var fieldInfo = body.Member as FieldInfo;
                if (fieldInfo.FieldType == typeof(DependencyProperty))
                {
                    propertyName = propertyName.Remove(propertyName.LastIndexOf("Property"));
                }
            }

            var getter = property.Compile();

            return Observable.Create<TResult>(observer =>
            {
                var handler = new PropertyChangedCallback((dpo, args) =>
                    observer.OnNext((TResult)getter(target)));
                RegisterForPropertyChangedNotification(propertyName, target, handler);
                return new Action(() => { });
            });

        }

        private static void RegisterForPropertyChangedNotification(string propertyName,
            DependencyObject element, PropertyChangedCallback callback)
        {
            Binding b = new Binding(propertyName) { Source = element };
            var prop = System.Windows.DependencyProperty.RegisterAttached(
                "__ListenAttached" + propertyName,
                typeof(object),
                element.GetType(),
                new System.Windows.PropertyMetadata(callback));

            BindingOperations.SetBinding(element, prop, b);
        }*/
    }
}
