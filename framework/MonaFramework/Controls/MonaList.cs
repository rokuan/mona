using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Collections.Specialized;
using MonaFramework.Interfaces;

namespace MonaFramework.Controls
{
    public class MonaList : ListBox, IAliasesContainer
    {
        private Dictionary<string, List<IAliasAnswerer>> components = new Dictionary<string, List<IAliasAnswerer>>();
        private IMonaComponent parent = null;

        public void setParent(IMonaComponent parent)
        {
            this.parent = parent;
        }

        public IMonaComponent getParent()
        {
            return parent;
        }

        public MonaList()
            : base()
        {
            ((INotifyCollectionChanged)this.Items).CollectionChanged += new NotifyCollectionChangedEventHandler(modificationHandler);
        }

        public ICollection<IAliasAnswerer> getComponentsByAlias(string alias)
        {
            if (components.ContainsKey(alias))
            {
                return components[alias];
            }

            return null;
        }

        public ICollection<IAliasAnswerer> getAllComponents()
        {
            List<IAliasAnswerer> answerers = new List<IAliasAnswerer>();

            foreach (List<IAliasAnswerer> values in components.Values)
            {
                answerers.AddRange(values);
            }

            return answerers;
        }

        public void notifyAliasAdd(string alias, IAliasAnswerer comp)
        {
            try
            {
                ((IAliasesContainer)Parent).notifyAliasAdd(alias, comp);
            }
            catch (Exception e)
            {

            }
        }

        public void notifyAliasRemove(string alias, IAliasAnswerer comp)
        {
            try
            {
                ((IAliasesContainer)Parent).notifyAliasRemove(alias, comp);
            }
            catch (Exception e)
            {

            }
        }

        private void tryAddingComponent(object o)
        {
            if (o is IAliasAnswerer)
            {
                IAliasAnswerer asAns = (IAliasAnswerer)o;
                List<IAliasAnswerer> answerers;

                if (asAns.getAlias() == null)
                {
                    return;
                }

                if (components.ContainsKey(asAns.getAlias()))
                {
                    answerers = components[asAns.getAlias()];
                    answerers.Add(asAns);
                }
                else
                {
                    answerers = new List<IAliasAnswerer>();
                    answerers.Add(asAns);
                    components.Add(asAns.getAlias(), answerers);
                }

                notifyAliasAdd(asAns.getAlias(), asAns);
            }
        }

        private void tryRemovingComponent(object o)
        {
            if (o is IAliasAnswerer)
            {
                IAliasAnswerer asAns = (IAliasAnswerer)o;

                if (asAns.getAlias() == null)
                {
                    return;
                }

                List<IAliasAnswerer> answerers = components[asAns.getAlias()];

                if (answerers != null)
                {
                    answerers.Remove(asAns);
                }

                notifyAliasRemove(asAns.getAlias(), asAns);
            }
        }

        private void modificationHandler(object o, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object comp in args.NewItems)
                    {
                        tryAddingComponent(comp);
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (object comp in args.OldItems)
                    {
                        tryRemovingComponent(comp);
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    //TODO
                    break;

                case NotifyCollectionChangedAction.Reset:
                    //TODO
                    break;
            }
        }
    }
}
