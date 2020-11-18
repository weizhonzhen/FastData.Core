namespace FastRedis.Core
{
    /// <summary>
    /// 配置实体
    /// </summary>
    public class ConfigModel
    {
        /// <summary>
        /// 服务器
        /// </summary>
        public string Server { get; set; }

        public int Db { get; set; } = 0;
    }
}
