using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Net.Code.Builder.Enums
{
    public enum NodeType
    {
        BEProj,//实体项目
        BEEntity,//实体节点
        BPProj,//操作项目
        BPEntity,//操作实体
        Floder,//文件夹分类
        DTOEntity,//数据传输对象
        Refrence,//引用节点
        RefrenceDll,//引用dll节点
        EnumEntity //枚举
    }

    public enum RefType
    {
        None,
        BEEntity,
        BPEntity,
        Deploy,//DTO形式dll
        Agent//BP代理形式dll
    }

    public enum ColumnTypeEnum
    {
        CommonType = 1,
        EntityType = 2,
        EnumType = 3
    }
}
