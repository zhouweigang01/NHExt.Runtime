using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Code.Builder.Build.Tpl
{
    public static class DTOCodeTemplate
    {
        /// <summary>
        /// 普通类型代码片段
        /// </summary>
        public static string CommonTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.DTOCommonTemplate + Environment.NewLine;
            }

        }
        /// <summary>
        /// 关联类型引用代码片段
        /// </summary>
        public static string RefrenceTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.DTORefrenceTemplate + Environment.NewLine;
            }
        }
        public static string EnumRefrenceTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.DTOEnumRefrenceTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 聚合类型代码片段
        /// </summary>
        public static string CompositionTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.DTOCompositionTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类开始代码片段
        /// </summary>
        public static string ClassBeginTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.DTOClassBeginTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类结束代码片段
        /// </summary>
        public static string ClassEndTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.DTOClassEndTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类扩展代码
        /// </summary>
        public static string ClassExtendTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.DTOClassExtendTemplate + Environment.NewLine;
            }
        }


        public static string DTOSetValueBeginTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.DTOSetValueBeginTemplate + Environment.NewLine;
            }
        }
        public static string DTOSetCommonValueTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.DTOSetCommonValueTemplate + Environment.NewLine;
            }
        }
        public static string DTOSetListValueTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.DTOSetListValueTemplate + Environment.NewLine;
            }
        }
        public static string DTOSetValueEndTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.DTOSetValueEndTemplate + Environment.NewLine;
            }
        }


    }
}
