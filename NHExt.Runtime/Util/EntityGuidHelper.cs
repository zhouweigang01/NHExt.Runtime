using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NHExt.Runtime.Util
{
    /// <summary>
    /// 实体ID生成类，最多支持一秒生成9999个实体ID
    /// </summary>
    public class EntityGuidHelper
    {
        /// <summary>
        /// 每一个种子区间最多生成999个实体
        /// </summary>
        private static readonly int Max = 1000;

        private static Object obj = new Object();
        /// <summary>
        /// 当前生成id的index
        /// </summary>
        private static int index = 0;
        /// <summary>
        /// 当前种子计数
        /// </summary>
        private static int seed = 0;

        /// <summary>
        /// 上一次产生ID号的时间key
        /// </summary>
        private static string strLastTime;

        /// <summary>
        /// 获取实体唯一ID
        /// </summary>
        /// <returns></returns>
        public static long New()
        {
            lock (obj)
            {
                string strNowTime = getDateTime();
                if (string.IsNullOrEmpty(strLastTime))
                {
                    index = 0;
                }
                else
                {
                    if (strNowTime != strLastTime)
                    {
                        index = 0;
                    }
                    else
                    {
                        index++;
                    }
                }
                strLastTime = strNowTime;

                string strIndex = "";
                if (index >= EntityGuidHelper.Max)
                {
                    index = index % EntityGuidHelper.Max;
                    seed++;
                }
                if (seed == 10)
                {
                    throw new NHExt.Runtime.Exceptions.RuntimeException("当前实体在一秒内已经达到生成的最大实体数目");
                }
                if (index < 10)
                {
                    strIndex = "00" + index;
                }
                else if (index < 100)
                {
                    strIndex = "0" + index;
                }
                else
                {
                    strIndex = index.ToString();
                }

                return Int64.Parse(strNowTime + seed + strIndex);
            }
        }
        /// <summary>
        /// 获取ID实体ID列表
        /// </summary>
        /// <param name="number"></param>
        /// <returns></returns>
        public static List<long> New(int number)
        {
            lock (obj)
            {
                if (number >= EntityGuidHelper.Max)
                {
                    throw new NHExt.Runtime.Exceptions.RuntimeException("一次生成的ID不能大于" + Max);
                }

                string strNowTime = getDateTime();
                if (string.IsNullOrEmpty(strLastTime))
                {
                    index = 0;
                    seed = 0;
                }
                else
                {
                    if (strNowTime != strLastTime)
                    {
                        index = 0;
                        seed = 0;
                    }
                    else
                    {
                        index++;
                    }
                }
                strLastTime = strNowTime;
                var tmpNumber = index + number;
                //如果超出了999的话种子跳跃一个区间，在下一个期间里面生成number个实体
                if (tmpNumber % EntityGuidHelper.Max != tmpNumber)
                {
                    seed++;
                    index = 0;
                    if (seed == 10)
                    {
                        throw new NHExt.Runtime.Exceptions.RuntimeException("当前实体在一秒内已经达到生成的最大实体数目");
                    }
                }

                List<long> idList = new List<long>();
                int start = 0;
                string strIndex;
                string strBegin = getDateTime();
                while (start < number)
                {
                    if (start < 10)
                    {
                        strIndex = "00" + start;
                    }
                    else if (start < 100)
                    {
                        strIndex = "0" + start;
                    }
                    else
                    {
                        strIndex = start.ToString();
                    }

                    idList.Add(long.Parse(strBegin + seed + strIndex));
                    start++;
                    index++;
                }
                return idList;
            }
        }

        private static string getDateTime()
        {
            DateTime dt = DateTime.Now;
            string strDT = dt.ToString("yyMMddHHmmss");
            return strDT;
        }
    }
}
