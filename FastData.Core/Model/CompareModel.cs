using System.Collections.Generic;

namespace FastData.Core.Model
{
    /// <summary>
    /// 实体对比返回结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CompareModel<T> where T : class, new()
    {
        /// <summary>
        /// 是否更新
        /// </summary>
        public bool IsUpdate { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool IsDelete { get; set; }

        /// <summary>
        /// 返回最新实体
        /// </summary>
        public T Item { set; get; } = new T();

        /// <summary>
        /// 移除主键
        /// </summary>
        public List<string> RemoveKey { set; get; } = new List<string>();

        /// <summary>
        /// 新增主键
        /// </summary>
        public List<ColumnType> AddKey { set; get; } = new List<ColumnType>();

        /// <summary>
        /// 移除字段为空
        /// </summary>
        public List<ColumnType> RemoveNull { set; get; } = new List<ColumnType>();

        /// <summary>
        /// 新增字段为空
        /// </summary>
        public List<ColumnType> AddNull { set; get; } = new List<ColumnType>();

        /// <summary>
        /// 删除列
        /// </summary>
        public List<string> RemoveName { set; get; } = new List<string>();

        /// <summary>
        /// 新增列
        /// </summary>
        public List<ColumnType> AddName { set; get; } = new List<ColumnType>();

        /// <summary>
        /// 备注
        /// </summary>
        public List<ColumnComments> Comments { set; get; } = new List<ColumnComments>();

        /// <summary>
        /// 列类型
        /// </summary>
        public List<ColumnType> Type { set; get; } = new List<ColumnType>();
    }
}
