using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Code.Builder.Build.TemplateDTO
{
    [Serializable]
    public class BPEntity
    {
        public BPEntity(Model.BPEntity bp)
        {
            this.Namespace = bp.Proj.Namespace;
            this.Guid = bp.Guid;
            this.Code = bp.Code;
            this.Name = bp.Name;
            this.IsEntity = bp.IsEntity;
            this.IsList = bp.IsList;
            if (this.IsList)
            {
                this.ReturnName = "List<" + bp.ReturnName + ">";
                this.ProxyReturnName = "List<" + bp.ReturnProxyName + ">";
            }
            else
            {
                this.ReturnName = bp.ReturnName;
                this.ProxyReturnName = bp.ReturnProxyName;
            }
            if (bp.Trans == 1)
            {
                this.Trans = "NHExt.Runtime.Enums.TransactionSupport.Support";
            }
            else if (bp.Trans == 2)
            {
                this.Trans = "NHExt.Runtime.Enums.TransactionSupport.Required";
            }
            else
            {
                this.Trans = "NHExt.Runtime.Enums.TransactionSupport.RequiredNew";
            }
            this.ColumnList = new List<BPColumn>();
            foreach (Model.BPColumn col in bp.ColumnList)
            {
                BPColumn bpColumn = new BPColumn(col);
                this.ColumnList.Add(bpColumn);
            }
        }
        /// <summary>
        /// 命名空间
        /// </summary>
        public string Namespace { get; set; }
        /// <summary>
        /// 实体GUID
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 实体code
        /// </summary>
        public string Code { get; set; }
        /// <summary>
        /// 实体描述
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 实际返回值名称
        /// </summary>
        public string ReturnName { get; set; }

        /// <summary>
        /// 代理返回值字符串
        /// </summary>
        public string ProxyReturnName { get; set; }

        /// <summary>
        /// 当前返回值是否实体
        /// </summary>
        public bool IsEntity { get; set; }

        /// <summary>
        /// 返回值类型是否列表
        /// </summary>
        public bool IsList { get; set; }
        /// <summary>
        /// 事务类别
        /// </summary>
        public string Trans { get; set; }

        public List<BPColumn> ColumnList { get; set; }
    }

    [Serializable]
    public class BPColumn
    {
        public BPColumn(Model.BPColumn col)
        {
            this.Code = col.Code;
            this.Name = col.Name;
            this.TypeString = col.TypeString;
            if (col.IsNull)
            {
                if (string.IsNullOrEmpty(col.RefGuid))
                {
                    if (col.TypeString.ToLower() != "string")
                    {
                        this.TypeString = col.TypeString + " ? ";
                    }
                }
            }

            this.RefType = this.TypeString;
            if (col.DataType == Base.DataTypeEnum.CompositionType)
            {
                this.IsList = true;
                this.TypeString = "List<" + this.TypeString + ">";
            }
            else
            {
                this.TypeString = this.TypeString;
                this.IsList = false;
            }
        }
        public string Code { get; set; }
        public string Name { get; set; }
        public string TypeString { get; set; }
        public string RefType { get; set; }
        /// <summary>
        /// 引用是否为DTO，如果不是DTO需要在后面加上DTO
        /// </summary>
        public bool IsList { get; set; }

    }
}
