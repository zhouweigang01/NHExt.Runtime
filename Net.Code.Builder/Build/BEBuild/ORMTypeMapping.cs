using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Net.Code.Builder.Build.BEBuild
{
    static class ORMTypeMapping
    {
        public static string Get(string type)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(System.Windows.Forms.Application.StartupPath + @"\Config\TypeMapping.xml");
            XmlNode node = xmlDoc.SelectSingleNode("Mappings/Mapping[@sourceType='" + type + "']");
            if (node != null)
            {
                return node.Attributes["durationType"].Value;
            }
            return type;
        }
    }
}
