using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.DTOBuild
{
    class BuildDTOCommon : BuildDTOColumn
    {
        public BuildDTOCommon(DTOColumn col)
            : base(col)
        {
            _templateCode = DTOCodeTemplate.CommonTemplate;
        }
        public override string BuildCode()
        {
            _templateCode = base.BuildCode();
            //string 类型比较特殊，不能设置为可空类型
            if (this._col.IsNull && this._col.TypeString.ToLower() != "string")
            {
                _templateCode = _templateCode.Replace(Attributes.IsNull, "?");
            }
            else
            {
                _templateCode = _templateCode.Replace(Attributes.IsNull, "");
            }
            return _templateCode;
        }
    }
}
