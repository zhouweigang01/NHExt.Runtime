using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NHibernate;
using NHibernate.Type;
using System.Collections;

namespace NHExt.Runtime.Session
{
    class MyInterceptor : EmptyInterceptor
    {
        public override NHibernate.SqlCommand.SqlString OnPrepareStatement(NHibernate.SqlCommand.SqlString sql)
        {
            NHExt.Runtime.Logger.LoggerHelper.Debug(sql.ToString(), NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            return base.OnPrepareStatement(sql);
        }


        public override void OnDelete(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
#if CONSOLE
            NHExt.Runtime.Model.BaseEntity be = entity as NHExt.Runtime.Model.BaseEntity;
            if (be.EntityState == Enums.EntityState.Delete)
            {
                string stateString = string.Empty;
                if (propertyNames != null && propertyNames.Length > 0)
                {
                    for (int i = 0; i < propertyNames.Length; i++)
                    {
                        stateString += "字段" + propertyNames[i] + ":" + state[i] + ", ";
                    }
                }
                NHExt.Runtime.Logger.LoggerHelper.Debug("删除实体" + id + "实体状态" + stateString, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
                NHExt.Runtime.Logger.LoggerHelper.Debug("删除实体" + id + "实体类型为" + (entity as NHExt.Runtime.Model.BaseEntity).EntityName, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            }
#endif
        }

        public override bool OnFlushDirty(object entity, object id, object[] currentState, object[] previousState, string[] propertyNames
                                          , IType[] types)
        {
#if CONSOLE
            string stateString = string.Empty;
            if (propertyNames != null && propertyNames.Length > 0)
            {
                for (int i = 0; i < propertyNames.Length; i++)
                {
                    stateString += propertyNames[i] + ":" + currentState[i] + "原值" + currentState[i] + ",";
                }
            }

            NHExt.Runtime.Logger.LoggerHelper.Debug("提交实体" + id + "到缓存中,实体状态" + stateString, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            NHExt.Runtime.Logger.LoggerHelper.Debug("提交实体" + id + "到缓存中,实体类型为" + (entity as NHExt.Runtime.Model.BaseEntity).EntityName, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
#endif
            return base.OnFlushDirty(entity, id, currentState, previousState, propertyNames, types);

        }

        public override bool OnLoad(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
#if CONSOLE
            string stateString = string.Empty;
            if (propertyNames != null && propertyNames.Length > 0)
            {
                for (int i = 0; i < propertyNames.Length; i++)
                {
                    stateString += propertyNames[i] + ":" + state[i] + ", ";
                }
            }
            NHExt.Runtime.Logger.LoggerHelper.Debug("加载实体" + id + "到缓存中,实体状态" + stateString, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            NHExt.Runtime.Logger.LoggerHelper.Debug("加载实体" + id + "到缓存中,实体类型" + (entity as NHExt.Runtime.Model.BaseEntity).EntityName, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
#endif
            return base.OnLoad(entity, id, state, propertyNames, types);
        }

        public override bool OnSave(object entity, object id, object[] state, string[] propertyNames, IType[] types)
        {
#if CONSOLE
            string stateString = string.Empty;
            if (propertyNames != null && propertyNames.Length > 0)
            {
                for (int i = 0; i < propertyNames.Length; i++)
                {
                    stateString += propertyNames[i] + ":" + state[i] + ",";
                }
            }
            NHExt.Runtime.Logger.LoggerHelper.Debug("保存实体" + id + "到缓存中,实体状态" + stateString, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            NHExt.Runtime.Logger.LoggerHelper.Debug("保存实体" + id + "到缓存中,实体类型" + (entity as NHExt.Runtime.Model.BaseEntity).EntityName, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
#endif
            return base.OnSave(entity, id, state, propertyNames, types);
        }

        public override void PostFlush(ICollection entities)
        {
#if CONSOLE
            string entityString = string.Empty;
            if (entities != null)
            {
                foreach (NHExt.Runtime.Model.BaseEntity entity in entities)
                {
                    entityString += "实体类型" + entity.EntityName + "实体Key" + entity.ID + ", ";
                }
            }
            NHExt.Runtime.Logger.LoggerHelper.Debug("批量提交缓存成功" + entityString, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            // NHExt.Runtime.Logger.LoggerHelper.Debug("批量提交缓存成功", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
#endif
            base.PostFlush(entities);
        }

        public override void PreFlush(ICollection entities)
        {
#if CONSOLE
            string entityString = string.Empty;
            if (entities != null)
            {
                foreach (NHExt.Runtime.Model.BaseEntity entity in entities)
                {
                    entityString += "实体类型" + entity.EntityName + "实体Key" + entity.ID + ", ";
                }
            }
            NHExt.Runtime.Logger.LoggerHelper.Debug("开始批量提交缓存" + entityString, NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
            // NHExt.Runtime.Logger.LoggerHelper.Debug("开始批量提交缓存", NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger);
#endif
            base.PreFlush(entities);
        }
    }
}
