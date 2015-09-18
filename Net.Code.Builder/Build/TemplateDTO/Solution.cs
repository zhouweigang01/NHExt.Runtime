using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Code.Builder.Build.TemplateDTO
{
    [Serializable]
    public class Solution
    {
        public List<Project> ProjList { get; set; }

        public List<string> GuidList { get; set; }

        public Solution()
        {
            this.ProjList = new List<Project>();
            this.GuidList = new List<string>();
        }
        public void AddProj(Project proj)
        {
            string projGuid = System.Guid.NewGuid().ToString().ToUpper();
            this.GuidList.Add(projGuid);
            this.ProjList.Add(proj);
        }
    }
}
