using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using Net.Code.Builder.Build.Model;

namespace Net.Code.Builder.Base
{
    public interface IProject
    {
        /// <summary>
        /// 项目地址
        /// </summary>
        string ProjPath
        {
            get;
        }
        /// <summary>
        /// 元数据目录
        /// </summary>
        string MetaDataPath { get; }
        string CodePath { get; }
        string RuntimePath { get; }
        string AppPath { get; }
        /// <summary>
        /// 项目名称
        /// </summary>
        string ProjName
        {
            get;
            set;
        }
        /// <summary>
        /// 命名空间
        /// </summary>
        string Namespace
        {
            set;
            get;
        }

        string FileName { get; }

        string Guid { get; }
        /// <summary>
        /// 获取实体工程项目中guid为guid的实体对象
        /// </summary>
        /// <param name="guid"></param>
        /// <returns></returns>
        AbstractPlatformComponent Get(string guid);
        /// <summary>
        /// 搜索节点
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        List<AbstractPlatformComponent> Find(string key);
        /// <summary>
        /// 从当前实体项目xml文件中获取guid的实体的attrcode属性的值
        /// 如果当前实体中部存在则从basemetadata文件夹中寻找
        /// </summary>
        /// <param name="guid"></param>
        /// <param name="attrCode"></param>
        /// <returns></returns>
        XmlNode GetEntityNode(string guid);
        /// <summary>
        /// 实体列表
        /// </summary>
        List<AbstractPlatformComponent> EntityList
        { get; }
        /// <summary>
        /// dto列表
        /// </summary>
        List<AbstractPlatformComponent> DTOList
        { get; }
        List<AbstractPlatformComponent> EnumList
        {
            get;
        }
        /// <summary>
        /// 文件夹列表
        /// </summary>
        List<AbstractPlatformComponent> FloderList
        { get; }
        /// <summary>
        /// 引用dll列表
        /// </summary>
        List<AbstractPlatformComponent> RefrenceList
        {
            get;
        }

        ProjectTypeEnum ProjType { get; }

        Floder GetFloder(string guid);

        List<Floder> LoadFloderList(XmlNode node);

        void FromFile(string filePath);
        void Load();
        void ToFile();
    }
}
