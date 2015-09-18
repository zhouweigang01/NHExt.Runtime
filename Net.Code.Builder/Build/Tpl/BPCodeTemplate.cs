using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Code.Builder.Build.Tpl
{
    public static class BPCodeTemplate
    {
        /// <summary>
        /// 普通类型代码片段
        /// </summary>
        public static string CommonTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPCommonTemplate + Environment.NewLine;
            }

        }
        /// <summary>
        /// 关联类型引用代码片段
        /// </summary>
        public static string RefrenceTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPRefrenceTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 聚合类型代码片段
        /// </summary>
        public static string CompositionTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPCompositionTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类开始代码片段
        /// </summary>
        public static string ProxyClassBeginTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPProxyClassBeginTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类结束代码片段
        /// </summary>
        public static string ProxyClassEndTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPProxyClassEndTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类扩展代码
        /// </summary>
        public static string ProxyConstructorTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPProxyConstructorTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类构造函数开始代码片段
        /// </summary>
        public static string ProxyDoCommonBeginTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPProxyDoCommonBeginTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类构造函数开始代码片段
        /// </summary>
        public static string ProxyDoListBeginTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPProxyDoListBeginTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类构造函数结束代码片段
        /// </summary>
        public static string ProxyDoCommonTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPProxyDoCommonTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类构造函数结束代码片段
        /// </summary>
        public static string ProxyDoListTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPProxyDoListTemplate + Environment.NewLine;
            }
        }

        /// <summary>
        /// 类构造函数结束代码片段
        /// </summary>
        public static string ProxyDoCommonEndTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPProxyDoCommonEndTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 类构造函数结束代码片段
        /// </summary>
        public static string ProxyDoListEndTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPProxyDoListEndTemplate + Environment.NewLine;
            }
        }
        /// <summary>
        /// 在构造函数中初始化聚合引用代码片段
        /// </summary>
        public static string ProxyInitParamListTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPProxyInitParamListTemplate + Environment.NewLine;
            }
        }

        public static string BPClassBeginTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPClassBeginTemplate + Environment.NewLine;
            }
        }

        public static string BPClassEndTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPClassEndTemplate + Environment.NewLine;
            }
        }

        public static string BPClassExtendTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPClassExtendTemplate + Environment.NewLine;
            }
        }
        public static string BPDoCommonObjectTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPDoCommonObjectTemplate + Environment.NewLine;
            }
        }
        public static string BPDoWcfObjectTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPDoWcfObjectTemplate + Environment.NewLine;
            }
        }

        public static string BPClassInitParamBeginTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPClassInitParamBeginTemplate + Environment.NewLine;
            }
        }
        public static string BPClassInitParamCommonTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPClassInitParamCommonTemplate + Environment.NewLine;
            }
        }
        public static string BPClassInitParamCompositionTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPClassInitParamCompositionTemplate + Environment.NewLine;
            }
        }
        public static string BPClassInitParamEndTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPClassInitParamEndTemplate + Environment.NewLine;
            }
        }
        public static string BPDoExtendObjectTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPDoExtendObjectTemplate + Environment.NewLine;
            }
        }

        public static string BPDoObjectObjectTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPDoObjectObjectTemplate + Environment.NewLine;
            }
        }
        public static string BPTypeConvertBeginTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPTypeConvertBeginTemplate + Environment.NewLine;
            }
        }
        public static string BPTypeConvertToDTOTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPTypeConvertToDTOTemplate + Environment.NewLine;
            }
        }
        public static string BPTypeConvertListToDTOTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPTypeConvertListToDTOTemplate + Environment.NewLine;
            }
        }
        public static string BPTypeConvertCommonTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPTypeConvertCommonTemplate + Environment.NewLine;
            }
        }
        public static string BPTypeConvertEndTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPTypeConvertEndTemplate + Environment.NewLine;
            }
        }

        public static string BPSetValueBeginTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPSetValueBeginTemplate + Environment.NewLine;
            }
        }
        public static string BPSetCommonValueTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPSetCommonValueTemplate + Environment.NewLine;
            }
        }
        public static string BPSetListValueTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPSetListValueTemplate + Environment.NewLine;
            }
        }
        public static string BPSetValueEndTemplate
        {
            get
            {
                return global::Net.Code.Builder.Properties.Resources.BPSetValueEndTemplate + Environment.NewLine;
            }
        }


        public static string TransSupportTemplate = "NHExt.Runtime.Enums.TransactionSupport.Support";
        public static string TransRequiredTemplate = "NHExt.Runtime.Enums.TransactionSupport.Required";
        public static string TransRequiredNewTemplate = "NHExt.Runtime.Enums.TransactionSupport.RequiredNew";
    }
}
