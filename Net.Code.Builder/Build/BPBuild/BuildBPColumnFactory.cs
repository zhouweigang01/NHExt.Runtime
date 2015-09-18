using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Base;

namespace Net.Code.Builder.Build.BPBuild
{
    static class BuildBPColumnFactory
    {
        public static BuildBPColumn Create(BPColumn col)
        {
            BuildBPColumn bc = null;
            if (col.DataType == DataTypeEnum.CommonType)
            {
                bc = new BuildBPCommon(col);
            }
            else if (col.DataType == DataTypeEnum.RefreceType)
            {
                bc = new BuildBPRefrence(col);
            }
            else if (col.DataType == DataTypeEnum.CompositionType)
            {
                bc = new BuildBPComposition(col);
            }
            return bc;
        }
    }
}
