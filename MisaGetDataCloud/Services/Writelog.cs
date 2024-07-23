using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MisaGetDataCloud.Services
{
    public static class Writelog
    {
        private static string RootPathLog = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)?.Replace("file:\\", "") + "\\LogError.txt";
        private static string RootPathLogInfo = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().GetName().CodeBase)?.Replace("file:\\", "") + "\\LogInfo.txt";

        /// <summary>
        /// hàm ghi log lỗi
        /// </summary>
        /// <param name="strLog">log cần ghi</param>
        public static void WriteLogError(string strLog)
        {
            File.AppendAllText(RootPathLog, DateTime.Now.ToString() + " : " + strLog + "\n");
        }

        /// <summary>
        /// hàm ghi log Success
        /// </summary>
        /// <param name="strLog">log cần ghi</param>
        public static void WriteLogInfo(string strLog)
        {
            File.AppendAllText(RootPathLogInfo, DateTime.Now.ToString() + " : " + strLog + "\n");
        }
    }
}
