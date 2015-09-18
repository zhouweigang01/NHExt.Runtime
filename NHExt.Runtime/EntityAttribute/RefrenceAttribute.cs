using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.EntityAttribute
{
    /// <summary>
    /// 引用普通实体
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class RefrenceAttribute : Attribute
    {
        private NHExt.Runtime.Enums.RefrenceTypeEnum _refType;
        /// <summary>
        /// 引用类型枚举
        /// </summary>
        public NHExt.Runtime.Enums.RefrenceTypeEnum RefType
        {
            get { return _refType; }
            set { _refType = value; }
        }
        private string _targetEntity;
        /// <summary>
        /// 引用类型全名称
        /// </summary>
        public string TargetEntity
        {
            get { return _targetEntity; }
            set { _targetEntity = value; }
        }
    }
     
}
