using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Model
{
    public interface IDTO
    {
        void SetValue(object obj, string memberName);
    }
}
