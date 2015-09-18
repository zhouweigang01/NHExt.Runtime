using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Code.Builder.Build.TemplateDTO
{
    [Serializable]
    public class BEEntity
    {
        public BEEntity(Model.BEEntity entity)
        {
            this.Namespace = entity.Proj.Namespace;
            this.Guid = entity.Guid;
            this.Code = entity.Code;
            this.Name = entity.Name;
            this.InhertClass = entity.InhertName;
            this.EntityName = this.Namespace + "." + this.Code;
            this.Range = entity.GetViewRangeStr();
            this.OrgFilter = entity.OrgFilter ? "true" : "false";
            this.Table = entity.TableName;
            this.ColumnList = new List<BEColumn>();
            foreach (Model.BEColumn col in entity.InhertColumnList)
            {
                if (col.DataType == Base.DataTypeEnum.RefreceType && !string.IsNullOrEmpty(col.RefColGuid))
                {
                    this.PEntityCol = col.Code;
                    this.PEntityType = col.RefEntityCode;
                }
            }
            foreach (Model.BEColumn col in entity.ColumnList)
            {
                BEColumn c = new BEColumn(col);
                this.ColumnList.Add(c);
                if (col.DataType == Base.DataTypeEnum.RefreceType && !string.IsNullOrEmpty(col.RefColGuid))
                {
                    this.PEntityCol = col.Code;
                    this.PEntityType = col.RefEntityCode;
                }
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
        /// 实体名称
        /// </summary>
        public string EntityName { get; set; }
        /// <summary>
        /// 可见范围
        /// </summary>
        public string Range { get; set; }
        /// <summary>
        /// 是否组织过滤
        /// </summary>
        public string OrgFilter { get; set; }
        /// <summary>
        /// 数据表名
        /// </summary>
        public string Table { get; set; }

        public string InhertClass { get; set; }

        public string PEntityCol { get; set; }
        public string PEntityType { get; set; }

        public List<BEColumn> ColumnList { get; set; }

    }

    [Serializable]
    public class BEColumn
    {
        public BEColumn(Model.BEColumn col)
        {
            this.Code = col.Code;
            this.Name = col.Name;
            this.Type = col.TypeString;
            this.NullMark = "";
            this.IsBuildEntityKey = false;
            if (col.DataType == Base.DataTypeEnum.CompositionType)
            {
                this.RefType = "NHExt.Runtime.Enums.RefrenceTypeEnum.Composition";
                this.EntityRefrence = "false";
                this.NullMark = "";
                this.IsBuildEntityKey = false;
                this.Target = col.BEProj.Namespace + "." + col.RefEntityCode;
                this.DTOType = col.BEProj.Namespace + ".Deploy." + col.RefEntityCode + "DTO";
                this.DTONullMark = "";
            }
            else if (col.DataType == Base.DataTypeEnum.RefreceType)
            {
                this.EntityRefrence = "true";
                this.NullMark = "";

                if (col.IsEnum)
                {
                    this.RefType = "NHExt.Runtime.Enums.RefrenceTypeEnum.Enum";
                }
                else
                {
                    if (string.IsNullOrEmpty(col.RefColGuid))
                    {
                        this.RefType = "NHExt.Runtime.Enums.RefrenceTypeEnum.Entity";
                    }
                    else
                    {
                        this.RefType = "NHExt.Runtime.Enums.RefrenceTypeEnum.PEntity";
                    }
                    this.IsBuildEntityKey = true;
                }
                this.Target = col.BEProj.Namespace + "." + col.RefEntityCode;
                this.DTOType = col.BEProj.Namespace + ".Deploy." + col.RefEntityCode + "DTO";
            }
            else
            {
                this.RefType = "NHExt.Runtime.Enums.RefrenceTypeEnum.Common";
                this.EntityRefrence = "false";
                if (col.IsNull)
                {
                    if (col.TypeString.ToLower() != "string")
                    {
                        this.NullMark = "?";
                    }
                }
                this.DTOType = string.Empty;
            }
            this.IsViewer = col.IsViewer ? "true" : "false";
            this.IsBizKey = col.IsBizKey ? "true" : "false";
            this.IsNull = col.IsNull ? "true" : "false";
        }

        /// <summary>
        /// 字段名称
        /// </summary>
        public string Code { get; set; }

        /// <summary>
        /// 字段描述
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 字段类型
        /// </summary>
        public string Type { get; set; }

        /// <summary>
        /// 是否引用类型
        /// </summary>
        public string EntityRefrence { get; set; }
        /// <summary>
        /// 是否可浏览
        /// </summary>
        public string IsViewer { get; set; }
        /// <summary>
        /// 是否业务主键
        /// </summary>
        public string IsBizKey { get; set; }
        /// <summary>
        /// 是否可空
        /// </summary>
        public string IsNull { get; set; }
        /// <summary>
        /// 是否为空标记
        /// </summary>
        public string NullMark { get; set; }
        /// <summary>
        /// 实体引用类型(实体、枚举、聚合父实体)
        /// </summary>
        public string RefType { get; set; }
        /// <summary>
        /// 字段的引用类型
        /// </summary>
        public string Target { get; set; }
        /// <summary>
        /// 是否生成实体KEY
        /// </summary>
        public bool IsBuildEntityKey { get; set; }
        /// <summary>
        /// 关联DTO类型
        /// </summary>
        public string DTOType { get; set; }

        public string DTONullMark { get; set; }
    }
}
