using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Code.Builder.Build.BEBuild
{
    /// <summary>
    /// 将BEColumn转换成BuildEntityColumn工厂类
    /// </summary>
    static class BuildEntityColumnFactory
    {
        public static BuildEntityColumn Create(Model.BEColumn col)
        {
            BuildEntityColumn bc = null;

            if (col.DataType == Base.DataTypeEnum.CommonType)
            {
                bc = new BuildEntityCommon(col);
            }
            else if (col.DataType == Base.DataTypeEnum.RefreceType)
            {
                if (col.IsEnum)
                {
                    bc = new BuildEntityEnumColumn(col);
                }
                else
                {
                    bc = new BuildEntityRefrence(col);
                }
            }
            else if (col.DataType == Base.DataTypeEnum.CompositionType)
            {
                bc = new BuildEntityComposition(col);
            }

            return bc;
        }
    }
}
