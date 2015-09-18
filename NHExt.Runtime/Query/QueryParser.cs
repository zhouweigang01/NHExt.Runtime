using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Reflection;
using NHExt.Runtime.EntityAttribute;
using NHExt.Runtime.Util;
using NHExt.Runtime.Enums;

namespace NHExt.Runtime.Query
{
    public class QueryTpl
    {
        public string EntityName { get; set; }
        public string Alias { get; set; }
        public string Hql { get; set; }
        public string ParserHql { get; set; }
        private static List<string> chractorList = new List<string>() { "=", "!=", ">", "<", "in", "like", "not", "is" };
        public void Parser()
        {
            if (string.IsNullOrEmpty(this.Alias) || string.IsNullOrEmpty(this.Hql))
            {
                this.ParserHql = this.Hql;
            }
            else
            {
                string[] segments = this.Hql.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < segments.Length; i++)
                {
                    string segment = segments[i];
                    //符号列
                    if (QueryTpl.chractorList.Contains(segment.ToLower()))
                    {
                        segments[i - 1] = this.Alias + "." + segments[i - 1];
                    }
                }
                string parserHql = string.Empty;
                foreach (string segment in segments)
                {
                    parserHql += " " + segment;
                }
                this.ParserHql = parserHql;
            }

        }
    }
    /// <summary>
    /// hql查询生成方案
    /// </summary>
    public class QueryParser
    {
        /// <summary>
        /// 关键字列表，逗号也要算成关键字
        /// </summary>
        private static List<string> keyList = new List<string>() { "select", "from", "left", "right", "inner", "outer", "join", "on", "where", "having", "as", "group", "order", "by", "asc", "desc" };
        private static List<string> conditionKeyList = new List<string>() { "order by", "group by", "having" };
        private static string WHERE = "where";
        private Queue<string> hqlSegQueue = new Queue<string>();
        protected List<QueryTpl> entityList = new List<QueryTpl>();
        private string hqlTemplate = string.Empty;
        public QueryParser(string queryStr)
        {
            if (!string.IsNullOrEmpty(queryStr))
            {
                string[] segments = queryStr.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string seg in segments)
                {
                    this.hqlSegQueue.Enqueue(seg);
                }
            }
            this.hqlTemplate = queryStr;

            while (this.hqlSegQueue.Count > 0)
            {
                string segment = this.hqlSegQueue.Dequeue();
                string lowerSegment = segment.ToLower();
                //如果是关键字需要解析关键字
                if (QueryParser.keyList.Contains(lowerSegment))
                {
                    //这个关键字后面肯定是实体
                    if (lowerSegment == "from" || lowerSegment == "join")
                    {
                        string entityName = hqlSegQueue.Dequeue();
                        bool exist = false;
                        foreach (QueryTpl tpl in this.entityList)
                        {
                            if (tpl.EntityName == entityName)
                            {
                                exist = true;
                                break;
                            }
                        }
                        if (!exist)
                        {
                            string alias = string.Empty;
                            if (hqlSegQueue.Count > 0)
                            {
                                alias = hqlSegQueue.Dequeue();
                            }
                            if (!string.IsNullOrEmpty(alias))
                            {
                                if (QueryParser.keyList.Contains(alias))
                                {
                                    alias = string.Empty;
                                }
                            }
                            this.entityList.Add(new QueryTpl() { EntityName = entityName, Alias = alias });
                        }
                    }
                }
            }
        }

        protected virtual string decodeHqlExtend()
        {
            return string.Empty;
        }

        public virtual string ParseHql(params object[] args)
        {
            string hqlExtend = this.decodeHqlExtend();
            string hql = this.hqlTemplate;
            if (!string.IsNullOrEmpty(hqlExtend))
            {
                string lowerHql = this.hqlTemplate.ToLower();
                string tmpCondition = string.Empty;

                int whereIndex = lowerHql.LastIndexOf(QueryParser.WHERE);


                //如果是where的话需要截取where的后面字符串

                string preHql = string.Empty;
                string whereHql = string.Empty;
                string nextHql = string.Empty;
                if (whereIndex >= 0)
                {
                    preHql = this.hqlTemplate.Substring(0, whereIndex);
                    whereHql = this.hqlTemplate.Substring(whereIndex + 5);
                }
                else
                {
                    whereHql = this.hqlTemplate;
                }
                //如果存在where的话不需要加1=1
                int conditionIndex = -1;
                foreach (string condition in QueryParser.conditionKeyList)
                {
                    int tmp = whereHql.ToLower().IndexOf(condition);
                    if (conditionIndex < 0 || (tmp > 0 && conditionIndex > tmp))
                    {
                        conditionIndex = tmp;
                    }
                }
                if (conditionIndex >= 0)
                {
                    if (whereIndex >= 0)
                    {
                        nextHql = whereHql.Substring(conditionIndex);
                        whereHql = whereHql.Substring(0, conditionIndex);
                    }
                    else
                    {
                        preHql = whereHql.Substring(0, conditionIndex);
                        nextHql = whereHql.Substring(conditionIndex);
                        whereHql = string.Empty;
                    }
                }
                else
                {
                    if (whereIndex < 0)
                    {
                        preHql = whereHql;
                        whereHql = string.Empty;
                    }
                }

                if (string.IsNullOrEmpty(preHql))
                {
                    throw new NHExt.Runtime.Exceptions.RuntimeException("HQL解析错误，preHql不能为空");
                }
                whereHql = whereHql.Trim();
                nextHql = nextHql.Trim();
                if (!string.IsNullOrEmpty(whereHql))
                {
                    whereHql = "(" + whereHql + ")";
                }

                if (string.IsNullOrEmpty(whereHql))
                {
                    hql = preHql + " " + QueryParser.WHERE + " " + hqlExtend + " " + nextHql;
                }
                else
                {
                    hql = preHql + " " + QueryParser.WHERE + " " + whereHql + " and " + hqlExtend + " " + nextHql;
                }

            }

            return hql;
        }
        /// <summary>
        /// 获取当前的hql解析器
        /// </summary>
        /// <param name="hql"></param>
        /// <returns></returns>
        public static QueryParser GetInstance(string hql)
        {
            return ParserFactory.GetParser(hql);
        }
    }
    /// <summary>
    /// HQL解析器工厂
    /// </summary>
    static class ParserFactory
    {
        public static QueryParser GetParser(string hql)
        {
            QueryParser parser = null;
            parser = NHExt.Runtime.Util.IocFactory.GetIocObject<QueryParser>("parser_extend", hql);
            if (parser == null)
            {
                parser = new NHExt.Runtime.Query.QueryParser(hql);
            }
            return parser;
        }
    }
}
