using FastData.Core.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using FastData.Core.Base;
using FastData.Core.CacheModel;
using FastData.Core.Property;
using System.Data.Common;
using FastData.Core.Type;
using FastUntility.Core.Base;
using System.Reflection;

namespace FastData.Core.Check
{
    /// <summary>
    /// 验证表
    /// </summary>
    internal static class BaseTable
    {
        #region 验证表
        /// <summary>
        /// 验证表
        /// </summary>
        public static void Check(DataQuery item, string tableName,List<PropertyInfo> listInfo, List<Attribute> listAttribute)
        {
            try
            {
                if (item.Config.DesignModel == FastData.Core.Base.Config.CodeFirst)
                {
                    var table = new TableModel();

                    if (IsExistsTable(item, tableName))
                    {
                        //存在表
                        table = GetTable(item, tableName);
                        var model = PropertyCache.GetAttributesColumnInfo(tableName, listInfo);

                        if (model.Count >= table.Column.Count)
                        {
                            foreach (var temp in model)
                            {
                                var tempSql = new List<string>();
                                var info = table.Column.Find(a => a.Name.ToLower() == temp.Name.ToLower()) ?? new ColumnModel();
                                var result = CheckModel.CompareTo<ColumnModel>(info, temp);
                                if (result.IsUpdate)
                                {
                                    table.Column.Remove(temp);
                                    table.Column.Add(result.Item);
                                    UpdateTable(item, result, tableName);
                                }
                            }
                        }
                        else
                        {
                            var tempColumn = new List<ColumnModel>();
                            tempColumn = table.Column;
                            foreach (var temp in tempColumn)
                            {
                                var tempSql = new List<string>();
                                var info = table.Column.Find(a => a.Name.ToLower() == temp.Name.ToLower()) ?? new ColumnModel();
                                var result = CheckModel.CompareTo<ColumnModel>(temp,info);
                                if (result.IsUpdate)
                                {
                                    table.Column.Remove(temp);
                                    table.Column.Add(result.Item);
                                    UpdateTable(item, result, tableName);
                                }

                                if (result.IsDelete)
                                {
                                    model.Remove(temp);
                                    UpdateTable(item, result, tableName);
                                }
                            }

                            table.Column = model;
                        }

                        var comments = PropertyCache.GetAttributesTableInfo(listAttribute);
                        if (table.Comments != comments)
                        {
                            table.Comments = comments;
                            UpdateComments(item, table.Comments, tableName);
                        }
                    }
                    else
                    {
                        table.Column = PropertyCache.GetAttributesColumnInfo(tableName, listInfo);
                        table.Name = tableName;
                        table.Comments = PropertyCache.GetAttributesTableInfo(listAttribute);
                        AddTable(item, table.Column, tableName);
                        UpdateComments(item, table.Comments, tableName);
                    }
                }
            }
            catch (Exception ex)
            {
                DbLog.LogException(item.Config.IsOutError, item.Config.DbType, ex, string.Format("Check_{0}", tableName), "");
            }
        }
        #endregion

        #region 增加表
        /// <summary>
        /// 增加表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="info"></param>
        private static void AddTable(DataQuery item, List<ColumnModel> info, string tableName)
        {
            var db = BaseContext.GetContext(item.Key);
            var sql = new StringBuilder();
            sql.AppendFormat("create table {0}(", tableName);
            var lastItem = info.Last();
            var key = new List<string>();
            var dataReturn = new DataReturn();

            //create table
            foreach (var temp in info)
            {
                if (temp == lastItem)
                    sql.AppendFormat("{0} {1} {2}", temp.Name, GetFieldType(temp), GetFieldKey(temp));
                else
                    sql.AppendFormat("{0} {1} {2},", temp.Name, GetFieldType(temp), GetFieldKey(temp));

                if (temp.IsKey)
                    key.Add(temp.Name);
            }
            sql.Append(")");
            db.ExecuteSql(sql.ToString(), null, false, item.Config.IsOutSql);

            //主键
            foreach (var temp in key)
            {
                sql = new StringBuilder();
                sql.AppendFormat("alter table {0} add constraint pk_{0}_{1} primary key ({1})", tableName, temp);
                db.ExecuteSql(sql.ToString(), null, false, item.Config.IsOutSql);
            }
            db.Dispose();

            foreach (var temp in info)
            {
                UpdateColumn(item, temp.Name, temp.Comments, GetFieldType(temp), tableName);
            }
        }
        #endregion

        #region 修改表结构
        /// <summary>
        /// 修改表结构
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="info"></param>
        private static void UpdateTable(DataQuery item, CompareModel<ColumnModel> info,  string tableName)
        {
            var db = BaseContext.GetContext(item.Key);

            //add colunm
            foreach (var temp in info.AddName)
            {
                var tempSql = string.Format("alter table {0} add {1} {2}", tableName, temp.Name, GetFieldType(temp));
                db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
            }

            //修改列不为空
            foreach (var temp in info.RemoveNull)
            {
                var tempSql = "";

                //删除主键
                var isKey = CheckKey(item, temp.Name, tableName);
                if (isKey)
                {
                    tempSql = string.Format("alter table {0} drop constraint pk_{0}_{1}", tableName, temp.Name);
                    db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
                }

                if (item.Config.DbType == DataDbType.SqlServer)
                {
                    tempSql = string.Format("alter table {0} alter column {1} {2} not null", tableName, temp.Name, GetFieldType(temp));
                    db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
                }

                if (item.Config.DbType == DataDbType.MySql || item.Config.DbType == DataDbType.Oracle)
                {
                    tempSql = string.Format("alter table {0} modify {1} {2} not null", tableName, temp.Name, GetFieldType(temp));
                    db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
                }

                //增加主键
                if (isKey)
                {
                    tempSql = string.Format("alter table {0} add constraint pk_{0}_{1} primary key ({1})", tableName, temp.Name);
                    db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
                }
            }

            //修改列空
            foreach (var temp in info.AddNull)
            {
                var tempSql = "";

                //删除主键
                var isKey = CheckKey(item, temp.Name, tableName);
                if (isKey)
                {
                    tempSql = string.Format("alter table {0} drop constraint pk_{0}_{1}", tableName, temp.Name);
                    db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
                }

                if (item.Config.DbType == DataDbType.SqlServer)
                {
                    tempSql = string.Format("alter table {0} alter column {1} {2} null", tableName, temp.Name, GetFieldType(temp));
                    db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
                }

                if (item.Config.DbType == DataDbType.MySql || item.Config.DbType == DataDbType.Oracle)
                {
                    tempSql = string.Format("alter table {0} modify {1} {2} null", tableName, temp.Name, GetFieldType(temp));
                    db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
                }


                //增加主键
                if (isKey)
                {
                    tempSql = string.Format("alter table {0} add constraint pk_{0}_{1} primary key ({1})", tableName, temp.Name);
                    db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
                }
            }

            //删除主键
            foreach (var temp in info.RemoveKey)
            {
                var tempSql = string.Format("alter table {0} drop constraint pk_{0}_{1}", tableName, temp);
                db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
            }

            //增加主键
            foreach (var temp in info.AddKey)
            {
                var tempSql = "";
                if (item.Config.DbType == DataDbType.SqlServer)
                {
                    tempSql = string.Format("alter table {0} alter column {1} {2} not null", tableName, temp.Name, GetFieldType(temp));
                    db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
                }

                if (item.Config.DbType == DataDbType.MySql || item.Config.DbType == DataDbType.Oracle)
                {
                    tempSql = string.Format("alter table {0} modify {1} {2} not null", tableName, temp.Name, GetFieldType(temp));
                    db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
                }

                tempSql = string.Format("alter table {0} add constraint pk_{0}_{1} primary key ({1})", tableName, temp.Name);
                db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
            }

            //修改列
            foreach (var temp in info.Type)
            {
                var tempSql = "";

                if (!info.AddName.Exists(a => a.Name == temp.Name))
                {
                    //删除主键
                    var isKey = CheckKey(item, temp.Name, tableName);
                    if (isKey)
                    {
                        tempSql = string.Format("alter table {0} drop constraint pk_{0}_{1}", tableName, temp.Name);
                        db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
                    }

                    if (item.Config.DbType == DataDbType.SqlServer)
                    {
                        tempSql = string.Format("alter table {0} alter column {1} {2}", tableName, temp.Name, GetFieldType(temp));
                        db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
                    }

                    if (item.Config.DbType == DataDbType.MySql || item.Config.DbType == DataDbType.Oracle)
                    {
                        tempSql = string.Format("alter table {0} modify {1} {2}", tableName, temp.Name, GetFieldType(temp));
                        db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
                    }

                    //增加主键
                    if (isKey)
                    {
                        tempSql = string.Format("alter table {0} add constraint pk_{0}_{1} primary key ({1})", tableName, temp.Name);
                        db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
                    }
                }
            }

            //删除列
            foreach (var temp in info.RemoveName)
            {
                var tempSql = string.Format("alter table {0} drop column {1}", tableName, temp);
                db.ExecuteSql(tempSql, null, false, item.Config.IsOutSql);
            }
            db.Dispose();

            //修改列备注
            foreach (var temp in info.Comments)
            {
                UpdateColumn(item, temp.Name, temp.Comments, GetFieldType(temp.Type), tableName);
            }
        }
        #endregion

        #region 修改表备注
        /// <summary>
        /// 修改表备注
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="value"></param>
        private static void UpdateComments(DataQuery item, string value, string tableName)
        {
            var sql = "";
            var db = BaseContext.GetContext(item.Key);

            if (item.Config.DbType == DataDbType.MySql)
                sql = string.Format("alter table {0} comment '{1}'", tableName, value);

            if (item.Config.DbType == DataDbType.Oracle)
                sql = string.Format("Comment on table {0} is '{1}'", tableName, value);

            if (item.Config.DbType == DataDbType.SqlServer)
            {
                sql = string.Format("select count(0) count from sys.extended_properties where object_id('{0}')=major_id and minor_id=0", tableName);
                var count = db.ExecuteSql(sql).DicList[0]["count"].ToStr().ToInt(0);
                if (count >= 1)
                    sql = string.Format("execute sp_updateextendedproperty N'MS_Description', '{0}', N'user', N'dbo', N'table', N'{1}', NULL, NULL", value, tableName);
                else
                    sql = string.Format("execute sp_addextendedproperty N'MS_Description', '{0}', N'user', N'dbo', N'table', N'{1}', NULL, NULL", value, tableName);
            }

            db.ExecuteSql(sql, null, false, item.Config.IsOutSql);
            db.Dispose();
        }
        #endregion

        #region 修改列备注
        /// <summary>
        /// 修改列备注
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <param name="value"></param>
        private static void UpdateColumn(DataQuery item, string name, string value, string type, string tableName)
        {
            var db = BaseContext.GetContext(item.Key);
            var readDb = BaseContext.GetContext(item);
            var sql = "";

            if (item.Config.DbType == DataDbType.MySql)
                sql = string.Format("alter table {0} modify column {1} {2} comment '{3}'", tableName, name, type, value);

            if (item.Config.DbType == DataDbType.Oracle)
                sql = string.Format("Comment on column {0}.{1} is '{2}'", tableName, name, value);

            if (item.Config.DbType == DataDbType.SqlServer)
            {
                sql = string.Format(@"select count(0) count from syscolumns where id = object_id('{0}') and name='{1}'
                                    and exists(select 1 from sys.extended_properties where object_id('{0}')=major_id and colid=minor_id)"
                                      , tableName, name);

                var count = readDb.ExecuteSql(sql).DicList[0]["count"].ToStr().ToInt(0);
                readDb.Dispose();

                if (count >= 1)
                    sql = string.Format("execute sp_updateextendedproperty N'MS_Description', '{0}', N'user', N'dbo', N'table', N'{1}', N'column', {2}", value, tableName, name);
                else
                    sql = string.Format("execute sp_addextendedproperty N'MS_Description', '{0}', N'user', N'dbo', N'table', N'{1}', N'column', {2}", value, tableName, name);
            }

            db.ExecuteSql(sql, null, false, item.Config.IsOutSql);
            db.Dispose();
        }
        #endregion

        #region 获取字段类型
        /// <summary>
        /// 获取字段类型
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static string GetFieldType(ColumnModel item)
        {
            switch (item.DataType.ToLower())
            {
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                case "varchar2":
                case "nvarchar2":
                    return string.Format("{0}({1})", item.DataType, item.Length);
                case "decimal":
                case "numeric":
                case "number":
                    return string.Format("{0}({1},{2})", item.DataType, item.Precision, item.Scale);
                default:
                    return item.DataType;
            }
        }
        #endregion

        #region 获取字段类型
        /// <summary>
        /// 获取字段类型
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static string GetFieldType(ColumnType item)
        {
            switch (item.Type.ToLower())
            {
                case "char":
                case "nchar":
                case "varchar":
                case "nvarchar":
                case "varchar2":
                case "nvarchar2":
                    return string.Format("{0}({1})", item.Type, item.Length);
                case "decimal":
                case "numeric":
                case "number":
                    if (item.Precision == 0 && item.Scale == 0)
                        return item.Type;
                    else
                        return string.Format("decimal({0},{1})", item.Precision, item.Scale);
                default:
                    return item.Type;
            }
        }
        #endregion

        #region 获取字段主键
        /// <summary>
        /// 获取字段类型
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static string GetFieldKey(ColumnModel item)
        {
            if (item.IsKey != null)
            {
                if ((bool)item.IsKey)
                    return "not null";
                else if (item.IsNull != null && !(bool)item.IsNull)
                    return "not null";
                else
                    return "";
            }
            else if (item.IsNull != null && !(bool)item.IsNull)
                return "not null";
            else
                return "";
        }
        #endregion

        #region 查询是否主键
        /// <summary>
        /// 查询是否主键
        /// </summary>
        /// <param name="item"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        private static bool CheckKey(DataQuery item, string name, string tableName)
        {
            var db = BaseContext.GetContext(item);
            var param = new List<DbParameter>();

            var sql = "";
            if (item.Config.DbType == DataDbType.SqlServer)
            {
                var tempParam = DbProviderFactories.GetFactory(item.Config).CreateParameter();
                tempParam.ParameterName = "type";
                tempParam.Value = "pk";
                param.Add(tempParam);

                tempParam = DbProviderFactories.GetFactory(item.Config).CreateParameter();
                tempParam.ParameterName = "name";
                tempParam.Value = string.Format("pk_{0}_{1}", tableName, name);
                param.Add(tempParam);

                sql = "select count(0) count from sysobjects where xtype=@type and name=@name";
            }

            if (item.Config.DbType == DataDbType.Oracle)
            {
                var tempParam = DbProviderFactories.GetFactory(item.Config).CreateParameter();
                tempParam.ParameterName = "name";
                tempParam.Value = string.Format("PK_{0}_{1}", tableName.ToUpper(), name.ToUpper());
                param.Add(tempParam);

                tempParam = DbProviderFactories.GetFactory(item.Config).CreateParameter();
                tempParam.ParameterName = "type";
                tempParam.Value = "P";
                param.Add(tempParam);

                sql = "select count(0) count from user_constraints where constraint_name = :name and constraint_type=:type";
            }

            if (item.Config.DbType == DataDbType.MySql)
            {
                var tempParam = DbProviderFactories.GetFactory(item.Config).CreateParameter();
                tempParam.ParameterName = "name";
                tempParam.Value = string.Format("PK_{0}_{1}", tableName.ToUpper(), name.ToUpper());
                param.Add(tempParam);

                sql = "select count(0) count from information_schema.columns where column_key=?name";
            }

            var count = db.ExecuteSql(sql, param.ToArray(), item.Config.IsOutSql).DicList[0]["count"].ToStr().ToInt(0);
            db.Dispose();
            if (count >= 1)
                return true;
            else
                return false;
        }
        #endregion

        #region 是否存在表
        /// <summary>
        /// 是否存在表
        /// </summary>
        /// <param name="query"></param>
        /// <param name="tableName"></param>
        /// <returns></returns>
        private static bool IsExistsTable(DataQuery query, string tableName)
        {
            var db = BaseContext.GetContext(query.Key);
            var param = new List<DbParameter>();
            var result = false;

            var tempParam = DbProviderFactories.GetFactory(query.Config).CreateParameter();
            tempParam.ParameterName = "name";
            tempParam.Value = tableName.ToUpper();
            param.Add(tempParam);

            if (query.Config.DbType == DataDbType.Oracle)
            {
                var sql = "select count(0) count from user_tables where table_name=:name";
                result = db.ExecuteSql(sql, param.ToArray(), query.Config.IsOutSql).DicList[0].GetValue("count").ToStr().ToInt(0) == 1;
            }

            if (query.Config.DbType == DataDbType.SqlServer)
            {
                var sql = "select count(0) count from dbo.sysobjects where upper(name)=@name";
                result = db.ExecuteSql(sql, param.ToArray(), query.Config.IsOutSql).DicList[0].GetValue("count").ToStr().ToInt(0) == 1;
            }

            if (query.Config.DbType == DataDbType.MySql)
            {
                var sql = "select count(0) count from information_schema.tables where upper(table_name)=?name";
                result = db.ExecuteSql(sql, param.ToArray(), query.Config.IsOutSql).DicList[0].GetValue("count").ToStr().ToInt(0) == 1;
            }

            db.Dispose();
            return result;
        }
        #endregion

        #region 获取表结构
        /// <summary>
        /// 获取表结构
        /// </summary>
        private static TableModel GetTable(DataQuery item, string tableName)
        {
            var result = new TableModel();
            result.Column = result.Column ?? new List<ColumnModel>();

            var db = BaseContext.GetContext(item.Key);
            var param = new List<DbParameter>();

            if (item.Config.DbType == DataDbType.Oracle)
            {
                #region oracle
                //参数
                var tempParam = DbProviderFactories.GetFactory(item.Config).CreateParameter();
                tempParam.ParameterName = "name";
                tempParam.Value = tableName.ToUpper();
                param.Add(tempParam);

                //表
                var sql = "select a.table_name,comments from user_tables a inner join user_tab_comments b on a.TABLE_NAME = b.TABLE_NAME  and a.table_name = :name";
                var dic = db.ExecuteSql(sql, param.ToArray(), item.Config.IsOutSql).DicList[0];

                result.Name = dic.GetValue("table_name").ToStr();
                result.Comments = dic.GetValue("comments").ToStr();

                //列
                sql = string.Format(@"select a.column_name,data_type,data_length,b.comments,
                                     (select count(0) from user_cons_columns aa, user_constraints bb where aa.constraint_name = bb.constraint_name and bb.constraint_type = 'P' and bb.table_name = :name and aa.column_name = a.column_name) iskey, 
                                     nullable,data_precision,data_scale
                                     from user_tab_columns a inner join user_col_comments b
                                     on a.table_name =:name and a.table_name = b.table_name and a.column_name = b.column_name order by a.column_id asc");

                var dicList = db.ExecuteSql(sql, param.ToArray(), item.Config.IsOutSql).DicList;
                foreach (var temp in dicList)
                {
                    var model = new ColumnModel();
                    model.Comments = temp.GetValue("comments").ToStr();
                    model.DataType = temp.GetValue("data_type").ToStr();
                    model.IsKey = temp.GetValue("iskey").ToStr() == "1" ? true : false;
                    model.IsNull = temp.GetValue("nullable").ToStr() == "Y" ? true : false;
                    model.Length = temp.GetValue("data_length").ToStr().ToInt(0);
                    model.Name = temp.GetValue("column_name").ToStr();
                    model.Precision = temp.GetValue("data_precision").ToStr().ToInt(0);
                    model.Scale = temp.GetValue("data_scale").ToStr().ToInt(0);
                    result.Column.Add(model);
                }
                #endregion
            }

            if (item.Config.DbType == DataDbType.MySql)
            {
                #region MySql
                //参数
                var tempParam = DbProviderFactories.GetFactory(item.Config).CreateParameter();
                tempParam.ParameterName = "name";
                tempParam.Value = tableName.ToUpper();
                param.Add(tempParam);

                //表
                var sql = "select table_name,table_comment count from information_schema.tables where upper(table_name)=?name";
                var dic = db.ExecuteSql(sql, param.ToArray(), item.Config.IsOutSql).DicList[0];

                result.Name = dic.GetValue("table_name").ToStr();
                result.Comments = dic.GetValue("table_comment").ToStr();

                //列
                sql = string.Format(@"select column_name,data_type,character_maximum_length,column_comment,
                                     (select count(0) from INFORMATION_SCHEMA.KEY_COLUMN_USAGE a where upper(TABLE_NAME)=?name and constraint_name='PRIMARY' and c.column_name=a.column_name) iskey,
                                      is_nullable,numeric_precision,numeric_scale from information_schema.columns c where upper(table_name)=?name order by ordinal_position asc");

                var dicList = FastRead.ExecuteSql(sql, param.ToArray()) ?? new List<Dictionary<string, object>>();
                foreach (var temp in dicList)
                {
                    var model = new ColumnModel();
                    model.Comments = temp.GetValue("column_comment").ToStr();
                    model.DataType = temp.GetValue("data_type").ToStr();
                    model.IsKey = temp.GetValue("iskey").ToStr() == "1" ? true : false;
                    model.IsNull = temp.GetValue("is_nullabl").ToStr() == "YES" ? true : false;
                    model.Length = temp.GetValue("character_maximum_length").ToStr().ToInt(0);
                    model.Name = temp.GetValue("column_name").ToStr();
                    model.Precision = temp.GetValue("numeric_precision").ToStr().ToInt(0);
                    model.Scale = temp.GetValue("numeric_scale").ToStr().ToInt(0);
                    result.Column.Add(model);
                }
                #endregion
            }

            if (item.Config.DbType == DataDbType.SqlServer)
            {
                #region SqlServer
                //参数
                var tempParam = DbProviderFactories.GetFactory(item.Config).CreateParameter();
                tempParam.ParameterName = "name";
                tempParam.Value = tableName.ToUpper();
                param.Add(tempParam);

                //表
                var sql = "select name,(select top 1 value from sys.extended_properties where major_id=object_id(a.name) and minor_id=0) as value from sys.objects a where type = 'U'and upper(name) = @name";
                var dic = db.ExecuteSql(sql, param.ToArray(), item.Config.IsOutSql).DicList[0];

                result.Name = dic.GetValue("name").ToStr();
                result.Comments = dic.GetValue("value").ToStr();

                //列
                sql = string.Format(@"select a.name,(select top 1 name from sys.systypes c where a.xtype=c.xtype) as type ,
                                    length,b.value,(select count(0) from INFORMATION_SCHEMA.KEY_COLUMN_USAGE where TABLE_NAME='@name' and COLUMN_NAME=a.name) as iskey,
                                    isnullable,prec,scale
                                    from syscolumns a left join sys.extended_properties b 
                                    on major_id = id and minor_id = colid and b.name ='MS_Description' 
                                    where a.id=object_id('@name') order by a.colid asc");

                var dicList = db.ExecuteSql(sql, param.ToArray(), item.Config.IsOutSql).DicList;

                foreach (var temp in dicList)
                {
                    var model = new ColumnModel();
                    model.Comments = temp.GetValue("value").ToStr();
                    model.DataType = temp.GetValue("type").ToStr();
                    model.IsKey = temp.GetValue("iskey").ToStr() == "1" ? true : false;
                    model.IsNull = temp.GetValue("isnullable").ToStr() == "1" ? true : false;
                    model.Length = temp.GetValue("length").ToStr().ToInt(0);
                    model.Name = temp.GetValue("name").ToStr();
                    model.Precision = temp.GetValue("prec").ToStr().ToInt(0);
                    model.Scale = temp.GetValue("scale").ToStr().ToInt(0);
                    result.Column.Add(model);
                }
                #endregion
            }

            db.Dispose();
            return result;
        }
        #endregion
    }
}
