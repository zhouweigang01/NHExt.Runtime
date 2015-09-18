using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Code.Builder.Util
{
    public delegate void ConsoleHandler(string msg);
    public class OutPut
    {
        public static ConsoleHandler OutPutMsgHandler;
        public static void OutPutMsg(string msg)
        {
            if (OutPutMsgHandler != null) {
                OutPutMsgHandler(msg);
            }
        }
    }
}
