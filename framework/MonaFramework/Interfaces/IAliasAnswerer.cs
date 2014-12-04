using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MonaFramework.Interfaces
{
    public interface IAliasAnswerer : IAliased
    {
        void defaultAction();
        void focusComponent();
        void clickComponent();
    }
}
