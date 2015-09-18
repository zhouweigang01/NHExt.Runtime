using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;

namespace NHExt.Runtime.Proxy
{
    class ThreadPoolItem
    {
        public System.Threading.WaitCallback CallBack { get; set; }
        public object State { get; set; }
        public bool AutoRun { get; set; }
        public event System.Threading.WaitCallback Complete;
        public event System.Threading.WaitCallback Error;

        public void Run()
        {
            try
            {
                NHExt.Runtime.Logger.LoggerHelper.Debug("线程池操作，线程执行开始");
                System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                sw.Start();
                this.CallBack(this.State);
                sw.Stop();
                NHExt.Runtime.Logger.LoggerHelper.Debug("线程池操作，线程执行结束,耗费时间：" + sw.ElapsedMilliseconds);
                if (this.Complete != null)
                {
                    this.Complete(this);
                }
            }
            catch (Exception ex)
            {
                NHExt.Runtime.Logger.LoggerHelper.Error("线程池操作，线程执行发生错误");
                NHExt.Runtime.Logger.LoggerHelper.Error(ex);
                if (this.Error != null)
                {
                    this.Error(this);
                }
            }
        }
    }
    public class TaskThreadPool
    {
        private static object lockObject = new object();
        private static TaskThreadPool _threadPoolManager = null;
        private bool CanRun
        {
            get
            {
                return this.WorkingCount < this.MaxCount;
            }
        }

        public int WorkingCount { get; set; }
        public int MaxCount { get; set; }

        private Queue<ThreadPoolItem> _waitThreadQueue = null;

        private TaskThreadPool()
        {
            _waitThreadQueue = new Queue<ThreadPoolItem>();
            this.MaxCount = Cfg.MaxThreadCount;
            this.WorkingCount = 1;
        }

        public static TaskThreadPool ThreadPool
        {
            get
            {
                lock (TaskThreadPool.lockObject)
                {
                    if (TaskThreadPool._threadPoolManager == null)
                    {
                        TaskThreadPool._threadPoolManager = new TaskThreadPool();
                    }
                }
                return TaskThreadPool._threadPoolManager;
            }

        }
        /// <summary>
        /// 新增工作调度
        /// </summary>
        /// <param name="callBack">回调函数</param>
        /// <param name="state">调用参数</param>
        /// <param name="autoRun">是否立即执行</param>
        public void AddThreadItem(System.Threading.WaitCallback callBack, object state, bool autoRun)
        {
            lock (TaskThreadPool.lockObject)
            {
                ThreadPoolItem item = new ThreadPoolItem() { CallBack = callBack, State = state, AutoRun = autoRun };
                item.Complete += new System.Threading.WaitCallback((st) =>
                {
                    lock (TaskThreadPool.lockObject)
                    {
                        this.WorkingCount--;
                    }
                    ThreadPoolItem tpi = st as ThreadPoolItem;
                    if (tpi != null && !tpi.AutoRun)
                    {
                        if (this._waitThreadQueue.Count > 0)
                        {
                            ThreadPoolItem queueItem = this._waitThreadQueue.Dequeue();
                            this.runThreadItem(queueItem);
                        }
                    }

                });
                item.Error += new System.Threading.WaitCallback((st) =>
                {
                    lock (TaskThreadPool.lockObject)
                    {
                        this.WorkingCount--;
                    }
                    ThreadPoolItem tpi = st as ThreadPoolItem;
                    if (tpi != null && !tpi.AutoRun)
                    {
                        if (this._waitThreadQueue.Count > 0)
                        {
                            ThreadPoolItem queueItem = this._waitThreadQueue.Dequeue();
                            this.runThreadItem(queueItem);
                        }
                    }
                });
                //如果直接运行，交由线程池去维护
                if (autoRun && this.CanRun)
                {
                    System.Threading.ThreadPool.QueueUserWorkItem((st) =>
                    {
                        ThreadPoolItem tpi = st as ThreadPoolItem;
                        if (tpi != null)
                        {
                            tpi.Run();
                        }
                    }, item);
                }
                else //交由线程队列去维护
                {

                    if (!this.CanRun)
                    {
                        this._waitThreadQueue.Enqueue(item);
                    }
                    else
                    {
                        this.runThreadItem(item);
                    }
                }
            }
        }

        private void runThreadItem(ThreadPoolItem item)
        {
            lock (TaskThreadPool.lockObject)
            {
                this.WorkingCount++;
            }
            System.Threading.ThreadPool.QueueUserWorkItem((st) =>
            {
                ThreadPoolItem tpi = st as ThreadPoolItem;
                if (tpi != null)
                {
                    tpi.Run();
                }
            }, item);
        }
    }
}
