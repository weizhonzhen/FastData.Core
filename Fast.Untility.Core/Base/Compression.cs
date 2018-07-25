using System;
using System.IO;
using System.IO.Compression;

namespace Fast.Untility.Core.Base
{
    /// <summary>
    /// 压缩
    /// </summary>
    public class Compression
    {
        #region deflate 压缩
        /// <summary>
        /// deflate 压缩
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] DeflateByte(byte[] data)
        {
            if (data == null || data.Length < 1)
                return data;
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var gZipStream = new DeflateStream(stream, CompressionMode.Compress))
                    {
                        gZipStream.Write(data, 0, data.Length);
                        gZipStream.Close();
                    }

                    return stream.ToArray();
                }
            }
            catch (Exception)
            {
                return data;
            }
        }
        #endregion

        #region gzip 压缩
        /// <summary>
        /// gzip 压缩
        /// </summary>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte[] GzipByte(byte[] data)
        {
            if (data == null || data.Length < 1)
                return data;
            try
            {
                using (var stream = new MemoryStream())
                {
                    using (var gZipStream = new GZipStream(stream, CompressionMode.Compress))
                    {
                        gZipStream.Write(data, 0, data.Length);
                        gZipStream.Close();
                    }
                    return stream.ToArray();
                }
            }
            catch (Exception)
            {
                return data;
            }
        }
        #endregion
    }
}
