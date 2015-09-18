using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;

namespace Net.Code.Builder.Base
{
    public interface IPlatformComponent
    {
        void FromXML(XmlNode node);
        void ToXML(XmlDocument xmlDoc);
    }
}
