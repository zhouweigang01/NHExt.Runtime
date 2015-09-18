using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Net.Code.Builder.Build.Model;
using Net.Code.Builder.Build.Tpl;

namespace Net.Code.Builder.Build.DTOBuild
{
    class BuildDTOComposition : BuildDTOColumn
    {
        public BuildDTOComposition(DTOColumn col)
            : base(col)
        {
            _templateCode = DTOCodeTemplate.CompositionTemplate;
        }

        public override string BuildCode()
        {
            return base.BuildCode();
        }
    }
}
