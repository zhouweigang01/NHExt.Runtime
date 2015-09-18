using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Base;

namespace Net.Code.Builder.Build.Model
{
    public static class ProjectFactory
    {
        public static IProject BuildProj(string filePath)
        {
            XmlDocument xmlDoc = new XmlDocument();
            xmlDoc.Load(filePath);
            string attr = xmlDoc.FirstChild.Name;
            IProject proj = null;
            if (attr == "EntityProj")
            {
                proj = new BEProj(filePath, string.Empty);
            }
            else if (attr == "BPProj")
            {
                proj = new BPProj(filePath, string.Empty);
            }

            return proj;
        }
    }
}
