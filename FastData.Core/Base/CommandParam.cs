namespace FastData.Core.Base
{
    /// <summary>
    /// 标签：2015.9.6，魏中针
    /// 说明：Command操作类
    /// </summary>
    internal static class CommandParam
    {
        #region oracle 批量参数
        /// <summary>
        /// oracle 批量参数
        /// </summary>
        //public static bool GetCmdParam<T>(DataTable dt, List<OracleColumn> list,ref string sql, ref Object cmd, bool IsOutError = false,bool IsCache=true, bool IsAsync = false) where T : new()
        //{
        //    try
        //    {
        //        string[] pName = null;
        //        var colCount = ColumnCount<T>(ref sql,ref pName,IsCache);
                
        //        for (int i = 0; i < colCount;i++ )
        //        {
        //            var param = new OracleParameter(pName[i], GetOracleDbType(list, pName[i]));
        //            object[] pValue = new object[dt.Rows.Count];

        //            for (int j = 0; j < dt.Rows.Count; j++)
        //            {
        //                var itemValue = dt.Rows[j][i];

        //                if (itemValue == null)
        //                    itemValue = DBNull.Value;

        //                pValue[j] = itemValue;
        //            }

        //            param.Value = pValue;
        //            (cmd as OracleCommand).Parameters.Add(param);
        //        }

        //        (cmd as OracleCommand).CommandText = sql;
        //        (cmd as OracleCommand).BindByName = true;
        //        return true;
        //    }
        //    catch (Exception ex)
        //    {
        //        DbLog.LogException<T>(IsOutError, IsAsync, DataDbType.Oracle, ex, "GetCmdParam<T>");
        //        return false;
        //    }
        //}
        #endregion

        #region 获取列数及SQL
        /// <summary>
        /// 获取列数及SQL
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        //private static int ColumnCount<T>(ref string sql,ref string[] pName,bool isCache)
        //{
        //    var sb = new StringBuilder();
        //    int count = 0;
        //    var pInfo = PropertyCache.GetPropertyInfo<T>(isCache);

        //    sb.AppendFormat("insert into {0} values(", typeof(T).Name);
        //    pName = new string[pInfo.Count];

        //    foreach(var p in pInfo)
        //    {
        //        sb.AppendFormat(":{0},",p.Name);
        //        pName[count] = p.Name;
        //        count++;
        //    }

        //    sql= sb.Append(")").ToString().Replace(",)", ")");

        //    return count;
        //}
        #endregion                

        #region 获取列类型
        /// <summary>
        /// 获取列类型
        /// </summary>
        /// <param name="list"></param>
        /// <param name="col"></param>
        /// <returns></returns>
        //private static OracleDbType GetOracleDbType(List<OracleColumn> list,string col)
        //{
        //    var item = list.Find(a => a.colName == col);

        //    switch (item.colType.ToUpper().Trim())
        //    {
        //        case "BFILE":
        //            return OracleDbType.BFile;
        //        case "REAL":
        //            return OracleDbType.Double;
        //        case "LONG":
        //            return OracleDbType.Long;
        //        case "DATE":
        //            return OracleDbType.Date;
        //        case "NUMBER":
        //            return OracleDbType.Decimal;
        //        case "VARCHAR2":
        //            return OracleDbType.Varchar2;
        //        case "NVARCHAR2":
        //            return OracleDbType.NVarchar2;
        //        case "RAW":
        //            return OracleDbType.Raw;
        //        case "DECIMAL":
        //            return OracleDbType.Decimal;
        //        case "INTEGER":
        //            return OracleDbType.Int32;
        //        case "CHAR":
        //            return OracleDbType.Char;
        //        case "NCHAR":
        //            return OracleDbType.NChar;
        //        case "FLOAT":
        //            return OracleDbType.Double;
        //        case "BLOB":
        //            return OracleDbType.Blob;
        //        case "CLOB":
        //            return OracleDbType.Clob;
        //        case "NCLOB":
        //            return OracleDbType.NClob;
        //        default:
        //            return OracleDbType.NVarchar2;
        //    }
        //}
        #endregion
    }
}
