using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.BPBuild
{
    class BuildBPComposition : BuildBPColumn
    {
        public BuildBPComposition(BPColumn col)
            : base(col)
        {
            _templateCode = BPCodeTemplate.CompositionTemplate;
          
            if (this._col.IsNull && string.IsNullOrEmpty(col.RefGuid) && this._col.TypeString.ToLower() != "string")
            {
                _templateCode = _templateCode.Replace(Attributes.IsNull, "?");
            }
            else
            {
                _templateCode = _templateCode.Replace(Attributes.IsNull, "");
            }
        }
    }
}
