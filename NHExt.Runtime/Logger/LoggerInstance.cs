using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace NHExt.Runtime.Logger
{
    public static class LoggerInstance
    {
        private static ILog _runtimeLogger = null;
        /// <summary>
        /// 运行框架日志
        /// </summary>
        public static ILog RuntimeLogger {
            get {
                if (_runtimeLogger == null) {
                    _runtimeLogger = getLogger("RUNTIME");
                }
                return _runtimeLogger;
            }
        }
        private static ILog _appLogger = null;
        /// <summary>
        /// 应用服务日志
        /// </summary>
        public static ILog AppLogger {
            get {
                if (_appLogger == null) {
                    _appLogger = getLogger("APP");
                }
                return _appLogger;
            }
        }
        private static ILog _bizLogger = null;
        /// <summary>
        /// 业务日志
        /// </summary>
        public static ILog BizLogger
        {
            get
            {
                if (_bizLogger == null) {
                    _bizLogger = getLogger("BIZ");
                }
                return LoggerInstance._bizLogger;
            }
        }

        public static ILog getLogger(string key) {
            ILog logger = log4net.LogManager.GetLogger(key);
            if (logger == null) {
                throw new NHExt.Runtime.Exceptions.AppException("日志节点没有配置");
            }
            return logger;
        }
    }
}
