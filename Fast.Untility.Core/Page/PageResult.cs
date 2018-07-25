using System.Collections.Generic;

namespace Fast.Untility.Core.Page
{
    // <summary>
    /// 分页返回实体
    /// </summary>
    public sealed class PageResult<T> where T : class, new()
    {
        private PageModel _pModel = new PageModel();
        private List<T> _list = new List<T>();

        /// <summary>
        /// 分页model
        /// </summary>
        public PageModel pModel
        {
            set { _pModel = value; }
            get { return _pModel; }
        }

        /// <summary>
        /// 分页列表
        /// </summary>
        public List<T> list
        {
            set { _list = value; }
            get { return _list; }
        }
    }


    /// <summary>
    /// 分页返回实体
    /// </summary>
    public class PageResult
    {
        private PageModel _pModel = new PageModel();

        /// <summary>
        /// 分页model
        /// </summary>
        public PageModel pModel
        {
            set { _pModel = value; }
            get { return _pModel; }
        }

        /// <summary>
        /// 分页列表
        /// </summary>
        public List<Dictionary<string, object>> list { get; set; }
    }
}
