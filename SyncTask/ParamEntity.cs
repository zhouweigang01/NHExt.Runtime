using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json.Linq;

namespace SyncTask
{
    /// <summary>
    /// 参数类
    /// </summary>
    public class ParamEntity
    {
        public static readonly string Now = "$Now$";
        //参数编码
        private string JSonStr { get; set; }
        public List<JToken> GetTokens()
        {
            if (string.IsNullOrEmpty(this.JSonStr))
            {
                this.JSonStr = "{}";
            }
            string tokenStr = JSonStr.Replace(ParamEntity.Now, DateTime.Now.ToString("yyyy-MM-dd"));
            List<JToken> tokenList = new List<JToken>();
            JToken token = JToken.Parse(tokenStr);

            //这个里面添加的肯定都是属性
            foreach (JToken t in token.Children())
            {
                tokenList.Add(t);
            }
            return tokenList;
        }

        public ParamEntity(string json)
        {
            this.JSonStr = json;
        }
    }
}
