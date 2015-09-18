using Net.Code.Builder.Build.Tpl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Code.Builder.Build.TemplateDTO
{
    [Serializable]
    public class BEEntityDTO
    {
        public BEEntityDTO(Model.BEEntity entity)
        {
            this.Namespace = entity.Proj.Namespace + "." + Net.Code.Builder.Build.Model.DTOEntity.AssemblyEndTag;
            this.Code = entity.Code + Net.Code.Builder.Build.Model.DTOEntity.AttrEndTag;
            this.Name = entity.Name + Net.Code.Builder.Build.Model.DTOEntity.AttrEndTag;

            if (entity.InhertName == Attributes.BaseEntity)
            {
                this.InhertClass = Attributes.BaseDTO;
            }
            else
            {
                this.InhertClass = string.Empty;
                //如果实体继承一个类的话我们生成的dto也需要继承这个类生成的dto
                string[] namespaceArray = entity.InhertName.Split('.');
                int length = namespaceArray.Length;
                for (int i = 0; i < length - 2; i++)
                {
                    this.InhertClass += namespaceArray[i] + ".";
                }
                this.InhertClass += namespaceArray[length - 2] + "." + Net.Code.Builder.Build.Model.DTOEntity.AssemblyEndTag + ".";
                this.InhertClass += namespaceArray[length - 1] + Net.Code.Builder.Build.Model.DTOEntity.AttrEndTag;
            }
            this.Type = this.Namespace + "." + this.Code;
            this.ColumnList = new List<BEColumnDTO>();
            foreach (Model.BEColumn col in entity.ColumnList)
            {
                TemplateDTO.BEColumnDTO c = new TemplateDTO.BEColumnDTO(col);
                this.ColumnList.Add(c);
            }
        }
        public BEEntityDTO(Model.DTOEntity entity)
        {
            this.Namespace = entity.Proj.Namespace + "." + Net.Code.Builder.Build.Model.DTOEntity.AssemblyEndTag;
            this.Code = entity.Code;
            this.Name = entity.Name;

            this.InhertClass = entity.InhertName;
            this.Type = this.Namespace + "." + this.Code;
            this.ColumnList = new List<BEColumnDTO>();
            foreach (Model.DTOColumn col in entity.ColumnList)
            {
                TemplateDTO.BEColumnDTO c = new TemplateDTO.BEColumnDTO(col);
                this.ColumnList.Add(c);
            }
        }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public string InhertClass { get; set; }
        public string Namespace { get; set; }
        public List<TemplateDTO.BEColumnDTO> ColumnList { get; set; }
    }
    [Serializable]
    public class BEColumnDTO
    {
        public BEColumnDTO(Model.BEColumn col)
        {
            this.Code = col.Code;
            this.Name = col.Name;
            this.Type = string.Empty;
            this.NullMark = string.Empty;
            if (col.DataType == Base.DataTypeEnum.CompositionType)
            {
                this.RefType = "NHExt.Runtime.Enums.RefrenceTypeEnum.Composition";
                this.EntityRefrence = "false";
                this.IsViewer = "false";
                this.Type = col.BEProj.Namespace + "." + Net.Code.Builder.Build.Model.DTOEntity.AssemblyEndTag + "." + col.RefEntityCode + Net.Code.Builder.Build.Model.DTOEntity.AttrEndTag;
            }
            else if (col.DataType == Base.DataTypeEnum.RefreceType)
            {
                this.EntityRefrence = "true";
                if (col.IsEnum)
                {
                    this.RefType = "NHExt.Runtime.Enums.RefrenceTypeEnum.Common";
                    this.Type = "int";
                }
                else
                {
                    if (string.IsNullOrEmpty(col.RefColGuid))
                    {
                        this.RefType = "NHExt.Runtime.Enums.RefrenceTypeEnum.Common";
                    }
                    else
                    {
                        this.RefType = "NHExt.Runtime.Enums.RefrenceTypeEnum.Common";
                    }
                    this.Type = "long";
                }
                if (col.IsNull)
                {
                    this.NullMark = "?";
                }
            }
            else
            {
                this.RefType = "NHExt.Runtime.Enums.RefrenceTypeEnum.Common";
                this.EntityRefrence = "false";
                this.Type = col.TypeString;
                if (col.IsNull)
                {
                    if (col.TypeString.ToLower() != "string")
                    {
                        this.NullMark = "?";
                    }
                }
            }
            this.IsViewer = col.IsViewer ? "true" : "false";
        }
        public BEColumnDTO(Model.DTOColumn col)
        {
            this.Code = col.Code;
            this.Name = col.Name;
            this.Type = col.TypeString;
            this.NullMark = string.Empty;
            if (col.DataType == Base.DataTypeEnum.CompositionType)
            {
                this.RefType = "NHExt.Runtime.Enums.RefrenceTypeEnum.Composition";
                this.EntityRefrence = "false";
                this.IsViewer = "false";
            }
            else if (col.DataType == Base.DataTypeEnum.RefreceType)
            {
                this.EntityRefrence = "true";
                this.RefType = "NHExt.Runtime.Enums.RefrenceTypeEnum.Entity";
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
            }
            this.IsViewer = col.IsViewer ? "true" : "false";
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
        /// 实体引用类型(实体、枚举、聚合父实体)
        /// </summary>
        public string RefType { get; set; }

        /// <summary>
        /// 是否为空标记
        /// </summary>
        public string NullMark { get; set; }
    }
}
