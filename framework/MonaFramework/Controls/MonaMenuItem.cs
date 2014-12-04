using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using MonaFramework.Interfaces;
using System.Windows.Controls;

namespace MonaFramework.Controls
{
    public class MonaMenuItem : MenuItem, IAliasAnswerer
    {
        private IMonaComponent parent = null;

        public void setParent(IMonaComponent parent)
        {
            this.parent = parent;
        }

        public IMonaComponent getParent()
        {
            return parent;
        }

        public string getAlias()
        {
            if (this.HasHeader)
            {
                return this.Header.ToString();
            }

            return null;
        }

        public void setAlias(string alias)
        {

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
