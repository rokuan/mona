using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonaFramework.Interfaces
{
    public interface IAliasReferencer
    {
        IAliasAnswerer getReference();
        void setReference(IAliasAnswerer answerer);
    }
}
