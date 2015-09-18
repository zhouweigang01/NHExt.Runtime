using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Enums
{
    [Serializable]
    public enum EntityState
    {
        UnChanged = 0,
        Add = 1,
        Update = 2,
        Delete = 3,
        UnKnow = 4
    }
}
