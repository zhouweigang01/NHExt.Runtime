using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Net.Code.Builder.Base
{
    public interface IBuild
    {
        string BuildCode();

        XmlElement BuildCfgXML(XmlDocument xmlDoc);

        string BuildMSSQL();

        string BuildMetaData();

    }
}
