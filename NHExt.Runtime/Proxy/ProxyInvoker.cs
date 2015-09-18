using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using NHExt.Runtime.Proxy;

namespace NHExt.Runtime.Proxy
{
    public class ProxyInvoker
    {
        public string DllName { get; set; }
        /// <summary>
        /// 调用服务的guid
        /// </summary>
        public string Guid { get; set; }
        /// <summary>
        /// 命名空间
        /// </summary>
        public string NS { get; set; }
        public Auth.AuthContext Ctx { get; set; }
        public bool? UseReadDB { get; set; }
        public bool IsTask { get; set; }
        public bool AutoRun { get; set; }

        /// <summary>
        /// 选择器名称
        /// </summary>
        public string ProxyName { get; set; }

        /// <summary>
        /// 请求参数
        /// </summary>
        public List<object> ParamList { get; set; }

        /// <summary>
        /// 来源页面
        /// </summary>
        public string SourcePage { get; set; }

        /// <summary>
        /// 请求IP
        /// </summary>
        public string RemoteIP { get; set; }

        /// <summary>
        /// 调用类型
        /// </summary>
        public NHExt.Runtime.Session.CallerTypeEnum CallerType { get; private set; }

        public ProxyInvoker()
        {
            this.CallerType = Session.CallerTypeEnum.None;
            this.ParamList = new List<object>();
        }
        public virtual object Do()
        {
            NHExt.Runtime.Logger.LoggerHelper.Info("开始创建服务工厂");
            AbstractInvoker invoker = InvokerFactory.BuildInvoker(this);
            NHExt.Runtime.Logger.LoggerHelper.Info("创建服务工厂完成");
            ProxyContext ctx = new ProxyContext(this.Ctx);
            //如果缓存中没有存来源单据页面的话则需要重bpproxy里面去取

            NHExt.Runtime.Logger.LoggerHelper.Info("开始调用服务");


            this.CallerType = invoker.CallerType;

            ctx.ProxyGuid = this.Guid;

            if (string.IsNullOrEmpty(this.Ctx.RemoteIP))
            {
                ctx.RemoteIP = this.RemoteIP;
            }
            if (string.IsNullOrEmpty(this.Ctx.SourcePage))
            {
                ctx.SourcePage = this.SourcePage;
            }

            //反射调用的话直接赋值就好了
            if (invoker is ReflectInvoker)
            {
                ctx.ParamList = this.ParamList;
            }
            else
            {//远程调用的话因为涉及到WCF序列化所以先要在本地将对象序列化成xml
                if (ctx.ParamList == null)
                {
                    ctx.ParamList = new List<object>();
                }
                foreach (object obj in this.ParamList)
                {
                    ctx.ParamList.Add(NHExt.Runtime.Serialize.XmlSerialize.Serialize(obj));
                }
                //调用远程WCF的话必须进行数据权限过滤
                ctx.IsDataAuth = true;
            }
            invoker.Ctx = ctx;
            invoker.Ctx.UseReadDB = this.UseReadDB;
            //只有第一层服务才能够用线程来解决
            if (this.IsTask)
            {
                NHExt.Runtime.Logger.LoggerHelper.Info("系统调度任务，使用线程调度服务");
                NHExt.Runtime.Proxy.TaskThreadPool.ThreadPool.AddThreadItem((state) =>
                {
                    AbstractInvoker invokerObj = state as AbstractInvoker;
                    invokerObj.InvokeProxy();
                }, invoker, this.AutoRun);
                return null;
            }
            else
            {
                object obj = invoker.InvokeProxy();
                NHExt.Runtime.Logger.LoggerHelper.Info("调用服务成功");
                return obj;
            }
        }
    }

}
