using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using log4net;

namespace NHExt.Runtime.Logger
{
    public static class LoggerHelper
    {
        public static void Info(string msg, bool console, ILog logger = null)
        {
            if (console)
            {
                Console.WriteLine(msg);
            }
            if (logger != null)
            {
                logger.Info(msg);
            }
            else
            {
                NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.Info(msg);
            }
        }
        public static void Info(string msg, ILog logger = null)
        {
            if (logger == null || logger.IsInfoEnabled)
            {
                Console.WriteLine(msg);
            }
            if (logger != null)
            {
                logger.Info(msg);
            }
            else
            {
                NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.Info(msg);
            }
        }
        public static void Info(Exception ex, ILog logger = null)
        {
            if (logger == null || logger.IsInfoEnabled)
            {
                Console.WriteLine(ex.Message);
            }
            if (logger != null)
            {
                logger.Info(ex);
            }
            else
            {
                NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.Info(ex);
            }
        }

        public static void Error(string msg, ILog logger = null)
        {
            if (logger == null || logger.IsErrorEnabled)
            {
                Console.WriteLine(msg);
            }
            if (logger != null)
            {
                logger.Error(msg);
            }
            else
            {
                NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.Error(msg);
            }
        }
        public static void Error(Exception ex, ILog logger = null)
        {
            if (logger == null || logger.IsErrorEnabled)
            {
                Console.WriteLine(ex.Message);
            }
            if (logger != null)
            {
                logger.Error(ex);
            }
            else
            {
                NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.Error(ex);
            }
        }
        public static void Debug(string msg, ILog logger = null)
        {
            if (logger == null || logger.IsDebugEnabled)
            {
                Console.WriteLine(msg);
            }
            if (logger != null)
            {
                logger.Debug(msg);
            }
            else
            {
                NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.Debug(msg);
            }
        }
        public static void Debug(Exception ex, ILog logger = null)
        {
            if (logger == null || logger.IsDebugEnabled)
            {
                Console.WriteLine(ex.Message);
            }
            if (logger != null)
            {
                logger.Debug(ex);
            }
            else
            {
                NHExt.Runtime.Logger.LoggerInstance.RuntimeLogger.Debug(ex);
            }
        }
    }
}
