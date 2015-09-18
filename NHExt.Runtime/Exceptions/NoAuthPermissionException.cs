using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Exceptions
{
    /// <summary>
    /// 没有访问权限
    /// </summary>
   public class NoAuthPermissionException : Exception
    {
        public NoAuthPermissionException() : base("当前用户没有该操作的权限") { 
        
        }
    }
}
