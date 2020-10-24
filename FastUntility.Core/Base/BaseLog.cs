using System;
using System.Threading.Tasks;
using System.IO;

namespace FastUntility.Core.Base
{
    public static class BaseLog
    {
        #region 写日志
        /// <summary>
        /// 标签：2015.7.13，魏中针
        /// 说明：写日记
        /// </summary>
        /// <param name="StrContent">日志内容</param>
        public static void SaveLog(string logContent, string fileName)
        {
            var path = string.Format("{0}/App_Data/log/{1}/{2}", AppDomain.CurrentDomain.BaseDirectory, DateTime.Now.ToString("yyyy-MM"), DateTime.Now.ToString("yyyy-MM-dd"));

            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            if (fileName == "")
                fileName = string.Format("{0}.txt", DateTime.Now.ToString("yyyy-MM-dd-HH"));
            else
                fileName = string.Format("{0}_{1}.txt", fileName, DateTime.Now.ToString("yyyy-MM-dd-HH"));

            path = string.Format("{0}/{1}", path, fileName);

            if (!File.Exists(path))
                using (var fs = File.Create(path)) { }

            //写日志
            using (var fs = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.ReadWrite))
            {
                using (var m_streamWriter = new StreamWriter(fs))
                {
                    m_streamWriter.BaseStream.Seek(0, SeekOrigin.End);
                    m_streamWriter.WriteLine(string.Format("[{0}]{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"), logContent));
                    m_streamWriter.WriteLine("");
                    m_streamWriter.Flush();
                    m_streamWriter.Close();
                    fs.Close();
                }
            }
        }
        #endregion

        #region 写日志 asy
        /// <summary>
        /// 写日志 asy
        /// </summary>
        /// <param name="logContent">日志内容</param>
        /// <param name="fileName">日志文件名</param>
        /// <param name="headName">文件头名</param>
        /// <param name="IsWrap">是否换行</param>
        /// <param name="logCount">日志重写数量</param>
        public static async void SaveLogAsy(string logContent, string fileName)
        {
            await Task.Factory.StartNew(() =>
            {
                SaveLog(logContent, fileName);
            }).ConfigureAwait(false);
        }
        #endregion
    }
}
