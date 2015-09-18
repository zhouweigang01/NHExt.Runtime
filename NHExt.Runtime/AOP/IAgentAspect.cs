using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.AOP
{
    public interface IAgentAspect
    {
        void BeforeDo(Model.IBizAgent agent, List<object> paramList);
        void AfterDo(Model.IBizAgent agent, object data);
    }
}
