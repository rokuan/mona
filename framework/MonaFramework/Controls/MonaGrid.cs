using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using MonaFramework.Interfaces;
using System.Collections.Specialized;
using System.Windows;
using MonaFramework.Collections;
using System.Collections.ObjectModel;
using System.Windows.Data;

namespace MonaFramework.Controls
{
    public class MonaGrid : Grid, IAliasesContainer
    {
        private Dictionary<string, List<IAliasAnswerer>> components = new Dictionary<string, List<IAliasAnswerer>>();
        private IMonaComponent parent = null;
        //private ObservableCollection<IAliasAnswerer> collection = null;
        private ObservableUIElementCollection collec = null;

        public void setParent(IMonaComponent parent)
        {
            this.parent = parent;
        }

        public IMonaComponent getParent()
        {
            return parent;
        }

        public MonaGrid()
            : base()
        {

        }

        protected override UIElementCollection CreateUIElementCollection(FrameworkElement logicalParent)
        {
            //ObservableUIElementCollection collec = new ObservableUIElementCollection(this, logicalParent);            
            collec = new ObservableUIElementCollection(this, logicalParent);            
            collec.CollectionChanged += new NotifyCollectionChangedEventHandler(modificationHandler);
            return collec;
            //return new ObservableUIElementCollection(this, logicalParent);
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

        public void notifyAliasAdd(string alias, IAliasAnswerer comp)
        {
            try
            {
                //System.Windows.MessageBox.Show("AliasAdd: " + alias);
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

        private void modificationHandler(object o, NotifyCollectionChangedEventArgs args)
        {
            switch (args.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (object comp in args.NewItems)
                    {
                        tryAddingComponent(comp);
                        /*try
                        {
                            System.Windows.MessageBox.Show("Added: " + ((IAliasAnswerer)comp).getAlias());
                        }
                        catch (Exception e)
                        {

                        }*/
                    }
                    break;

                case NotifyCollectionChangedAction.Remove:
                    foreach (object comp in args.OldItems)
                    {
                        tryRemovingComponent(comp);
                        /*try
                        {
                            System.Windows.MessageBox.Show("Removed: " + ((IAliasAnswerer)comp).getAlias());
                        }
                        catch (Exception e)
                        {

                        }*/
                    }
                    break;

                case NotifyCollectionChangedAction.Replace:
                    foreach (object comp in args.OldItems)
                    {
                        tryRemovingComponent(comp);
                    }

                    foreach (object comp in args.NewItems)
                    {
                        tryAddingComponent(comp);
                        //System.Windows.MessageBox.Show("Added: " + ((IAliasAnswerer)comp).getAlias());
                        /*try
                        {
                            System.Windows.MessageBox.Show("Added: " + ((IAliasAnswerer)comp).getAlias());
                        }
                        catch (Exception e)
                        {

                        }*/
                    }
                    break;

                case NotifyCollectionChangedAction.Reset:
                    //TODO
                    break;
            }
        }

        /*static MonaGrid()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(MonaGrid),
                new FrameworkPropertyMetadata(typeof(MonaGrid)));
        }*/
    }
}
