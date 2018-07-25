namespace Fast.Redis.Core
{
    /// <summary>
    /// 配置实体
    /// </summary>
    internal class ConfigModel
    {
        /// <summary>
        /// 写服务器,各个服务器之间用逗号分开
        /// </summary>
        public string WriteServerList { get; set; }

        /// <summary>
        ///读服务器,各个服务器之间用逗号分开
        /// </summary>
        public string ReadServerList { get; set; }

        /// <summary>
        /// 最大写链接数
        /// </summary>
        public int MaxWritePoolSize { get; set; }

        /// <summary>
        /// 最大读链接数
        /// </summary>
        public int MaxReadPoolSize { get; set; }

        /// <summary>
        /// 自动重启
        /// </summary>
        public bool AutoStart { get; set; }        
    }
}
