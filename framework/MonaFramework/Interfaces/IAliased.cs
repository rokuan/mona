using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonaFramework.Interfaces
{
    public interface IAliased : IMonaComponent
    {
        string getAlias();
        void setAlias(string alias);
    }
}
