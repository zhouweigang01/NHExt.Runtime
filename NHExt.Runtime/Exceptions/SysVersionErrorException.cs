using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Exceptions
{
    class SysVersionErrorException : Exception
    {
        public SysVersionErrorException(string entityName, long id)
            : base("当前数据已经被其他用户修改，请刷新重试")
        {

            NHExt.Runtime.Logger.LoggerHelper.Error(this, NHExt.Runtime.Logger.LoggerInstance.BizLogger);
            NHExt.Runtime.Logger.LoggerHelper.Error("实体类型“" + entityName + "”，实体ID:" + id, NHExt.Runtime.Logger.LoggerInstance.BizLogger);
        }
    }
}
