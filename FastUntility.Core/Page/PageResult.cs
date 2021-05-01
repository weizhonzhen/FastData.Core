using System.Collections.Generic;

namespace FastUntility.Core.Page
{
    // <summary>
    /// 分页返回实体
    /// </summary>
    public sealed class PageResult<T> where T : class, new()
    {
        /// <summary>
        /// 分页model
        /// </summary>
        public PageModel pModel = new PageModel();

        /// <summary>
        /// 分页列表
        /// </summary>
        public List<T> list = new List<T>();
    }


    /// <summary>
    /// 分页返回实体
    /// </summary>
    public class PageResult
    {
        /// <summary>
        /// 分页model
        /// </summary>
        public PageModel pModel = new PageModel();

        /// <summary>
        /// 分页列表
        /// </summary>
        public List<Dictionary<string, object>> list { get; set; }
    }
}
