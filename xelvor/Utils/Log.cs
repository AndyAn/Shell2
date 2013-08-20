using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;

namespace xelvor.Utils
{
    public enum DateRangeType
    {
        BeforeTheDate,
        AfterTheDate
    }

    public class Log
    {
        private static Log lsvc = null;
        private static object locker = new object();
        private static Dictionary<string, List<string>> logSet = new Dictionary<string, List<string>>();
        private static string defaultLogName = "miiror";

        public int MaxActiveRecord { get; set; }
        public string LogPath { get; set; }

        private Log()
        {
            this.MaxActiveRecord = 1;
            this.LogPath = "logs";
            this.CreateLog(defaultLogName);

            if (!Directory.Exists(LogPath))
            {
                Directory.CreateDirectory(LogPath);
            }
        }

        public static Log GetInstance()
        {
            lock (locker)
            {
                if (lsvc == null)
                {
                    lsvc = new Log();
                }

                return lsvc;
            }
        }

        public void CreateLog(string logName)
        {
            if (!logSet.ContainsKey(logName))
            {
                logSet.Add(logName, new List<string>());
            }
        }

        public void WriteLog(string log)
        {
            WriteLog(defaultLogName, log);
        }

        public void WriteLog(string logName, string log)
        {
            lock (locker)
            {
                logSet[logName].Add(DateTime.Now.ToString("yyyyMMddHHmmss") + "\t" + log);

                if (logSet[logName].Count < this.MaxActiveRecord)
                {
                    return;
                }

                using (StreamWriter sw = new StreamWriter(string.Format(@"{0}\{1}_{2}.log", LogPath, logName, DateTime.Now.ToString("yyyyMMdd")), true))
                {
                    sw.Write(string.Join("\r\n", logSet[logName].ToArray()) + "\r\n");
                }

                logSet[logName].Clear();
            }
        }

        public List<string> GetLogs(string logName, DateTime date, DateRangeType dateRangeType)
        {
            List<string> logs = null;
            switch (dateRangeType)
            {
                case DateRangeType.BeforeTheDate:
                    logs = GetLogs(logName, DateTime.Parse("1900-01-01"), date.AddDays(-1));
                    break;
                case DateRangeType.AfterTheDate:
                    logs = GetLogs(logName, date.AddDays(1), DateTime.Now);
                    break;
                default:
                    break;
            }

            return logs;
        }

        public List<string> GetLogs(string logName)
        {
            return GetLogs(logName, DateTime.Parse("1900-01-01"), DateTime.Now);
        }

        public List<string> GetLogs(string logName, DateTime start, DateTime end)
        {
            lock (locker)
            {
                List<string> logs = new List<string>();

                logs.AddRange(logSet[logName]);

                string[] allLogs = Directory.GetFiles(LogPath, logName + "_*.log", SearchOption.AllDirectories);
                foreach (string log in allLogs)
                {
                    logs.AddRange(File.ReadAllLines(log).ToList<string>());
                }

                IEnumerable<string> filteredLogs = logs.Select(p => p).Where(p => DateTime.Parse(p.Substring(0, 14)) >= start).Where(p => DateTime.Parse(p.Substring(0, 14)) <= end);

                return filteredLogs.ToList<string>();
            }
        }
    }
}
