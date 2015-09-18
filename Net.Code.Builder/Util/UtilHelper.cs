using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Build.Model;

namespace Net.Code.Builder.Util
{
    static class UtilHelper
    {
        private static Hashtable htTypeMapping = null;
        public static string GetDBType(string type)
        {
            if (htTypeMapping == null)
            {
                htTypeMapping = new Hashtable();
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.Load(System.Windows.Forms.Application.StartupPath + @"\Config\TypeMapping.xml");
                XmlNodeList nodeList = xmlDoc.SelectNodes("Mappings/Mapping");
                if (nodeList != null)
                {
                    foreach (XmlNode node in nodeList)
                    {
                        string key = node.Attributes["sourceType"].Value.ToString();
                        if (!htTypeMapping.ContainsKey(key))
                        {
                            htTypeMapping.Add(key, node.Attributes["databaseType"].Value.ToString());
                        }
                    }
                }
            }
            return htTypeMapping[type] as string;
        }

        public static string GetDTOColumnTypeByBEColumn(string beColTypeString)
        {
            string dtoColTypeString = string.Empty;
            string[] typeArray = beColTypeString.Split('.');
            int length = typeArray.Length;
            for (int i = 0; i < length - 2; i++)
            {
                dtoColTypeString += typeArray[i] + ".";
            }
            dtoColTypeString += typeArray[length - 2] + "." +DTOEntity.AssemblyEndTag + ".";
            dtoColTypeString += typeArray[length - 1] + DTOEntity.AttrEndTag;
            return dtoColTypeString;
        }
    }

}
