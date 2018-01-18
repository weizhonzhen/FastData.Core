using System.Collections.Generic;

namespace Data.Core.Model
{
    #region 查询
    /// <summary>
    /// 查询
    /// </summary>
    public sealed class DataQuery
    {
        private List<VisitModel> _predicate = new List<VisitModel>();
        private List<string> _field = new List<string>();
        private List<string> _table = new List<string>();
        private List<string> _asName = new List<string>();
        private List<string> _orderBy = new List<string>();
        private List<string> _groupBy = new List<string>();

        /// <summary>
        /// 条件集
        /// </summary>
        internal List<VisitModel> Predicate
        {
            set { _predicate = value; }
            get { return _predicate; }
        }

        /// <summary>
        /// 排序
        /// </summary>
        internal List<string> OrderBy
        {
            set { _orderBy = value; }
            get { return _orderBy; }
        }

        /// <summary>
        /// group by
        /// </summary>
        internal List<string> GroupBy
        {
            set { _groupBy = value; }
            get { return _groupBy; }
        }

        /// <summary>
        /// 字段集
        /// </summary>
        internal List<string> Field
        {
            set { _field = value; }
            get { return _field; }
        }

        /// <summary>
        /// 前几条
        /// </summary>
        internal int Take { get; set; }
        
        /// <summary>
        /// 数据库键
        /// </summary>
        internal string Key { get; set; }

        /// <summary>
        /// 字段集别名
        /// </summary>
        internal List<string> AsName
        {
            set { _asName = value; }
            get { return _asName; }
        }

        /// <summary>
        /// 表集
        /// </summary>
        internal List<string> Table
        {
            set { _table = value; }
            get { return _table; }
        }

        /// <summary>
        /// 连接配置
        /// </summary>
        internal ConfigModel Config { get; set; }
    }
    #endregion
}
