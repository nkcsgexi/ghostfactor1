using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace warnings.util
{
    public class NLoggerUtil
    {

        private static string desktopPath = @"C:\Users\Xi Ge\Desktop\";

        private static bool toDesktop = true;
        
        static public Logger getNLogger(Type t)
        {
            var config = new LoggingConfiguration();
            string path =  "GhostFactor.log";
            if (toDesktop)
                path = desktopPath + path;

            var target = new FileTarget
            {
                FileName = path
            };

            // Add a file target where all the log shall go to.
            config.AddTarget("file", target);

            // Add a rule that any information higher than debug goes to the file.
            var rule = new LoggingRule("*", LogLevel.Debug, target);
            config.LoggingRules.Add(rule);
            LogManager.Configuration = config;
            return LogManager.GetLogger(t.FullName);
        }
    }
}
