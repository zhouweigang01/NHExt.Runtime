using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Exceptions
{
    public class RuntimeException : Exception
    {
        public RuntimeException(string errorMsg) : base(errorMsg) { }
    }
}
