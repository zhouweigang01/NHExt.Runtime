using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Code.Builder.Build.TemplateDTO
{
    [Serializable]
    public class Enum
    {

        public Enum(Model.EnumEntity entity, bool isDTO)
        {

            this.Name = entity.Name;
            if (isDTO)
            {
                this.Code = entity.Code + Net.Code.Builder.Build.Model.DTOEntity.AttrEndTag;
                this.Namespace = entity.Proj.Namespace + "." + Net.Code.Builder.Build.Model.DTOEntity.AssemblyEndTag;
            }
            else
            {
                this.Code = entity.Code;
                this.Namespace = entity.Proj.Namespace;
            }
            this.Type = this.Namespace + "." + this.Code;
            this.ColumnList = new List<EnumColumn>();
            foreach (Model.EnumColumn col in entity.ColumnList)
            {
                this.ColumnList.Add(new EnumColumn() { Code = col.Code, Name = col.Name, Value = col.EnumValue });
            }

        }
        public string Namespace { get; set; }
        public string Code { get; set; }
        public string Name { get; set; }
        public string Type { get; set; }
        public List<EnumColumn> ColumnList { get; set; }
    }
    [Serializable]
    public class EnumColumn
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public int Value { get; set; }
    }
}
