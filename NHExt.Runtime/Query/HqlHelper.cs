using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

using System.Data;

namespace NHExt.Runtime.Query
{
    internal static class HqlHelper
    {
        public static string TransferHql(string hql)
        {
            hql = hql.Replace("${", ":fn");
            hql = hql.Replace("}$", " ");
            return hql;
        }

        public static string TransferSQL(string sql)
        {
            sql = sql.Replace("${", ":fn");
            sql = sql.Replace("}$", " ");
            return sql;
        }

        public static DbType GetDBType(object obj)
        {
            if (obj is System.Collections.IList)
            {
                System.Collections.IList objList = (System.Collections.IList)obj;
                if (objList == null && objList.Count == 0)
                {
                    throw new NHExt.Runtime.Exceptions.RuntimeException("集合参数数据列表不能为空");
                }
                Type t = objList[0].GetType();
                return HqlHelper.GetDBType(t);
            }
            else
            {
                Type t = obj.GetType();
                return HqlHelper.GetDBType(t);
            }
        }

        private static DbType GetDBType(Type t)
        {

            if (t.IsValueType)
            {
                if (t.Equals(typeof(bool)) || t.Equals(typeof(Boolean)))
                {
                    return DbType.Boolean;
                }
                else if (t.Equals(typeof(byte)) || t.Equals(typeof(Byte)))
                {
                    return DbType.Byte;
                }
                else if (t.Equals(typeof(char)) || t.Equals(typeof(Char)))
                {
                    return DbType.Byte;
                }
                else if (t.Equals(typeof(DateTime)))
                {
                    return DbType.DateTime;
                }
                else if (t.Equals(typeof(short)) || t.Equals(typeof(Int16)))
                {
                    return DbType.Int16;
                }
                else if (t.Equals(typeof(int)) || t.Equals(typeof(Int32)))
                {
                    return DbType.Int32;
                }
                else if (t.Equals(typeof(double)) || t.Equals(typeof(Double)))
                {
                    return DbType.Double;
                }
                else if (t.Equals(typeof(long)) || t.Equals(typeof(Int64)))
                {
                    return DbType.Int64;
                }
                else if (t.Equals(typeof(decimal)) || t.Equals(typeof(Decimal)))
                {
                    return DbType.Decimal;
                }
                else if (t.Equals(typeof(float)) || t.Equals(typeof(Single)))
                {
                    return DbType.Decimal;
                }

            }
            else
            {
                if (t.Equals(typeof(NHExt.Runtime.Model.BaseEntity)))
                {
                    return DbType.Int64;
                }
                else if (t.Equals(typeof(NHExt.Runtime.Model.BaseEnum)))
                {
                    return DbType.Int32;
                }
                else if (t.Equals(typeof(string)) || t.Equals(typeof(String)) || t.Equals(typeof(StringBuilder)))
                {
                    return DbType.String;
                }
            }
            throw new NHExt.Runtime.Exceptions.RuntimeException("没有找到合适的类型");

        }



    }
}
