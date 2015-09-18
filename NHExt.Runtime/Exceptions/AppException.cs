using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Exceptions
{
    public class AppException : Exception
    {
        public AppException(string errorMsg) : base(errorMsg) { }
    }
}
