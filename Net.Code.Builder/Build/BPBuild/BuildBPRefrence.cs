using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Code.Builder.Base;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.BPBuild
{
    class BuildBPRefrence : BuildBPColumn
    {
        public BuildBPRefrence(BPColumn col)
            : base(col)
        {
            _templateCode = BPCodeTemplate.RefrenceTemplate;
            _templateCode = _templateCode.Replace(Attributes.IsNull, "");
        }
    }
}
