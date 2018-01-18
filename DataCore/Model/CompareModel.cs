using System.Collections.Generic;

namespace Data.Core.Model
{
    /// <summary>
    /// 实体对比返回结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class CompareModel<T> where T : class, new()
    {
        private T _item = new T();
        private List<string> _removeKey = new List<string>();
        private List<ColumnType> _addKey = new List<ColumnType>();
        private List<ColumnType> _removeNull = new List<ColumnType>();
        private List<ColumnType> _addNull = new List<ColumnType>();
        private List<string> _removeName = new List<string>();
        private List<ColumnType> _addName = new List<ColumnType>();
        private List<ColumnComments> _comments = new List<ColumnComments>();
        private List<ColumnType> _type = new List<ColumnType>();

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
        public T Item
        {
            set { _item = value; }
            get { return _item; }
        }

        /// <summary>
        /// 移除主键
        /// </summary>
        public List<string> RemoveKey
        {
            set { _removeKey = value; }
            get { return _removeKey; }
        }

        /// <summary>
        /// 新增主键
        /// </summary>
        public List<ColumnType> AddKey
        {
            set { _addKey = value; }
            get { return _addKey; }
        }

        /// <summary>
        /// 移除字段为空
        /// </summary>
        public List<ColumnType> RemoveNull
        {
            set { _removeNull = value; }
            get { return _removeNull; }
        }

        /// <summary>
        /// 新增字段为空
        /// </summary>
        public List<ColumnType> AddNull
        {
            set { _addNull = value; }
            get { return _addNull; }
        }

        /// <summary>
        /// 删除列
        /// </summary>
        public List<string> RemoveName
        {
            set { _removeName = value; }
            get { return _removeName; }
        }

        /// <summary>
        /// 新增列
        /// </summary>
        public List<ColumnType> AddName
        {
            set { _addName = value; }
            get { return _addName; }
        }

        /// <summary>
        /// 备注
        /// </summary>
        public List<ColumnComments> Comments
        {
            set { _comments = value; }
            get { return _comments; }
        }

        /// <summary>
        /// 列类型
        /// </summary>
        public List<ColumnType> Type
        {
            set { _type = value; }
            get { return _type; }
        }
    }
}
