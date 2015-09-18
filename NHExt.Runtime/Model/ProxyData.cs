using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace NHExt.Runtime.Model
{
    public class ProxyData
    {
        private NHExt.Runtime.Session.CallerTypeEnum _callerType = Session.CallerTypeEnum.None;

        public NHExt.Runtime.Session.CallerTypeEnum CallerType
        {
            get { return _callerType; }
            set { _callerType = value; }
        }
        private object _objData = null;
        public object ObjData
        {
            get { return _objData; }
            set { _objData = value; }
        }
    }
}
