using System.Collections.Generic;
using FastUntility.Core.Page;

namespace FastData.Core.Model
{
    /// <summary>
    /// 返回操作数据结果
    /// </summary>
    public sealed class DataReturn<T> where T : class,new()
    {
        private T _item = new T();
        private List<T> _list = new List<T>();
        private PageResult<T> _page = new PageResult<T>();
        private WriteReturn _writeReturn = new WriteReturn();

        /// <summary>
        /// 条数
        /// </summary>
        public int count { get; set; }

        /// <summary>
        /// 实体
        /// </summary>
        public T item 
        {
            set { _item = value; }
            get { return _item; }
        }

        /// <summary>
        /// 列表
        /// </summary>
        public List<T> list
        {
            set { _list = value; }
            get { return _list; }
        }
        
        /// <summary>
        /// sql
        /// </summary>
        public string sql { get; set; }
        
        /// <summary>
        /// 分页
        /// </summary>
        public PageResult<T> pageResult
        {
            set { _page = value; }
            get { return _page; }
        }

        /// <summary>
        /// 写返回结果
        /// </summary>
        public WriteReturn writeReturn
        {
            set { _writeReturn = value; }
            get { return _writeReturn; }
        }
    }

     /// <summary>
    /// 返回操作数据结果
    /// </summary>
    public class DataReturn
    {
        private PageResult _pageResult = new PageResult();
        private WriteReturn _writeReturn = new WriteReturn();

        /// <summary>
        /// 条数
        /// </summary>
        public int Count { get; set; }
        
        /// <summary>
        /// sql
        /// </summary>
        public string Sql { get; set; }

        /// <summary>
        /// json
        /// </summary>
        public string Json { get; set; }
        
        /// <summary>
        /// dic list
        /// </summary>
        public List<Dictionary<string, object>> DicList { get; set; }

        /// <summary>
        /// dic item
        /// </summary>
        public Dictionary<string, object> Dic { get; set; }

        /// <summary>
        /// 分页
        /// </summary>
        public PageResult PageResult
        {
            set { _pageResult = value; }
            get { return _pageResult; }
        }

        /// <summary>
        /// 写返回结果
        /// </summary>
        public WriteReturn writeReturn
        {
            set { _writeReturn = value; }
            get { return _writeReturn; }
        }
    }

    /// <summary>
    /// 写返回结果
    /// </summary>
    public class WriteReturn
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 是否出错
        /// </summary>
        public bool IsError { get; set; }
    }
}
