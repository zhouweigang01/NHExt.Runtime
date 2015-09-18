using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHExt.Runtime.Enums;
using Iesi.Collections;
using System.Reflection;
using NHExt.Runtime.EntityAttribute;

namespace NHExt.Runtime.Model
{
    public abstract class BaseEntity : IEntity
    {
        #region 属性字段

        private EntityState _entityState = EntityState.UnKnow;

        /// <summary>
        /// 数据可见范围
        /// </summary>
        public virtual ViewRangeEnum Range
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        public virtual bool OrgFilter
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        /// <summary>
        /// 实体状态
        /// </summary>
        public virtual EntityState EntityState
        {
            get
            {
                return _entityState;
            }
            set
            {
                if (this._entityState != value)
                {
                    _entityState = value;
                    if (this._entityState == Enums.EntityState.UnKnow || this._entityState == Enums.EntityState.UnChanged)
                    {
                        this._NEED_CRUD = false;
                    }
                    else
                    {
                        this._NEED_CRUD = true;
                    }
                }
            }
        }
        //是否需要进行CRUD
        private bool _NEED_CRUD = false;

        //是否需要校验
        private bool _isBizKeyCheck = true;
        protected virtual bool IsBizKeyCheck
        {
            get { return _isBizKeyCheck; }
        }

        private long _id;
        public virtual long ID
        {
            get { return _id; }
            set { _id = value; }
        }

        private int _sysVersion;
        public virtual int SysVersion
        {
            get { return _sysVersion; }
            set { _sysVersion = value; }
        }

        public virtual NHExt.Runtime.Model.EntityKey<BaseEntity> Key
        {
            get
            {
                throw new NotImplementedException("没有重写实体KEY");
            }
        }
        #endregion

        #region 业务实体内部校验

        private List<NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute> _columnAttrbuteList = null;
        /// <summary>
        /// 实体内部基础校验，主要是非空和主键重复校验
        /// </summary>
        private void BizEntityValidate()
        {
            if (this._columnAttrbuteList == null)
            {
                this._columnAttrbuteList = BaseEntity.GetPropertyByAttrList(this);
            }
            if (this._columnAttrbuteList != null)
            {

                string hql = string.Empty;
                List<object> paramList = new List<object>();
                string errMsg = string.Empty;
                bool bizCheck = false;
                foreach (NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute attr in this._columnAttrbuteList)
                {
                    //字段不能为空校验
                    if (!attr.IsNull)
                    {
                        if (this.getData(attr.Code) == null)
                        {
                            throw new NHExt.Runtime.Exceptions.BizException("字段“" + attr.Description + "(" + attr.Code + ")”不允许为空");
                        }
                    }
                    //是否走校验
                    if (this._isBizKeyCheck)
                    {
                        //硬编码，费组织字段如果有勾选为业务主键的话则需要进行校验
                        if (attr.IsBizKey)
                        {
                            if (attr.Code != "Orgnization")
                            {
                                bizCheck = true;
                            }
                            if (string.IsNullOrEmpty(hql))
                            {
                                hql = "ID != ${0}$";
                                paramList.Add(this.ID);
                                errMsg = "主键组合为";
                            }
                            object obj = this.getData(attr.Code);
                            if (obj == null)
                            {
                                hql += " and " + attr.Code + " is null";
                            }
                            else
                            {
                                hql += " and " + attr.Code + "=${" + paramList.Count + "}$";
                                paramList.Add(obj);
                            }
                            errMsg += " " + attr.Description + ",";

                        }
                    }
                }
                ///主键数据不能重复校验
                if (bizCheck)
                {
                    BaseEntity be = NHExt.Runtime.Query.EntityFinder.Find(this.GetType(), hql, paramList);
                    if (be != null)
                    {
                        errMsg = errMsg.Substring(0, errMsg.Length - 1);
                        errMsg += "的数据已经存在";
                        throw new NHExt.Runtime.Exceptions.BizException(errMsg);
                    }
                }

            }
        }

        public static List<NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute> GetPropertyByAttrList(BaseEntity entity)
        {
            NHExt.Runtime.Cache.ICache<string, List<NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute>> cache = NHExt.Runtime.Cache.CacheFactory.GetEntityColumnCache();
            List<NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute> columnList = cache.GetCache(entity.EntityName);
            if (columnList == null)
            {
                columnList = NHExt.Runtime.Util.AttributeHelper.GetPropertyByAttrList<NHExt.Runtime.EntityAttribute.ColumnDescriptionAttribute>(entity.GetType());
                if (columnList != null)
                {
                    cache.SetCache(entity.EntityName, columnList);
                }
            }
            return columnList;
        }

        #endregion

        #region     内部调用保护方法


        #region AOP

        private List<NHExt.Runtime.AOP.IEntityAspect> _insertingInsectorList = null;
        private List<NHExt.Runtime.AOP.IEntityAspect> _insertedInsectorList = null;

        private List<NHExt.Runtime.AOP.IEntityAspect> _updatingInsectorList = null;
        private List<NHExt.Runtime.AOP.IEntityAspect> _updatedInsectorList = null;

        private List<NHExt.Runtime.AOP.IEntityAspect> _deletingInsectorList = null;
        private List<NHExt.Runtime.AOP.IEntityAspect> _deletedInsectorList = null;

        #endregion

        protected virtual void SetDefaultValue()
        {
            NHExt.Runtime.Logger.LoggerHelper.Debug("执行" + _entityName + "设置默认值事件ID=" + ID);
        }

        protected virtual void Validate()
        {
            NHExt.Runtime.Logger.LoggerHelper.Debug("执行" + _entityName + "校验事件ID=" + ID);

        }
        #region inserting
        private void beforeInserting()
        {
            if (this._insertingInsectorList == null)
            {
                this._insertingInsectorList = AOP.AspectManager.BuildEntityAspect(this._entityName, AOP.AopPositionEnum.Inserting);
            }
            //执行AOP前事件
            if (this._insertingInsectorList != null)
            {
                foreach (AOP.IEntityAspect insector in this._insertingInsectorList)
                {
                    insector.BeforeDo(this);
                }
            }
        }
        protected virtual void Inserting()
        {
            NHExt.Runtime.Logger.LoggerHelper.Debug("执行" + _entityName + "新增前事件ID=" + ID);
        }

        private void afterInserting()
        {
            this.BizEntityValidate();

            if (this._insertingInsectorList != null)
            {
                foreach (AOP.IEntityAspect insector in this._insertingInsectorList)
                {
                    insector.AfterDo(this);
                }
            }
        }
        #endregion

        #region inserted
        private void beforeInserted()
        {
            if (this._insertedInsectorList == null)
            {
                this._insertedInsectorList = AOP.AspectManager.BuildEntityAspect(this._entityName, AOP.AopPositionEnum.Inserted);
            }
            //执行AOP前事件
            if (this._insertedInsectorList != null)
            {
                foreach (AOP.IEntityAspect insector in this._insertedInsectorList)
                {
                    insector.BeforeDo(this);
                }
            }
        }
        protected virtual void Inserted()
        {
             NHExt.Runtime.Logger.LoggerHelper.Debug("执行" + _entityName + "新增后事件ID=" + ID);
        }

        private void afterInserted()
        {
            //执行AOP前事件
            if (this._insertedInsectorList != null)
            {
                foreach (AOP.IEntityAspect insector in this._insertedInsectorList)
                {
                    insector.AfterDo(this);
                }
            }
        }
        #endregion

        #region updating
        private void beforeUpdating()
        {
            if (this._updatingInsectorList == null)
            {
                this._updatingInsectorList = AOP.AspectManager.BuildEntityAspect(this._entityName, AOP.AopPositionEnum.Updating);
            }
            //执行AOP前事件
            if (this._updatingInsectorList != null)
            {
                foreach (AOP.IEntityAspect insector in this._updatingInsectorList)
                {
                    insector.BeforeDo(this);
                }
            }
        }
        protected virtual void Updating()
        {
             NHExt.Runtime.Logger.LoggerHelper.Debug("执行" + _entityName + "更新前事件ID=" + ID);

        }

        private void afterUpdating()
        {
            this.BizEntityValidate();
            //执行AOP前事件
            if (this._updatingInsectorList != null)
            {
                foreach (AOP.IEntityAspect insector in this._updatingInsectorList)
                {
                    insector.AfterDo(this);
                }
            }
        }
        #endregion

        #region  updated

        private void beforeUpdated()
        {
            if (this._updatedInsectorList == null)
            {
                this._updatedInsectorList = AOP.AspectManager.BuildEntityAspect(this._entityName, AOP.AopPositionEnum.Updated);
            }
            //执行AOP前事件
            if (this._updatedInsectorList != null)
            {
                foreach (AOP.IEntityAspect insector in this._updatedInsectorList)
                {
                    insector.BeforeDo(this);
                }
            }
        }
        protected virtual void Updated()
        {
             NHExt.Runtime.Logger.LoggerHelper.Debug("执行" + _entityName + "更新后事件ID=" + ID);
        }
        private void afterUpdated()
        {
            //执行AOP前事件
            if (this._updatedInsectorList != null)
            {
                foreach (AOP.IEntityAspect insector in this._updatedInsectorList)
                {
                    insector.AfterDo(this);
                }
            }
        }
        #endregion

        #region deleting

        private void beforeDeleting()
        {
            if (this._deletingInsectorList == null)
            {
                this._deletingInsectorList = AOP.AspectManager.BuildEntityAspect(this._entityName, AOP.AopPositionEnum.Deleting);
            }
            //执行AOP前事件
            if (this._deletingInsectorList != null)
            {
                foreach (AOP.IEntityAspect insector in this._deletingInsectorList)
                {
                    insector.BeforeDo(this);
                }
            }
        }

        protected virtual void Deleting()
        {
             NHExt.Runtime.Logger.LoggerHelper.Debug("执行" + _entityName + "删除前事件ID=" + ID);
        }

        private void afterDeleting()
        {
            //执行AOP前事件
            if (this._deletingInsectorList != null)
            {
                foreach (AOP.IEntityAspect insector in this._deletingInsectorList)
                {
                    insector.AfterDo(this);
                }
            }
        }

        #endregion

        #region deleted
        private void beforeDeleted()
        {
            if (this._deletedInsectorList == null)
            {
                this._deletedInsectorList = AOP.AspectManager.BuildEntityAspect(this._entityName, AOP.AopPositionEnum.Deleted);
            }
            //执行AOP前事件
            if (this._deletedInsectorList != null)
            {
                foreach (AOP.IEntityAspect insector in this._deletedInsectorList)
                {
                    insector.BeforeDo(this);
                }
            }
        }

        protected virtual void Deleted()
        {
             NHExt.Runtime.Logger.LoggerHelper.Debug("执行" + _entityName + "删除后事件ID=" + ID);
        }

        private void afterDeleted()
        {
            //执行AOP前事件
            if (this._deletedInsectorList != null)
            {
                foreach (AOP.IEntityAspect insector in this._deletedInsectorList)
                {
                    insector.AfterDo(this);
                }
            }
        }
        #endregion

        #endregion

        #region 构造函数

        /// <summary>
        /// 实体名称
        /// </summary>
        protected string _entityName = string.Empty;
        /// <summary>
        /// 实体名称
        /// </summary>
        public virtual string EntityName
        {
            get { return _entityName; }
        }
        /// <summary>
        /// 实体GUID
        /// </summary>
        public static string Guid = string.Empty;
        /// <summary>
        /// 构造函数
        /// </summary>
        protected BaseEntity()
        {

        }
        #endregion

        #region 实体组合和引用列表集合
        /// <summary>
        /// 聚合实体列表
        /// </summary>
        public virtual List<System.Collections.IEnumerable> EntityComposition
        {
            get
            {
                List<System.Collections.IEnumerable> lst = new List<System.Collections.IEnumerable>();
                foreach (string key in this.ComStr)
                {
                    object obj = this.GetData(key);
                    if (obj == null) continue;
                    System.Collections.IEnumerable enumerableObj = obj as System.Collections.IEnumerable;
                    if (enumerableObj == null)
                    {
                        NHExt.Runtime.Exceptions.RuntimeException ex = new NHExt.Runtime.Exceptions.RuntimeException("持久化对象列表强制转换出错");
                        NHExt.Runtime.Logger.LoggerHelper.Error(ex);
                        throw ex;
                    }
                    lst.Add(enumerableObj);
                }
                return lst;
            }
        }
        /// <summary>
        ///当前实体所有的引用实体列表 
        /// </summary>
        public virtual List<BaseEntity> EntityRefrence
        {
            get
            {
                List<BaseEntity> lst = new List<BaseEntity>();
                foreach (string key in this.RefStr)
                {
                    object obj = this.GetData(key);
                    if (obj == null) continue;
                    BaseEntity be = obj as BaseEntity;
                    if (be == null)
                    {
                        NHExt.Runtime.Exceptions.RuntimeException ex = new NHExt.Runtime.Exceptions.RuntimeException("持久化对象列表强制转换出错");
                        NHExt.Runtime.Logger.LoggerHelper.Error(ex);
                        throw ex;
                    }
                    lst.Add(be);
                }
                return lst;

            }
        }
        /// <summary>
        /// 引用实体的编码
        /// </summary>
        protected virtual List<string> RefStr
        {
            get
            {
                return null;
            }
        }
        /// <summary>
        /// 聚合实体编码
        /// </summary>
        protected virtual List<string> ComStr
        {
            get
            {
                return null;
            }
        }

        protected virtual string PColumnStr
        {
            get
            {
                return string.Empty;
            }
        }

        #endregion

        #region IEntity接口成员
        #endregion

        #region 保护成员方法
        protected internal virtual void Do()
        {

            bool beforeExcuted = false;
            if (this._NEED_CRUD)
            {
                beforeExcuted = true;
                if (this.EntityState == EntityState.Add && this.ID <= 0)
                {
                    this.ID = NHExt.Runtime.Util.EntityGuidHelper.New();
                }
                if (this.ID <= 0)
                {
                    throw new NHExt.Runtime.Exceptions.RuntimeException("实体" + this._entityName + "当前状态为:" + this.EntityState.ToString() + "ID必须大于0");
                }
                if (this.EntityState != Enums.EntityState.Add && this._orignalData == null)
                {
                    this.InitOrignalData();
                }
                if (this.EntityState != EntityState.Delete)
                {
                    this.SetDefaultValue();
                }
                //执行校验事件
                this.Validate();
                //执行前事件
                if (this.EntityState == EntityState.Add)
                {
                    this.beforeInserting();
                    this.Inserting();
                    this.afterInserting();
                }
                else if (this.EntityState == EntityState.Update)
                {
                    this.beforeUpdating();
                    this.Updating();
                    this.afterUpdating();
                }
                else if (this.EntityState == EntityState.Delete)
                {
                    this.beforeDeleting();
                    this.Deleting();
                    this.afterDeleting();
                }
            }
            //先走引用确保数据库中所有的引用都存在
            foreach (BaseEntity entity in this.EntityRefrence)
            {
                //只有新增才需要走
                if (entity != null)
                {
                    ///关联实体如果是删除状态不需要自动处理
                    ///如果是新增或者修改状态则需要自动处理
                    if (entity.EntityState != Enums.EntityState.Delete)
                    {
                        entity.Do();
                    }
                }
            }
            //删除操作先走组合
            if (this.EntityState == Enums.EntityState.Delete)
            {
                this.excuteComposition();
            }
            //执行CRUD代码
            this.Excute();
            //如果不是删除的话后走执行操作
            if (this.EntityState != Enums.EntityState.Delete)
            {
                this.excuteComposition();
            }

            if (beforeExcuted)
            {
                if (this.EntityState == EntityState.Add)
                {
                    this.beforeInserted();
                    this.Inserted();
                    this.afterInserted();
                }
                else if (this.EntityState == EntityState.Update)
                {
                    this.beforeUpdated();
                    this.Updated();
                    this.afterUpdated();
                }
                else if (this.EntityState == EntityState.Delete)
                {
                    this.beforeDeleted();
                    this.Deleted();
                    this.afterDeleted();
                }

            }
        }
        /// <summary>
        /// 提交实体到NH缓存中去
        /// </summary>
        protected virtual void Excute()
        {
            if (this.EntityState == Enums.EntityState.UnKnow || this.EntityState == Enums.EntityState.UnChanged)
            {
                return;
            }
             NHExt.Runtime.Logger.LoggerHelper.Debug("执行SQL,实体对象:" + this._entityName);
            //获取当前上下文的session边界
            Session.Session session = Session.Session.Current;
            if (session != null)
            {
                session.CommitEntity(this);
            }
            this._NEED_CRUD = false;
        }
        /// <summary>
        /// 执行组合函数代码
        /// </summary>
        private void excuteComposition()
        {
            //先走组合，最后走引用
            foreach (System.Collections.IEnumerable enumerable in this.EntityComposition)
            {
                System.Collections.IEnumerator enumerator = enumerable.GetEnumerator();
                while (enumerator.MoveNext())
                {
                    BaseEntity entity = enumerator.Current as BaseEntity;
                    //如果主实体删除的话需要将所有子实体全部设置为删除
                    if (this.EntityState == Enums.EntityState.Delete)
                    {
                        entity.EntityState = Enums.EntityState.Delete;
                    }
                    //先设置父节点，再走GUID
                    entity.SetParentKey(this);
                    entity.Do();

                }
            }
        }
        /// <summary>
        /// 获取实体entitykey公用方法
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="entityKey"></param>
        /// <returns></returns>
        protected EntityKey<T> GetEntityKey<T>(ref EntityKey<T> entityKey) where T : BaseEntity
        {
            if (this.ID < 0) return null;
            if (entityKey != null)
            {
                if (entityKey.ID == this.ID)
                {
                    return entityKey;
                }
                else
                {
                    entityKey = new EntityKey<T>(this.ID);
                }
            }
            else
            {
                entityKey = new EntityKey<T>(this.ID);
            }
            return entityKey;
        }
        /// <summary>
        /// 设置实体中的聚合属性
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="source"></param>
        /// <param name="duration"></param>
        public virtual void SetComposition<T>(string key, IList<T> v) where T : BaseEntity
        {
            if (v == null)
            {
                NHExt.Runtime.Exceptions.RuntimeException ex = new NHExt.Runtime.Exceptions.RuntimeException("聚合对象列表不能为空");
                Logger.LoggerHelper.Error(ex);
                throw ex;
            }
            this.setData(key, v);
        }
        protected IList<T> GetComposition<T>(string key) where T : BaseEntity
        {
            if (!_hashData.Contains(key))
            {
                IList<T> lst = new List<T>();
                this.SetComposition<T>(key, lst);
            }
            return this.getDataList<T>(key);

        }
        /// <summary>
        /// 设置引用关系
        /// </summary>
        /// <param name="sourceEntity"></param>
        /// <param name="durationEntity"></param>
        /// <param name="primaryKey">是否是聚合关系父实体主键</param>
        public virtual void SetRefrence<T>(string key, T v) where T : BaseEntity
        {
            bool exist = this._hashData.ContainsKey(key);
            this.setData(key, v);
            if (exist)
            {
                this.ModifyEntityState(key);
            }

        }
        /// <summary>
        /// 获取引用值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceEntity"></param>
        /// <param name="sourceEntityKey"></param>
        /// <returns></returns>
        protected T GetRefrence<T>(string key) where T : BaseEntity
        {
            return this.getData<T>(key);
        }
        /// <summary>
        /// 设置引用实体key
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceEntityKey"></param>
        /// <param name="sourceEntity"></param>
        /// <param name="durationEntityKey"></param>
        public virtual void SetRefrenceKey<T>(string key, NHExt.Runtime.Model.EntityKey<T> entityKey) where T : BaseEntity
        {
            bool exist = this._hashData.ContainsKey(key);
            //设置entitykey的时候需要更改entity的值
            if (entityKey == null)
            {
                this.setData(key, null);
            }
            else
            {
                T entity = entityKey.ToEntity();
                this.setData(key, entity);
            }
            if (exist)
            {
                this.ModifyEntityState(key);
            }
        }
        protected NHExt.Runtime.Model.EntityKey<T> GetRefrenceKey<T>(string key) where T : BaseEntity
        {
            T entity = this.getData<T>(key);
            if (entity == null)
            {
                return null;
            }
            else
            {
                return new EntityKey<T>(entity.ID);
            }
        }
        /// <summary>
        /// 设置枚举
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceEnum"></param>
        /// <param name="sourceEnumValue"></param>
        /// <param name="durationEnum"></param>
        public virtual void SetEnum<T>(string key, T v) where T : BaseEnum
        {
            bool exist = this._hashData.ContainsKey(key);
            this.setData(key, v);
            if (exist)
            {
                this.ModifyEntityState(key);
            }

        }
        protected T GetEnum<T>(string key) where T : BaseEnum
        {

            return this.getData<T>(key);
        }
        /// <summary>
        /// 基础类型赋值
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sourceEntity"></param>
        /// <param name="durationEntity"></param>
        public virtual void SetValue<T>(string key, T v)
        {
            bool exist = this._hashData.ContainsKey(key);
            this.setData(key, v);
            if (exist)
            {
                this.ModifyEntityState(key);
            }

        }
        protected T GetValue<T>(string key)
        {

            return this.getData<T>(key);
        }
        /// <summary>
        /// 从DTO转换为实体内部调用函数
        /// </summary>
        /// <param name="dto"></param>
        /// <param name="entity"></param>
        protected virtual void FromDTO(BaseDTO dto, BaseEntity entity)
        {
            // entity.ID = dto.ID;
            // entity.SysVersion = dto.SysVersion;
            //从dto转过来的话不需要转entitystate
        }
        /// <summary>
        /// 将实体转换为DTO内部调用函数
        /// </summary>
        /// <param name="dto"></param>
        protected virtual void ToDTO(BaseDTO dto)
        {
            dto.ID = this.ID;
            dto.SysVersion = this.SysVersion;
        }

        public virtual BaseDTO ToBaseDTO()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region 静态方法

        public static BaseEntity Create()
        {
            return null;
        }

        #endregion

        #region 公共方法
        public virtual BaseEntity GetEntity()
        {
            return null;
        }
        /// <summary>
        /// 记录实体父实体的缓存数据
        /// </summary>
        private static Dictionary<string, PropertyInfo> PKeyList = new Dictionary<string, PropertyInfo>();
        /// <summary>
        /// 获取当前实体关联的类型为type类型的父实体的KEY
        /// </summary>
        /// <param name="typeName">关联父实体的类型</param>
        /// <returns></returns>
        public virtual long GetParentKey(string typeName)
        {
            if (!BaseEntity.PKeyList.ContainsKey(this.EntityName))
            {
                EntityAttribute.RefrenceAttribute compareAttribute = new EntityAttribute.RefrenceAttribute() { RefType = RefrenceTypeEnum.PEntity, TargetEntity = typeName };
                if (compareAttribute != null)
                {
                    List<PropertyInfo> piList = Util.AttributeHelper.GetPropertyByAttr<EntityAttribute.RefrenceAttribute>(this.GetType(), compareAttribute, new Comparison<RefrenceAttribute>(CompositionAttributeCompare));
                    if (piList.Count > 0)
                    {
                        BaseEntity.PKeyList.Add(this.EntityName, piList[0]);
                    }
                }
            }
            PropertyInfo pi = null;
            long pKey = -1;
            BaseEntity.PKeyList.TryGetValue(this.EntityName, out pi);
            if (pi != null)
            {
                pKey = ((BaseEntity)pi.GetValue(this, null)).ID;
            }

            return pKey;
        }
        #endregion

        #region 内部使用方法

        protected virtual void SetParentKey(BaseEntity entity)
        {
            if (!string.IsNullOrEmpty(this.PColumnStr))
            {
                string key = this.PColumnStr;
                BaseEntity be = this.GetData(key) as BaseEntity;
                if (!entity.Equals(be))
                {
                    this.setData(key, entity);

                    this.ModifyEntityState(key);
                }
            }

        }
        /// <summary>
        /// 聚合类型引用判断
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int CompositionAttributeCompare(RefrenceAttribute a, RefrenceAttribute b)
        {
            if (a.RefType == b.RefType)
            {
                if (!string.IsNullOrEmpty(a.TargetEntity))
                {
                    if (a.TargetEntity == b.TargetEntity)
                    {
                        return 0;
                    }
                }
                else
                {
                    return 0;
                }
            }
            return 1;
        }
        /// <summary>
        /// 业务主键判断
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private int ColumnDescriptionAttributeIsBizKeyCompare(ColumnDescriptionAttribute a, ColumnDescriptionAttribute b)
        {
            if (a.IsBizKey == b.IsBizKey)
            {
                return 0;
            }
            return 1;
        }

        #endregion

        #region orignaldata相关
        public virtual BaseEntity OrignalData
        {
            get
            {
                if (this._orignalData == null)
                {
                    this.InitOrignalData();
                }
                return this._orignalData;

            }
        }
        protected BaseEntity _orignalData = null;
        /// <summary>
        /// 设置后台缓存，如果存在就使用原来的缓存
        /// </summary>
        public virtual void InitOrignalData()
        {
            ///只有为空的时候才需要刷新
            if (this._orignalData == null)
            {
                if (NHExt.Runtime.Session.Session.Current != null)
                {
                    BaseEntity be = NHExt.Runtime.Session.Session.Current.Get(this._entityName, this._id);
                    if (be != null)
                    {
                        this._orignalData = be.Clone();
                    }
                    else
                    {
                        this._orignalData = null;
                    }
                }
            }
        }
        /// <summary>
        /// 重新刷新后台缓存
        /// </summary>
        public virtual void RefreshOrignalData()
        {
            this._orignalData = null;
            this.InitOrignalData();
        }
        public virtual BaseEntity Clone()
        {
            throw new NHExt.Runtime.Exceptions.RuntimeException("Clone方法没有实现:" + this._entityName);
        }

        public virtual void Clone(BaseEntity cloneBE)
        {
            cloneBE.ID = this.ID;
            cloneBE.SysVersion = this.SysVersion;
            cloneBE.EntityState = this.EntityState;
        }

        /// <summary>
        /// 更新当前实体状态，并将update状态的实体加入到缓存中
        /// </summary>
        /// <param name="key"></param>
        private void ModifyEntityState(string key)
        {
            //状态为更改缓存中没有需要重新添加到缓存中
            if (this.EntityState != Enums.EntityState.UnChanged && this.EntityState != Enums.EntityState.UnKnow)
            {
                if (!NHExt.Runtime.Session.Session.Current.Contains(this))
                {
                    NHExt.Runtime.Session.Session.Current.InList(this);
                    return;
                }
            }
            if (this.ID > 0 && this.EntityState == Enums.EntityState.UnKnow)
            {
                this.EntityState = Enums.EntityState.UnChanged;
            }
            if (this.EntityState == Enums.EntityState.UnKnow) return;
            if (this._orignalData == null)
            {
                this.InitOrignalData();
            }
            if (this._orignalData != null)
            {
                object entityObj = this._hashData[key];
                object orignalObj = this._orignalData._hashData[key];
                if (entityObj == null)
                {
                    if (orignalObj != null)
                    {
                        if (this.EntityState == Enums.EntityState.UnChanged || this.EntityState == Enums.EntityState.UnKnow)
                        {
                            this.EntityState = Enums.EntityState.Update;
                        }
                        if (NHExt.Runtime.Session.Session.Current != null)
                        {
                            NHExt.Runtime.Session.Session.Current.InList(this);
                        }
                    }
                }
                else
                {
                    if (!entityObj.Equals(orignalObj))
                    {
                        if (this.EntityState == Enums.EntityState.UnChanged || this.EntityState == Enums.EntityState.UnKnow)
                        {
                            this.EntityState = Enums.EntityState.Update;
                        }
                        if (NHExt.Runtime.Session.Session.Current != null)
                        {
                            NHExt.Runtime.Session.Session.Current.InList(this);
                        }
                    }
                }
            }
            else
            {
                if (this.ID > 0 && this.EntityState != Enums.EntityState.Add)
                {
                    NHExt.Runtime.Exceptions.RuntimeException ex = new NHExt.Runtime.Exceptions.RuntimeException("后台取orignaldata出错");
                    NHExt.Runtime.Logger.LoggerHelper.Error(ex);
                    throw ex;
                }
            }
        }

        #endregion

        #region 取值和赋值相关操作
        private System.Collections.Hashtable _hashData = new System.Collections.Hashtable();
        protected void setData(string key, object obj)
        {
            if (_hashData.ContainsKey(key))
            {
                this.InitOrignalData();
                _hashData[key] = obj;
            }
            else
            {
                _hashData.Add(key, obj);
            }
        }
        private T getData<T>(string key)
        {
            if (!_hashData.ContainsKey(key))
            {
                return default(T);
            }
            if (typeof(T).IsValueType)
            {
                return (T)_hashData[key];
            }
            else
            {
                Converter<object, T> converter = new Converter<object, T>(this.refrenceConvert<T>);
                return converter(this._hashData[key]);
            }
        }

        private IList<T> getDataList<T>(string key)
        {
            if (!_hashData.ContainsKey(key))
            {
                return new List<T>();
            }

            Converter<object, IList<T>> converter = new Converter<object, IList<T>>(this.refrenceConvert<IList<T>>);
            return converter(this._hashData[key]);

        }
        private T refrenceConvert<T>(object obj)
        {
            if (Object.ReferenceEquals(obj, null))
            {
                return default(T);
            }
            return (T)obj;
        }
        /// <summary>
        /// 获取object类型参数
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public virtual object GetData(string key)
        {
            string[] keyArray = key.Split(new string[] { "." }, StringSplitOptions.None);
            if (keyArray.Length == 1) return this.getData(key);
            BaseEntity entityObj = this.getData(keyArray[0]) as BaseEntity;
            for (int i = 1; i < keyArray.Length - 1; i++)
            {
                if (entityObj == null) return null;
                entityObj = entityObj.GetData(keyArray[i]) as BaseEntity;
            }
            if (entityObj != null)
            {
                return entityObj.getData(keyArray[keyArray.Length - 1]);
            }
            else
            {
                return null;
            }

        }
        private object getData(string key)
        {
            object obj = null;
            if (this._hashData != null && _hashData.ContainsKey(key))
            {
                obj = _hashData[key];
            }
            else
            {
                //通过反射方式来进行获取
                PropertyInfo pi = this.GetType().GetProperty(key);
                if (pi != null)
                {
                    obj = pi.GetValue(this, null);
                }
            }
            if (obj is BaseEnum)
            {
                return (obj as BaseEnum).EnumValue;
            }
            else if (obj is NHibernate.Collection.PersistentBag)
            {
                NHibernate.Collection.PersistentBag cursor = obj as NHibernate.Collection.PersistentBag;
                {
                    List<NHExt.Runtime.Model.BaseEntity> beList = new List<BaseEntity>();
                    foreach (BaseEntity be in cursor)
                    {
                        beList.Add(be);
                    }
                    return beList;
                }
            }

            return obj;
        }



        /// <summary>
        /// 获取当前发生修改的字段
        /// </summary>
        /// <returns></returns>
        public virtual List<string> GetChangedAttribute()
        {
            List<string> attr = new List<string>();
            foreach (string key in this._hashData.Keys)
            {
                object obj = this.GetData(key);
                object oriObj = this.OrignalData.GetData(key);
                if (obj == null && oriObj == null)
                {
                    continue;
                }
                if (obj == null && oriObj != null || oriObj == null && obj != null)
                {
                    attr.Add(key);
                }
                if (obj is System.Collections.IEnumerable)
                {
                    continue;
                }
                else if (obj is BaseEntity)
                {
                    BaseEntity be = obj as BaseEntity;
                    BaseEntity oriBE = oriObj as BaseEntity;
                    if (be.ID != oriBE.ID)
                    {
                        attr.Add(key);
                    }
                }
                else if (!obj.Equals(oriObj) && obj != oriObj)
                {
                    attr.Add(key);
                }
            }
            return attr;
        }
        #endregion

        #region 重写函数
        public override bool Equals(object obj)
        {
            if (object.ReferenceEquals(obj, null))
            {
                return false;
            }
            BaseEntity be = obj as BaseEntity;
            if (be == null)
            {
                return false;
            }
            if (this.EntityName != be.EntityName) return false;
            if (this.ID != be.ID) return false;
            return true;
        }

        public static bool operator !=(BaseEntity a, BaseEntity b)
        {
            return !(a == b);
        }
        public static bool operator ==(BaseEntity a, BaseEntity b)
        {
            if (!object.ReferenceEquals(a, null))
            {
                return a.Equals(b);
            }
            else if (!object.ReferenceEquals(b, null))
            {
                return b.Equals(a);
            }
            else
            {
                return true;
            }
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return this.ID.ToString();
        }
        #endregion
    }
}

