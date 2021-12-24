using FastData.Core.Property;
using FastUntility.Core.Base;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace FastData.Core.Base
{
    /// <summary>
    /// 标签：2015.9.6，魏中针
    /// 说明：Command操作类
    /// </summary>
    internal static class CommandParam
    {
        #region 获取列类型
        /// <summary>
        /// 获取列类型
        /// </summary>
        /// <param name="list"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        public static DbType GetOracleDbType(string type)
        {
            switch (type.ToLower())
            {                
                case "string":
                    return DbType.String;
                case "datetime":
                    return DbType.DateTime;
                case "decimal":
                    return DbType.Decimal;
                case "int32":
                    return  DbType.Decimal;
                case "int64":
                    return DbType.Decimal;
                case "byte[]":
                    return DbType.Byte;
                case "float":
                    return DbType.Double;
                case "double":
                    return DbType.Double;
                default:
                    return DbType.Object;
            }            
        }
        #endregion

        #region tvsps sql
        /// <summary>
        /// tvsps sql
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dyn"></param>
        /// <returns></returns>
        public static string GetTvps<T>()
        {
            var sql1 = new StringBuilder();
            var sql2 = new StringBuilder();

            sql1.AppendFormat("insert into {0} (", typeof(T).Name);
            sql2.Append("select ");

            PropertyCache.GetPropertyInfo<T>().ForEach(a => {
                sql1.AppendFormat("{0},", a.Name);
                sql2.AppendFormat("tb.{0},", a.Name);
            });

            sql1.Append(")");
            sql2.AppendFormat("from @{0} as tb", typeof(T).Name);

            return string.Format("{0}{1}", sql1.ToString().Replace(",)", ") "), sql2.ToString().Replace(",from", " from"));
        }
        #endregion

        #region tvsps sql
        /// <summary>
        /// tvsps sql
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="dyn"></param>
        /// <returns></returns>
        public static string GetTvps(object model)
        {
            var sql1 = new StringBuilder();
            var sql2 = new StringBuilder();

            sql1.AppendFormat("insert into {0} (", model.GetType().Name);
            sql2.Append("select ");

            PropertyCache.GetPropertyInfo(model).ForEach(a => {
                sql1.AppendFormat("{0},", a.Name);
                sql2.AppendFormat("tb.{0},", a.Name);
            });

            sql1.Append(")");
            sql2.AppendFormat("from @{0} as tb", model.GetType().Name);

            return string.Format("{0}{1}", sql1.ToString().Replace(",)", ") "), sql2.ToString().Replace(",from", " from"));
        }
        #endregion

        #region 获取datatabel
        /// <summary>
        /// 获取datatabel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <returns></returns>
        public static DataTable GetTable<T>(DbCommand cmd,List<T> list)
        {
            var dyn = new Property.DynamicGet<T>();
            var dt = new DataTable();
            cmd.CommandText = string.Format("select top 1 * from {0}", typeof(T).Name);
            dt.Load(cmd.ExecuteReader());
            dt.Clear();

            list.ForEach(a => {
                var row = dt.NewRow();
                PropertyCache.GetPropertyInfo<T>().ForEach(p => {
                    row[p.Name] = dyn.GetValue(a, p.Name);
                });
                dt.Rows.Add(row);
            });

            return dt;
        }
        #endregion

        #region 获取datatabel
        /// <summary>
        /// 获取datatabel
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="db"></param>
        /// <returns></returns>
        public static DataTable GetTable(DbCommand cmd, List<object> list, System.Type type)
        {
            var dyn = new Property.DynamicGet(list[0]);
            var dt = new DataTable();
            cmd.CommandText = string.Format("select top 1 * from {0}", type.Name);
            dt.Load(cmd.ExecuteReader());
            dt.Clear();

            list.ForEach(a =>
            {
                var row = dt.NewRow();
                PropertyCache.GetPropertyInfo(list[0]).ForEach(p =>
                {
                    row[p.Name] = dyn.GetValue(a, p.Name);
                });
                dt.Rows.Add(row);
            });

            return dt;
        }
        #endregion

        #region tvps
        /// <summary>
        /// tvps
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        public static void InitTvps<T>(DbCommand cmd)
        {
            InitTvps(cmd, typeof(T));
        }
        #endregion

        #region tvps
        /// <summary>
        /// tvps
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="cmd"></param>
        public static void InitTvps(DbCommand cmd,System.Type type)
        {
            var sql = new StringBuilder();
            cmd.CommandText = string.Format("select a.name,(select top 1 name from sys.systypes c where a.xtype=c.xtype) as type,length,isnullable,prec,scale from syscolumns a where a.id=object_id('{0}') order by a.colid asc", type.Name);
            var dr = cmd.ExecuteReader();
            var dic = BaseJson.DataReaderToDic(dr);
            dr.Close();

            sql.AppendFormat("if not exists(SELECT 1 FROM sys.table_types where name='{0}')", type.Name);
            sql.AppendFormat("CREATE TYPE {0} AS TABLE(", type.Name);

            foreach (var item in dic)
            {
                switch (item.GetValue("type").ToStr())
                {
                    case "char":
                    case "nchar":
                    case "varchar":
                    case "nvarchar":
                    case "varchar2":
                    case "nvarchar2":
                        sql.AppendFormat("[{0}] [{1}]({2}) {3},", item.GetValue("name"), item.GetValue("type"), item.GetValue("length"), item.GetValue("isnullable").ToStr() == "1" ? "NULL" : "NOT NULL");
                        break;
                    case "decimal":
                    case "numeric":
                    case "number":
                        if (item.GetValue("prec").ToStr() == "0" && item.GetValue("scale").ToStr() == "0")
                            sql.AppendFormat("[{0}] [{1}] {2},", item.GetValue("name"), item.GetValue("type"), item.GetValue("isnullable").ToStr() == "1" ? "NULL" : "NOT NULL");
                        else
                            sql.AppendFormat("[{0}] [{1}]({2},{3}) {4},", item.GetValue("name"), item.GetValue("type"), item.GetValue("prec"), item.GetValue("scale"), item.GetValue("isnullable").ToStr() == "1" ? "NULL" : "NOT NULL");
                        break;
                    default:
                        sql.AppendFormat("[{0}] [{1}] {2},", item.GetValue("name"), item.GetValue("type"), item.GetValue("isnullable").ToStr() == "1" ? "NULL" : "NOT NULL");
                        break;
                }
            }

            sql.Append(")").Replace(",)", ")");
            cmd.CommandText = sql.ToString();
            cmd.ExecuteNonQuery();
        }
        #endregion

        #region mysql 
        /// <summary>
        /// mysql
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string GetMySql<T>(List<T> list)
        {
            var sql = new StringBuilder();
            sql.AppendFormat("insert into {0}(", typeof(T).Name);
            var dyn = new Property.DynamicGet<T>();

            PropertyCache.GetPropertyInfo<T>().ForEach(a => { sql.AppendFormat("{0},", a.Name); });

            sql.Append(")").Replace(",)", ")");

            list.ForEach(a => {
                sql.Append("(");
                PropertyCache.GetPropertyInfo<T>().ForEach(b => { sql.AppendFormat("'{0}',", dyn.GetValue(a, b.Name)); });
                sql.Append("),").Replace(",)", ")");
            });

            return sql.ToStr().Substring(0, sql.ToStr().Length - 1);
        }
        #endregion

        #region mysql 
        /// <summary>
        /// mysql
        /// </summary>
        /// <param name="list"></param>
        /// <returns></returns>
        public static string GetMySql(List<object> list)
        {
            var sql = new StringBuilder();
            sql.AppendFormat("insert into {0}(", list[0].GetType().Name);
            var dyn = new Property.DynamicGet(list[0]);

            PropertyCache.GetPropertyInfo(list[0]).ForEach(a => { sql.AppendFormat("{0},", a.Name); });

            sql.Append(")").Replace(",)", ")");

            list.ForEach(a =>
            {
                sql.Append("(");
                PropertyCache.GetPropertyInfo(list[0]).ForEach(b => { sql.AppendFormat("'{0}',", dyn.GetValue(a, b.Name)); });
                sql.Append("),").Replace(",)", ")");
            });

            return sql.ToStr().Substring(0, sql.ToStr().Length - 1);
        }
        #endregion
    }
}
