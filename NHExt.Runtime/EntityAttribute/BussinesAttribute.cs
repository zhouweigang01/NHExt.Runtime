using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHExt.Runtime.Enums;

namespace NHExt.Runtime.EntityAttribute
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
    public class BussinesAttribute : Attribute
    {
        /// <summary>
        /// 实体名称
        /// </summary>
        public string EntityName { get; set; }
        /// <summary>
        /// 实体关联表
        /// </summary>
        public string Table { get; set; }
        /// <summary>
        /// 可见范围
        /// </summary>
        public ViewRangeEnum Range { get; set; }
        /// <summary>
        /// 是否根组织过滤
        /// </summary>
        public bool OrgFilter { get; set; }
    }
}
