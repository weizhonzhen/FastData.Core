using System.Collections.Generic;

namespace FastData.Core.Model
{
    #region 查询
    /// <summary>
    /// 查询
    /// </summary>
    public sealed class DataQuery
    {
        /// <summary>
        /// 条件集
        /// </summary>
        internal List<VisitModel> Predicate { set; get; } = new List<VisitModel>();

        /// <summary>
        /// 排序
        /// </summary>
        internal List<string> OrderBy { set; get; } = new List<string>();

        /// <summary>
        /// group by
        /// </summary>
        internal List<string> GroupBy { set; get; } = new List<string>();

        /// <summary>
        /// 字段集
        /// </summary>
        internal List<string> Field { set; get; } = new List<string>();

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
        internal List<string> AsName { set; get; } = new List<string>();

        /// <summary>
        /// 表集
        /// </summary>
        internal List<string> Table { set; get; } = new List<string>();

        /// <summary>
        /// 连接配置
        /// </summary>
        internal ConfigModel Config { get; set; }

        /// <summary>
        /// 表名
        /// </summary>
        internal List<string> TableName { set; get; } = new List<string>();

        /// <summary>
        /// 过滤
        /// </summary>
        internal bool IsFilter { get; set; } = true;
    }
    #endregion
}
