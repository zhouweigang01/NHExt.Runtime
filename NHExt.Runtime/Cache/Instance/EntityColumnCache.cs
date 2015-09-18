using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Cache.Instance
{
    public class EntityColumnCache : NHExt.Runtime.Cache.AbstractCache<string, List<NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute>>
    {
        public EntityColumnCache()
            : base("NHExt.Runtime.Cache.EntityColumnCache")
        {
        }

        public override string JoinKey(string entityName)
        {
            return entityName;
        }


    }
}
