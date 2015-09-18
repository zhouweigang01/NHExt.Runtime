using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Code.Builder.Build.Tpl
{
    public static class Attributes
    {
        public static string Code = "{Code}";
        public static string Name = "{Name}";
        public static string TypeString = "{TypeString}";
        public static string NameSpace = "{NameSpace}";
        public static string Class = "{Class}";
        public static string Guid = "{Guid}";
        public static string Assembly = "{Assembly}";
        public static string CompileFile = "{CompileFiles}";
        public static string ProjCode = "{ProjCode}";
        public static string Memo = "{Memo}";
        public static string PrimaryKey = "{PrimaryKey}";
        public static string EmbeddedResource = "{EmbeddedResource}";
        public static string Table = "{TableName}";
        public static string EntityName = "{EntityName}";
        public static string InhertClass = "{InhertClass}";
        public static string IsNull = "{IsNull}";
        public static string Return = "{Return}";
        public static string ProxyReturn = "{ProxyReturn}";
        public static string RefrenceDll = "{RefrenceDll}";
        public static string RefrenceEntity = "{RefrenceEntity}";
        public static string PostBuild = "{PostBuild}";
        public static string Value = "{Value}";
        public static string EntityRefrence = "{EntityRefrence}";
        public static string IsViewer = "{IsViewer}";
        public static string IsBizKey = "{IsBizKey}";
        public static string CanNull = "{CanNull}";

        public static string ProjectRefrence = "{ProjectRefrence}";
        public static string ProjectSelection = "{ProjectSelection}";

        public static string Index = "{Index}";

        public static string BaseEntity = "NHExt.Runtime.Model.BaseEntity";
        public static string BaseDTO = "NHExt.Runtime.Model.BaseDTO";
        public static string BaseBP = "NHExt.Runtime.Model.BaseBP";

        public static string Trans = "{Trans}";

        public static string Range = "{Range}";
        public static string OrgFilter = "{OrgFilter}";

        public static string RefStr = "{RefStr}";
        public static string ComStr = "{ComStr}";
        public static string PColumn = "{PColumn}";
    }
}
