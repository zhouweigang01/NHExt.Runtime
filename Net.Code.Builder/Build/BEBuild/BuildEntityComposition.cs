using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.BEBuild
{
    class BuildEntityComposition : BuildEntityColumn
    {

        public BuildEntityComposition(BEColumn col)
            : base(col)
        {
            _templateCode = BECodeTemplate.CompositionTemplate;
        }

        public override string BuildCode()
        {
            _templateCode = base.BuildCode();
            _templateCode = _templateCode.Replace(Attributes.RefrenceEntity, _col.BEProj.Namespace + "." + _col.RefEntityCode);
            return _templateCode;
        }

        public override string BuildMSSQL()
        {
            return string.Empty;
        }

        public override XmlElement BuildCfgXML(XmlDocument xmlDoc)
        {
            XmlElement el = xmlDoc.CreateElement("bag");
            el.SetAttribute("name", _col.Code);
            el.SetAttribute("table", _col.RefEntityTableName);
            el.SetAttribute("generic", "true");
            el.SetAttribute("inverse", "true");

            //缓存相关
            XmlElement el_cache = xmlDoc.CreateElement("cache");
            el_cache.SetAttribute("usage", "read-write");
            el.AppendChild(el_cache);


            XmlElement el1 = xmlDoc.CreateElement("key");
            el1.SetAttribute("column", _col.RefDBCode);
            el1.SetAttribute("foreign-key", "FK_" + _col.RefEntityTableName + "_" + _col.RefColCode + "_" + _col.Entity.Code);
            el.AppendChild(el1);

            XmlElement el2 = xmlDoc.CreateElement("one-to-many");
            el2.SetAttribute("class", _col.TypeString + "," + _col.BEProj.Namespace);
            el.AppendChild(el2);
            return el;
        }
        /// <summary>
        /// 聚合类型
        /// </summary>
        /// <returns></returns>
        public override string BuildMetaData()
        {
            return string.Empty;
        }

        public override string EntityColumnToDTOColumn()
        {
            string tpl = BECodeTemplate.EntityColumnToDTOCompositionTemplate.Replace(Attributes.TypeString, this._col.TypeString).Replace(Attributes.Code, this._col.Code);
            return tpl;
        }
        public override string EntityColumnFromDTOColumn()
        {
            string dtoClass = Util.UtilHelper.GetDTOColumnTypeByBEColumn(this._col.TypeString);
            string tpl = BECodeTemplate.EntityColumnFromDTOCompositionTemplate.Replace(Attributes.Code, this._col.Code).Replace(Attributes.TypeString, this._col.TypeString).Replace(Attributes.Class, dtoClass);
            return tpl;
        }
    }
}
