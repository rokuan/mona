using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonaFramework.Interfaces
{
    public interface IAliasesContainer : IMonaComponent
    {
        //void addComponentForAlias(string aliass, AliasAnswerer comp);

        //void removeComponentForAlias(string alias, AliasAnswerer comp);

        ICollection<IAliasAnswerer> getComponentsByAlias(string alias);
        ICollection<IAliasAnswerer> getAllComponents();

        void notifyAliasAdd(string alias, IAliasAnswerer comp);
        void notifyAliasRemove(string alias, IAliasAnswerer comp);
    }
}
