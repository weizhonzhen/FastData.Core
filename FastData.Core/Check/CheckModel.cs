using FastData.Core.Model;
using FastUntility.Core.Base;
using FastData.Core.CacheModel;

namespace FastData.Core.Check
{
    internal static class CheckModel
    {
        #region 比对model
        /// <summary>
        /// 比对model
        /// </summary>
        /// <param name="cacheItem">缓存实体</param>
        /// <param name="modelItem">实体</param>
        /// <returns></returns>
        public static CompareModel<ColumnModel> CompareTo(ColumnModel cacheItem, ColumnModel modelItem)
        {
            var result = new CompareModel<ColumnModel>();
            result.Item = modelItem;

            if (modelItem.Name.ToStr() == "")
            {
                result.RemoveName.Add(cacheItem.Name);
                result.IsDelete = true;
                return result;
            }

            var type = modelItem.DataType.ToStr();
            if (type == "")
                type = cacheItem.DataType.ToStr();

            var name = modelItem.Name.ToStr();
            if (name == "")
            {
                name = cacheItem.Name.ToStr();
                result.Item = cacheItem;
            }

            if (modelItem.IsKey != cacheItem.IsKey)
            {
                result.IsUpdate = true;
                if (modelItem.IsKey)
                    result.AddKey.Add(GetColumnType(modelItem, type, name));
                else
                    result.RemoveKey.Add(name);
            }

            if (modelItem.IsNull != cacheItem.IsNull && !modelItem.IsKey)
            {
                result.IsUpdate = true;
                if (modelItem.IsNull)
                    result.AddNull.Add(GetColumnType(modelItem, type, name));
                else
                    result.RemoveNull.Add(GetColumnType(modelItem, type, name));
            }

            if (modelItem.Name.ToStr().ToLower() != cacheItem.Name.ToStr().ToLower())
            {
                result.IsUpdate = true;
                if (modelItem.Name.ToStr() == "")
                    result.RemoveName.Add(name);
                else
                    result.AddName.Add(GetColumnType(modelItem, type, name));
            }

            if (modelItem.DataType.ToStr().ToLower() != cacheItem.DataType.ToStr().ToLower())
            {
                result.IsUpdate = true;
                result.Type.Add(GetColumnType(modelItem, type, name));
            }
            else
                switch (modelItem.DataType.ToStr().ToLower())
                {
                    case "char":
                    case "nchar":
                    case "varchar":
                    case "nvarchar":
                    case "varchar2":
                    case "nvarchar2":
                        if (modelItem.Length != cacheItem.Length)
                        {
                            result.IsUpdate = true;
                            result.Type.Add(GetColumnType(modelItem, type, name));
                        }
                        break;
                    case "decimal":
                    case "numeric":
                    case "number":
                        if (modelItem.Precision != cacheItem.Precision || modelItem.Scale != cacheItem.Scale)
                        {
                            result.IsUpdate = true;
                            result.Type.Add(GetColumnType(modelItem, type, name));
                        }
                        break;
                }


            if (modelItem.Comments.ToStr() != cacheItem.Comments.ToStr())
            {
                result.IsUpdate = true;
                result.Comments.Add(new ColumnComments { Comments = modelItem.Comments, Name = name, Type = GetColumnType(modelItem, type, name) });
            }
            return result;
        }
        #endregion

        #region 获取列类型
        /// <summary>
        /// 获取列类型
        /// </summary>
        /// <returns></returns>
        private static ColumnType GetColumnType(ColumnModel model, string type, string name)
        {
            var item = new ColumnType();
            item.Name = name;

            switch (type.ToLower())
            {
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                case "varchar2":
                case "nvarchar2":
                    item.Length = model.Length;
                    item.Type = type;
                    break;
                case "decimal":
                case "numeric":
                case "number":
                    item.Precision = model.Precision;
                    item.Scale = model.Scale;
                    item.Type = type;
                    break;
                default:
                    item.Type = type;
                    break;
            }

            return item;
        }
        #endregion
    }
}
