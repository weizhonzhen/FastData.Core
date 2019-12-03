using FastData.Core.Property;
using FastData.Core.Model;
using FastUntility.Core.Base;

namespace FastData.Core.Check
{
    internal static class CheckModel
    {
        #region 比对model
        /// <summary>
        /// 比对model
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="CacheItem">缓存实体</param>
        /// <param name="modelItem">实体</param>
        /// <returns></returns>
        public static CompareModel<T> CompareTo<T>(T CacheItem, T modelItem) where T : class, new()
        {
            var dynGet = new Property.DynamicGet<T>();
            var dynSet = new Property.DynamicSet<T>();
            var result = new CompareModel<T>();

            result.Item = modelItem;

            if (modelItem == null)
            {
                result.RemoveName.Add(dynGet.GetValue(CacheItem, "Name", true).ToStr());
                result.IsDelete = true;
                return result;
            }

            var type = dynGet.GetValue(modelItem, "DataType", true).ToStr();

            if (type == "")
                type = dynGet.GetValue(CacheItem, "DataType", true).ToStr();

            var name = dynGet.GetValue(modelItem, "Name", true).ToStr();

            if (name == "")
            {
                name = dynGet.GetValue(CacheItem, "Name", true).ToStr();
                result.Item = CacheItem;
            }

            foreach (var info in PropertyCache.GetPropertyInfo<T>())
            {
                var modelValue = dynGet.GetValue(modelItem, info.Name, true);
                var cacheValue = dynGet.GetValue(CacheItem, info.Name, true);

                dynSet.SetValue(result.Item, info.Name, modelValue, true);


                if ((modelValue != null && cacheValue != null && modelValue.ToStr().ToLower() != cacheValue.ToStr().ToLower())
                    || (modelValue == null && cacheValue != null) || (modelValue != null && cacheValue == null))
                {
                    result.IsUpdate = true;

                    switch (info.Name)
                    {
                        case "IsKey":
                            {
                                if (!(bool)modelValue)
                                    result.RemoveKey.Add(name);

                                if ((bool)modelValue)
                                    result.AddKey.Add(GetColumnType<T>(modelItem, type, name));
                                break;
                            }
                        case "IsNull":
                            {
                                if (!(bool)modelValue)
                                    result.RemoveNull.Add(GetColumnType<T>(modelItem, type, name));

                                if ((bool)modelValue)
                                    result.AddNull.Add(GetColumnType<T>(modelItem, type, name));
                                break;
                            }
                        case "Name":
                            {
                                if (modelValue == null)
                                    result.RemoveName.Add(name);

                                if (modelValue != null)
                                    result.AddName.Add(GetColumnType<T>(modelItem, type, name));
                                break;
                            }
                        case "Length":
                        case "Precision":
                        case "Scale":
                            {
                                result.Type.Add(GetColumnType<T>(modelItem, type, name));
                                break;
                            }
                        case "DataType":
                            {
                                result.Type.Add(GetColumnType<T>(modelItem, modelValue, name));
                                break;
                            }
                        case "Comments":
                            {
                                result.Comments.Add(new ColumnComments { Comments = modelValue.ToStr(), Name = name, Type = GetColumnType<T>(modelItem, type, name) });
                                break;
                            }
                        default:
                            break;
                    }
                }
            }

            return result;
        }
        #endregion

        #region 获取列类型
        /// <summary>
        /// 获取列类型
        /// </summary>
        /// <returns></returns>
        private static ColumnType GetColumnType<T>(T ModelItem, object ModelVaue, string name)
        {
            var dynGet = new Property.DynamicGet<T>();
            var item = new ColumnType();
            item.Name = name;
            var type = ModelVaue.ToStr().ToLower();

            switch (type)
            {
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                case "varchar2":
                case "nvarchar2":
                    item.Length = dynGet.GetValue(ModelItem, "Length", true).ToStr().ToInt(0);
                    item.Type = type;
                    break;
                case "decimal":
                case "numeric":
                case "number":
                    item.Precision = dynGet.GetValue(ModelItem, "Precision", true).ToStr().ToInt(0);
                    item.Scale = dynGet.GetValue(ModelItem, "Scale", true).ToStr().ToInt(0);
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
