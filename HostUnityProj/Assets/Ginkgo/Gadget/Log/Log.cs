using System;
using System.Collections.Generic;

namespace Ginkgo
{
    public partial class Log
    {
        public static string CONFIG_FILE_NAME = "log_config";

        public static Logger Exception = new Logger("Exception");
        public static Logger Net = new Logger("Net");
        //for common log.
        public static Logger Common = new Logger("Common");
        public static Logger TimeCost = new Logger("TimeCost");

        private readonly LogInfo[] DEFAULT_LOG_INFOS;

        private static Log s_instance;

        private Dictionary<string, LogInfo> m_logInfos = new Dictionary<string, LogInfo>();

        public static Log Get()
        {
            if (s_instance == null)
            {
                s_instance = new Log();
                s_instance.Initialize();
            }
            return s_instance;
        }

        protected Log()
        {
            DEFAULT_LOG_INFOS = new LogInfo[]
            {
                new LogInfo(){m_name = "Common", m_consolePrinting = true, m_filePrinting = true},
            };
#if UNITY_EDITOR || UNITY_STANDALONE_WIN || UNITY_ANDROID || UNITY_IOS || UNITY_IPHONE
            string logconfig = UnityEngine.Application.persistentDataPath + "/log_config.txt";
            if (System.IO.File.Exists(logconfig))
            {
                CONFIG_FILE_NAME = logconfig;
            }
#endif
        }

        public void Load()
        {
            this.m_logInfos.Clear();
            this.LoadConfig(CONFIG_FILE_NAME);
            LogInfo[] dEFAULT_LOG_INFOS = this.DEFAULT_LOG_INFOS;
            for (int i = 0; i < dEFAULT_LOG_INFOS.Length; i++)
            {
                LogInfo logInfo = dEFAULT_LOG_INFOS[i];
                if (!this.m_logInfos.ContainsKey(logInfo.m_name))
                {
                    this.m_logInfos.Add(logInfo.m_name, logInfo);
                }
            }
        }

        public LogInfo GetLogInfo(string name)
        {
            LogInfo result = null;
            this.m_logInfos.TryGetValue(name, out result);
            return result;
        }

        private void Initialize()
        {
            this.Load();
        }

        private void LoadConfig(string path)
        {
            ConfigFile configFile = new ConfigFile();
            if (!configFile.LightLoad(path))
            {
                return;
            }
            foreach (ConfigFile.Line current in configFile.GetLines())
            {
                string sectionName = current.m_sectionName;
                string lineKey = current.m_lineKey;
                string value = current.m_value;
                LogInfo logInfo;
                if (!this.m_logInfos.TryGetValue(sectionName, out logInfo))
                {
                    logInfo = new LogInfo
                    {
                        m_name = sectionName
                    };
                    this.m_logInfos.Add(logInfo.m_name, logInfo);
                }
                if (lineKey.Equals("ConsolePrinting", StringComparison.OrdinalIgnoreCase))
                {
                    logInfo.m_consolePrinting = GeneralUtils.ForceBool(value);
                }
                else if (lineKey.Equals("ScreenPrinting", StringComparison.OrdinalIgnoreCase))
                {
                    logInfo.m_screenPrinting = GeneralUtils.ForceBool(value);
                }
                else if (lineKey.Equals("FilePrinting", StringComparison.OrdinalIgnoreCase))
                {
                    logInfo.m_filePrinting = GeneralUtils.ForceBool(value);
                }
                else if (lineKey.Equals("MinLevel", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        LogLevel logLevel = EnumUtils.GetEnum<LogLevel>(value, StringComparison.OrdinalIgnoreCase);
                        logInfo.m_minLevel = logLevel;
                    }
                    catch (ArgumentException)
                    {
                    }
                }
                else if (lineKey.Equals("DefaultLevel", StringComparison.OrdinalIgnoreCase))
                {
                    try
                    {
                        LogLevel enum2 = EnumUtils.GetEnum<LogLevel>(value, StringComparison.OrdinalIgnoreCase);
                        logInfo.m_defaultLevel = enum2;
                    }
                    catch (ArgumentException)
                    {
                    }
                }
                else if (lineKey.Equals("Verbose", StringComparison.OrdinalIgnoreCase))
                {
                    logInfo.m_verbose = GeneralUtils.ForceBool(value);
                }
            }
        }
    }
}