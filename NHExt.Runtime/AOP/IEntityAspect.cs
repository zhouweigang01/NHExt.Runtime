using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.AOP
{
    public interface IEntityAspect
    {
        void BeforeDo(Model.BaseEntity entity);
        void AfterDo(Model.BaseEntity entity);
    }
}
