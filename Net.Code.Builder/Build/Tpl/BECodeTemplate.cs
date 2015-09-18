using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Code.Builder.Build.Tpl
{
    public static class BECodeTemplate
    {

        /// <summary>
        /// 普通类型代码片段
        /// </summary>
        public static string CommonTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityCommonTemplate + Environment.NewLine;
            }

        }
        /// <summary>
        /// 关联类型引用代码片段
        /// </summary>
        public static string RefrenceCompositionTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityRefrenceCompositionTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 关联类型引用代码片段
        /// </summary>
        public static string RefrenceTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityRefrenceTemplate + Environment.NewLine;
            }
        }

        public static string EnumRefrenceTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityEnumRefrenceTemplate + Environment.NewLine;
            }
        }

        /// <summary>
        /// 聚合类型代码片段
        /// </summary>
        public static string CompositionTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityCompositionTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类开始代码片段
        /// </summary>
        public static string ClassBeginTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityClassBeginTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类结束代码片段
        /// </summary>
        public static string ClassEndTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityClassEndTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类扩展代码
        /// </summary>
        public static string ClassExtendTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityClassExtendTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类构造函数开始代码片段
        /// </summary>
        public static string ConstructorBeginTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityConstructorBeginTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类构造函数结束代码片段
        /// </summary>
        public static string ConstructorEndTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityConstructorEndTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 静态函数Create生成代码片段（根据主实体生成子实体）
        /// </summary>
        public static string CreateFuncTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityCreateFuncTemplate + Environment.NewLine;
            }
        }

        public static string CloneFunctionTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityCloneFunctionTemplate + Environment.NewLine;
            }
        }
        public static string CloneObjBeginTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityCloneObjBeginTemplate + Environment.NewLine;
            }
        }
        public static string CloneObjEndTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityCloneObjEndTemplate + Environment.NewLine;
            }
        }
        public static string CloneCommonTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityCloneCommonTemplate + Environment.NewLine;
            }
        }
        public static string CloneEnumTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityCloneEnumTemplate + Environment.NewLine;
            }
        }
        public static string CloneRefrenceTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityCloneRefrenceTemplate + Environment.NewLine;
            }
        }

        public static string EntityToDTOBeginTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityToDTOBeginTemplate + Environment.NewLine;
            }
        }
        public static string EntityToDTOEndTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityToDTOEndTemplate + Environment.NewLine;
            }
        }
        public static string EntityColumnToDTOCommonTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityColumnToDTOCommonTemplate + Environment.NewLine;
            }
        }
        public static string EntityColumnToDTORefrenceTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityColumnToDTORefrenceTemplate + Environment.NewLine;
            }
        }
        public static string EntityColumnToDTOEnumTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityColumnToDTOEnumTemplate + Environment.NewLine;
            }
        }
        public static string EntityColumnToDTOCompositionTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityColumnToDTOCompositionTemplate + Environment.NewLine;
            }
        }


        public static string EntityFromDTOBeginTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityFromDTOBeginTemplate + Environment.NewLine;
            }
        }
        public static string EntityFromDTOEndTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityFromDTOEndTemplate + Environment.NewLine;
            }
        }
        public static string EntityColumnFromDTOCommonTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityColumnFromDTOCommonTemplate + Environment.NewLine;
            }
        }
        public static string EntityColumnFromDTORefrenceTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityColumnFromDTORefrenceTemplate + Environment.NewLine;
            }
        }
        public static string EntityColumnFromDTOEnumTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityColumnFromDTOEnumTemplate + Environment.NewLine;
            }
        }
        public static string EntityColumnFromDTOCompositionTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.EntityColumnFromDTOCompositionTemplate + Environment.NewLine;
            }
        }


    }
}
