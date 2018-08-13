namespace FastData.Core.Base
{
    internal static class Config
    {
        /// <summary>
        /// 表缓存键
        /// </summary>
        public static readonly string TableKey = "FastData.Core.Cache.Table.{0}";

        /// <summary>
        /// 设计模式
        /// </summary>
        public static readonly string CodeFirst = "CodeFirst";

        /// <summary>
        /// 设计模式
        /// </summary>
        public static readonly string DbFirst = "DbFirst";
    }

    internal static class CacheType
    {
        public static readonly string Web = "web";
        public static readonly string Redis = "redis";
    }

    internal static class SqlErrorType
    {
        public static readonly string Db = "db";
        public static readonly string File = "file";
    }
}
