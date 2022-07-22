using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Threading.Tasks;
using FastUntility.Core.Base;
using FastData.Core.Type;
using FastData.Core.Model;
using System.Data;
using System.Linq;

namespace FastData.Core.Base
{
    // <summary>
    /// 标签：2015.9.6，魏中针
    /// 说明：lambda表达式解析
    /// </summary>
    internal static class VisitExpression
    {
        #region Lambda where
        /// <summary>
        /// Lambda where
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <param name="Param"></param>
        /// <returns></returns>
        public static VisitModel LambdaWhere<T>(Expression<Func<T, bool>> item, DataQuery query)
        {
            var result = new VisitModel();
            var strType = "";
            int i = 0;

            var leftList = new List<string>();
            var rightList = new List<string>();
            var typeList = new List<System.Type>();
            var sb = new StringBuilder();

            try
            {
                if (item == null)
                    return result;

                result.Where = RouteExpressionHandler(query, item.Body, ExpressionType.Goto, ref leftList, ref rightList, ref typeList, ref sb, ref strType, ref i);

                result.Where = Remove(result.Where);

                for (i = 0; i < leftList.Count; i++)
                {
                    var temp = DbProviderFactories.GetFactory(query.Config).CreateParameter();
                    temp.ParameterName = leftList[i] + i.ToString();
                    temp.Value = rightList[i];

                    if (typeList.Count >= i + 1 && typeList[i].Name == "DateTime")
                    {
                        if (query.Config.DbType == DataDbType.Oracle)
                            temp.DbType = DbType.Date;
                        else
                            temp.DbType = DbType.DateTime;

                        temp.Value = rightList[i].ToDate();
                    }

                    result.Param.Add(temp);
                }

                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                if (string.Compare(query.Config.SqlErrorType, SqlErrorType.Db, true) ==0)
                    DbLogTable.LogException<T>(query.Config, ex, "LambdaWhere<T>", "");
                else
                    DbLog.LogException<T>(query.Config.IsOutError, query.Config.DbType, ex, "LambdaWhere<T>", "");

                result.IsSuccess = false;
                return result;
            }
        }
        #endregion

        #region Lambda where 2个表
        /// <summary>
        /// Lambda where 2个表
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        public static VisitModel LambdaWhere<T1, T2>(Expression<Func<T1, T2, bool>> item, DataQuery query, bool isPage = false)
        {
            var result = new VisitModel();
            int i = 0;
            string strType = "";

            var leftList = new List<string>();
            var rightList = new List<string>();
            var fieldList = new List<string>();
            var typeList = new List<System.Type>();
            var sb = new StringBuilder();

            try
            {
                if (item == null)
                    return result;

                result.Where = RouteExpressionHandler(query, item.Body, ExpressionType.Goto, ref leftList, ref rightList, ref typeList, ref sb, ref strType, ref i);

                result.Where = Remove(result.Where);

                for (i = 0; i < leftList.Count; i++)
                {
                    var temp = DbProviderFactories.GetFactory(query.Config).CreateParameter();
                    temp.ParameterName = leftList[i] + i.ToString();
                    temp.Value = rightList[i];

                    if (typeList.Count >= i + 1 && typeList[i].Name == "DateTime")
                    {
                        if (query.Config.DbType == DataDbType.Oracle)
                            temp.DbType = DbType.Date;
                        else
                            temp.DbType = DbType.DateTime;

                        temp.Value = rightList[i].ToDate();
                    }

                    result.Param.Add(temp);
                }

                result.IsSuccess = true;
                return result;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    if (string.Compare(query.Config.SqlErrorType, SqlErrorType.Db, true) ==0)
                        DbLogTable.LogException(query.Config, ex, "LambdaWhere<T1, T2>", "");
                    else
                        DbLog.LogException(query.Config.IsOutError, query.Config.DbType, ex, "LambdaWhere<T1, T2>", "");
                });
                result.IsSuccess = false;
                return result;
            }
        }
        #endregion

        #region 解析表达式
        /// <summary>
        /// 解析表达式
        /// </summary>
        /// <param name="exp"></param>
        /// <param name="isRight"></param>
        /// <returns></returns>
        private static string RouteExpressionHandler(DataQuery query, Expression exp, ExpressionType expType, ref List<string> leftList, ref List<string> rightList, ref List<System.Type> typeList, ref StringBuilder sb, ref string strType, ref int i, bool isRight = false)
        {
            var isReturnNull = false;
            if (exp is BinaryExpression)
            {
                BinaryExpression be = (BinaryExpression)exp;

                return BinaryExpressionHandler(query, be.Left, be.Right, be.NodeType, ref leftList, ref rightList, ref typeList, ref sb, ref strType, ref i, isRight);
            }
            else if (exp is MemberExpression)
            {
                if ((exp as MemberExpression).Expression is ParameterExpression)
                    return (exp as MemberExpression).Member.Name;
                else
                {
                    if (Expression.Lambda(exp).Compile().DynamicInvoke() == null)
                        typeList.Add("".GetType());
                    else
                        typeList.Add(Expression.Lambda(exp).Compile().DynamicInvoke().GetType());
                    return Expression.Lambda(exp).Compile().DynamicInvoke().ToStr();
                }
            }
            else if (exp is NewArrayExpression)
            {
                NewArrayExpression naExp = (NewArrayExpression)exp;
                StringBuilder sbArray = new StringBuilder();
                foreach (Expression expression in naExp.Expressions)
                {
                    sbArray.AppendFormat(",{0}", RouteExpressionHandler(query, expression, expType, ref leftList, ref rightList, ref typeList, ref sb, ref strType, ref i, isRight));
                }

                return sbArray.Length == 0 ? "" : sbArray.Remove(0, 1).ToString();
            }
            else if (exp is MethodCallExpression)
            {
                if (isRight)
                {
                    typeList.Add(Expression.Lambda(exp).Compile().DynamicInvoke().GetType());
                    return Expression.Lambda(exp).Compile().DynamicInvoke().ToStr();
                }
                else
                {
                    isRight = false;
                    try
                    {
                        var asName = "";
                        var meExp = (MethodCallExpression)(exp.ReduceExtensions().Reduce());

                        #region 表别名
                        if (meExp.Object != null)
                        {                            
                            if (meExp.Object is MemberExpression && (meExp.Object as MemberExpression).Expression is ParameterExpression)
                                asName = string.Format("{0}.", query.TableAsName.GetValue((meExp.Object as MemberExpression).Expression.Type.Name));
                            else if (meExp.Object is UnaryExpression)
                                asName = string.Format("{0}.", query.TableAsName.GetValue(((meExp.Object as UnaryExpression).Operand as MemberExpression).Expression.Type.Name));
                        }
                        #endregion

                        if (meExp.Object is MemberExpression && !string.IsNullOrEmpty(asName))
                        {
                            #region system的方法转sql的系统函数
                            var mMethod = meExp.Method.Name;
                            var mName = ((MemberExpression)meExp.Object).Member.Name;
                            var mValue = "";
                            var mStar = "";
                            var mLength = "";
                            var mCount = 0;

                            meExp.Arguments.ToList().ForEach(a =>
                            {
                                mCount++;
                                mValue = Expression.Lambda(a).Compile().DynamicInvoke().ToString();

                                if (meExp.Arguments.Count == 2)
                                {
                                    if (mCount == 1)
                                        mStar = Expression.Lambda(a).Compile().DynamicInvoke().ToString();

                                    if (mCount == 2)
                                        mLength = Expression.Lambda(a).Compile().DynamicInvoke().ToString();
                                }
                            });

                            if (string.Compare(mMethod, "contains", true) == 0)
                            {
                                sb.AppendFormat(" {2}{0} like {3}{0}{1}", mName, i, asName, query.Config.Flag);
                                leftList.Add(mName);
                                rightList.Add(string.Format("%{0}%", mValue));
                                i++;
                            }
                            else if (string.Compare(mMethod, "endswith", true) == 0)
                            {
                                sb.AppendFormat(" {2}{0} like {3}{0}{1}", mName, i, asName, query.Config.Flag);
                                leftList.Add(mName);
                                rightList.Add(string.Format("%{0}", mValue));
                            }
                            else if (string.Compare(mMethod, "startswith", true) == 0)
                            {
                                sb.AppendFormat(" {2}{0} like {3}{0}{1}", mName, i, asName, query.Config.Flag);
                                leftList.Add(mName);
                                rightList.Add(string.Format("{0}%", mValue));
                                i++;
                            }
                            else if (string.Compare(mMethod, "substring", true) == 0)
                            {
                                var tempType = "";
                                if (expType == ExpressionType.Goto)
                                    tempType = "=";
                                else
                                    tempType = ExpressionTypeCast(expType);

                                if (query.Config.DbType == DataDbType.SqlServer)
                                    sb.AppendFormat(" substring({4}{0},{2},{3}) {6} {5}{0}{1}", mName, i, mStar, mLength, asName, query.Config.Flag, tempType);
                                else if (query.Config.DbType == DataDbType.Oracle || query.Config.DbType == DataDbType.MySql || query.Config.DbType == DataDbType.DB2)
                                    sb.AppendFormat(" substr({4}{0},{2},{3}) {6} {5}{0}{1}", mName, i, mStar, mLength, asName, query.Config.Flag, tempType);

                                leftList.Add(mName);
                                i++;
                            }
                            else if (string.Compare(mMethod, "toupper", true) == 0)
                            {
                                var tempType = "";
                                if (expType == ExpressionType.Goto)
                                    tempType = "=";
                                else
                                    tempType = ExpressionTypeCast(expType);
                                sb.AppendFormat(" upper({0}{1}) {4} {2}{1}{3}", asName, mName, query.Config.Flag, i, tempType);

                                leftList.Add(mName);
                                i++;
                            }
                            else if (string.Compare(mMethod, "CompareTo", true) == 0)
                            {
                                var tempType = "";
                                if (expType == ExpressionType.Goto)
                                    tempType = "=";
                                else
                                    tempType = ExpressionTypeCast(expType);
                                sb.AppendFormat(" upper({0}{1}) {4} upper({2}{1}{3})", asName, mName, query.Config.Flag, i, tempType);

                                leftList.Add(mName);
                                rightList.Add(mValue.ToString());
                                i++;
                            }
                            else if (string.Compare(mMethod, "tolower", true) == 0)
                            {
                                var tempType = "";
                                if (expType == ExpressionType.Goto)
                                    tempType = "=";
                                else
                                    tempType = ExpressionTypeCast(expType);
                                sb.AppendFormat(" lower({0}{1}) {4} {2}{1}{3}", asName, mName, query.Config.Flag, i, tempType);

                                leftList.Add(mName);
                                i++;
                            }
                            #endregion
                        }

                        if (meExp.Object == null && meExp.Method.Name == "Contains" && meExp.Arguments.Count == 2)
                        {
                            #region array.Contains
                            var array = Expression.Lambda(meExp.Arguments[0]).Compile().DynamicInvoke() as Array;
                            var mName = (meExp.Arguments[1] as MemberExpression).Member.Name;                           
                            asName = string.Format("{0}.", query.TableAsName.GetValue((meExp.Arguments[1] as MemberExpression).Expression.Type.Name));
                            sb.AppendFormat(" {0}{1} in (", asName, mName);
                            for (int ary = 0; ary < array.Length; ary++)
                            {
                                sb.AppendFormat("{0}{1}{2},", query.Config.Flag, mName, i);
                                leftList.Add(mName);
                                rightList.Add(array.GetValue(ary).ToStr());
                                i++;
                            }
                            sb.Remove(sb.Length - 1, 1);
                            sb.Append(")");
                            #endregion
                        }

                        if (string.IsNullOrEmpty(asName) && meExp.Method.Name == "Contains" && meExp.Arguments.Count == 1)
                        {
                            #region list.Contains
                            var mName = (meExp.Arguments[0] as MemberExpression).Member.Name;                            
                            asName = string.Format("{0}.", query.TableAsName.GetValue((meExp.Arguments[0] as MemberExpression).Expression.Type.Name));
                            var model = Expression.Lambda(meExp.Object).Compile().DynamicInvoke();
                            var count = (int)BaseEmit.Invoke(model, model.GetType().GetMethod("get_Count"), null);

                            sb.AppendFormat(" {0}{1} in (", asName, mName);
                            for (var j = 0; j < count; j++)
                            {
                                sb.AppendFormat("{0}{1}{2},", query.Config.Flag, mName, i);
                                leftList.Add(mName);
                                rightList.Add(BaseEmit.Invoke(model, model.GetType().GetMethod("get_Item"), new object[] { j }).ToStr());
                                i++;
                            }
                            sb.Remove(sb.Length - 1, 1);
                            sb.Append(")");
                            #endregion
                        }

                        if (isReturnNull)
                            return "";
                        else
                            return sb.ToString();
                    }
                    catch
                    {
                        return "";
                    }
                }
            }
            else if (exp is ConstantExpression)
            {
                typeList.Add("".GetType());
                ConstantExpression cExp = (ConstantExpression)exp;
                if (cExp.Value == null)
                    return "null";
                else
                    return cExp.Value.ToString();
            }
            else if (exp is UnaryExpression)
            {
                var ue = ((UnaryExpression)exp);
                return RouteExpressionHandler(query, ue.Operand, expType, ref leftList, ref rightList, ref typeList, ref sb, ref strType, ref i, isRight);
            }

            return null;
        }
        #endregion

        #region 拆分表达式树
        /// <summary>
        /// 拆分表达式树 
        /// </summary>
        /// <param name="left"></param>
        /// <param name="right"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        private static string BinaryExpressionHandler(DataQuery query, Expression left, Expression right, ExpressionType expType, ref List<string> leftList, ref List<string> rightList, ref List<System.Type> typeList, ref StringBuilder sb, ref string strType, ref int i, bool isRight = false)
        {
            string needParKey = "=,>,<,>=,<=,<>";

            string leftPar = RouteExpressionHandler(query, left, expType, ref leftList, ref rightList, ref typeList, ref sb, ref strType, ref i, isRight);

            string typeStr = ExpressionTypeCast(expType);

            isRight = needParKey.IndexOf(typeStr) > -1;

            if (!isRight)
            {
                strType = typeStr;

                sb.Append(string.Format(" {0} ", strType));
            }

            string rightPar = RouteExpressionHandler(query, right, expType, ref leftList, ref rightList, ref typeList, ref sb, ref strType, ref i, isRight);

            if (string.Compare(rightPar, "NULL", true) == 0 || (query.Config.DbType == DataDbType.Oracle && string.IsNullOrEmpty(rightPar)))
            {
                if (typeStr == "=")
                    rightPar = "IS NULL";
                else if (typeStr == "<>")
                    rightPar = "IS NOT NULL";

                if (left is UnaryExpression)
                    left = (left as UnaryExpression).Operand;

                if (left is MemberExpression)
                    sb.AppendFormat("{2}.{0} {1} ", leftPar, rightPar, query.TableAsName.GetValue((left as MemberExpression).Expression.Type.Name));

                if (left is MethodCallExpression)
                {
                    var meExp = (MethodCallExpression)(left.ReduceExtensions().Reduce());

                    if (string.Compare(meExp.Method.Name, "substring", true) == 0 ||
                        string.Compare(meExp.Method.Name, "toupper", true) == 0 ||
                        string.Compare(meExp.Method.Name, "tolower", true) == 0)
                    {
                        rightList.Add(rightPar);
                        i++;
                    }
                }
            }
            else
            {
                if (isRight)
                {
                    if (left is UnaryExpression)
                        left = (left as UnaryExpression).Operand;

                    if (left is MemberExpression && right is MemberExpression && ((right as MemberExpression).Expression as ParameterExpression) != null)
                    {
                        sb.AppendFormat("{0}.{1}{2}{3}.{4} "
                                        , query.TableAsName.GetValue((left as MemberExpression).Expression.Type.Name), leftPar
                                        , typeStr
                                        , query.TableAsName.GetValue((right as MemberExpression).Expression.Type.Name), rightPar);
                    }
                    else if (!(left is MethodCallExpression))
                    {                        
                        sb.AppendFormat("{0}.{1}{2}{5}{3}{4} ", query.TableAsName.GetValue((left as MemberExpression).Expression.Type.Name), leftPar
                                                            , typeStr, leftPar, i.ToString(), query.Config.Flag);
                        rightList.Add(rightPar);
                        leftList.Add(leftPar);
                        i++;
                    }
                    else if (left is MethodCallExpression)
                    {
                        var meExp = (MethodCallExpression)(left.ReduceExtensions().Reduce());

                        if (string.Compare(meExp.Method.Name, "substring", true) == 0 ||
                        string.Compare(meExp.Method.Name, "toupper", true) == 0 ||
                        string.Compare(meExp.Method.Name, "tolower", true) == 0)
                        {
                            rightList.Add(rightPar);
                            i++;
                        }
                    }
                }
            }

            return sb.ToString();
        }
        #endregion

        #region 转换运算符
        /// <summary>
        /// 转换运算符
        /// </summary>
        /// <param name="expType"></param>
        /// <returns></returns>
        private static string ExpressionTypeCast(ExpressionType expType)
        {
            switch (expType)
            {
                case ExpressionType.And:
                case ExpressionType.AndAlso:
                    return "and";
                case ExpressionType.Equal:
                    return "=";
                case ExpressionType.GreaterThan:
                    return ">";
                case ExpressionType.GreaterThanOrEqual:
                    return ">=";
                case ExpressionType.LessThan:
                    return "<";
                case ExpressionType.LessThanOrEqual:
                    return "<=";
                case ExpressionType.NotEqual:
                    return "<>";
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return "or";
                case ExpressionType.Add:
                case ExpressionType.AddChecked:
                    return "+";
                case ExpressionType.Subtract:
                case ExpressionType.SubtractChecked:
                    return "-";
                case ExpressionType.Divide:
                    return "/";
                case ExpressionType.Multiply:
                case ExpressionType.MultiplyChecked:
                    return "*";
                default:
                    return "";
            }
        }
        #endregion

        #region 删除最后四位
        /// <summary>
        /// 删除最后四位
        /// </summary>
        /// <param name="mValue"></param>
        /// <returns></returns>
        private static string Remove(string mValue)
        {
            if (mValue == "")
                return "";

            if (mValue == "and")
                return "";

            if (mValue.Trim().Substring(mValue.Trim().Length - 4, 4) == " and")
                return Remove(mValue.Trim().Substring(0, mValue.Trim().Length - 4));
            return mValue;
        }
        #endregion
    }
}
