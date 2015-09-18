using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Exceptions
{
    public class BizException : Exception
    {
        public BizException(string errorMsg) : base(errorMsg) { }
    }
}
