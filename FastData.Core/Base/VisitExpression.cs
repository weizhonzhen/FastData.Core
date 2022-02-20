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
        public static VisitModel LambdaWhere<T>(Expression<Func<T, bool>> item, ConfigModel config)
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

                result.Where = RouteExpressionHandler(config, item.Body, ExpressionType.Goto, ref leftList, ref rightList, ref typeList, ref sb, ref strType, ref i);

                result.Where = Remove(result.Where);

                for (i = 0; i < leftList.Count; i++)
                {
                    var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                    temp.ParameterName = leftList[i] + i.ToString();
                    temp.Value = rightList[i];

                    if (typeList.Count >= i + 1 && typeList[i].Name == "DateTime")
                    {
                        if (config.DbType == DataDbType.Oracle)
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
                if (string.Compare(config.SqlErrorType, SqlErrorType.Db,false)==0)
                    DbLogTable.LogException<T>(config, ex, "LambdaWhere<T>", "");
                else
                    DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "LambdaWhere<T>", "");

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
        public static VisitModel LambdaWhere<T1, T2>(Expression<Func<T1, T2, bool>> item, ConfigModel config, bool isPage = false)
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

                result.Where = RouteExpressionHandler(config, item.Body, ExpressionType.Goto, ref leftList, ref rightList, ref typeList, ref sb, ref strType, ref i);

                result.Where = Remove(result.Where);

                for (i = 0; i < leftList.Count; i++)
                {
                    var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                    temp.ParameterName = leftList[i] + i.ToString();
                    temp.Value = rightList[i];

                    if (typeList.Count >= i + 1 && typeList[i].Name == "DateTime")
                    {
                        if (config.DbType == DataDbType.Oracle)
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
                    if (string.Compare( config.SqlErrorType, SqlErrorType.Db,false)==0)
                        DbLogTable.LogException(config, ex, "LambdaWhere<T1, T2>", "");
                    else
                        DbLog.LogException(config.IsOutError, config.DbType, ex, "LambdaWhere<T1, T2>", "");
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
        private static string RouteExpressionHandler(ConfigModel config, Expression exp, ExpressionType expType, ref List<string> leftList, ref List<string> rightList, ref List<System.Type> typeList, ref StringBuilder sb, ref string strType, ref int i, bool isRight = false)
        {
            var isReturnNull = false;
            if (exp is BinaryExpression)
            {
                BinaryExpression be = (BinaryExpression)exp;

                return BinaryExpressionHandler(config, be.Left, be.Right, be.NodeType, ref leftList, ref rightList, ref typeList, ref sb, ref strType, ref i, isRight);
            }
            else if (exp is MemberExpression)
            {
                if ((exp as MemberExpression).Expression is ParameterExpression)
                {
                    //typeList.Add("".GetType());
                    return (exp as MemberExpression).Member.Name;
                }
                else
                {
                    if (Expression.Lambda(exp).Compile().DynamicInvoke() == null)
                        typeList.Add("".GetType());
                    else
                        typeList.Add(Expression.Lambda(exp).Compile().DynamicInvoke().GetType());
                    return Expression.Lambda(exp).Compile().DynamicInvoke() + "";
                }
            }
            else if (exp is NewArrayExpression)
            {
                NewArrayExpression naExp = (NewArrayExpression)exp;
                StringBuilder sbArray = new StringBuilder();
                foreach (Expression expression in naExp.Expressions)
                {
                    sbArray.AppendFormat(",{0}", RouteExpressionHandler(config, expression, expType, ref leftList, ref rightList, ref typeList, ref sb, ref strType, ref i, isRight));
                }

                return sbArray.Length == 0 ? "" : sbArray.Remove(0, 1).ToString();
            }
            else if (exp is MethodCallExpression)
            {
                if (isRight)
                {
                    typeList.Add(Expression.Lambda(exp).Compile().DynamicInvoke().GetType());
                    return Expression.Lambda(exp).Compile().DynamicInvoke() + "";
                }
                else
                {
                    //typeList.Add("".GetType());
                    isRight = false;
                    try
                    {
                        var asName = "";
                        var meExp = (MethodCallExpression)(exp.ReduceExtensions().Reduce());

                        #region 表别名
                        if (meExp.Object != null)
                        {
                            if (meExp.Object is MemberExpression)
                                asName = ((meExp.Object as MemberExpression).Expression as ParameterExpression).Name + ".";
                            else if (meExp.Object is UnaryExpression)
                                asName = (((meExp.Object as UnaryExpression).Operand as MemberExpression).Expression as ParameterExpression).Name + ".";
                        }
                        #endregion

                        if(meExp.Object is MethodCallExpression)
                        {

                        }

                        if (meExp.Object is MemberExpression)
                        {
                            #region system的方法转sql的系统函数
                            var mMethod = meExp.Method.Name;
                            var mName = ((MemberExpression)meExp.Object).Member.Name;
                            var mValue = "";
                            var mStar = "";
                            var mLength = "";
                            var mCount = 0;

                            meExp.Arguments.ToList().ForEach(a => {
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

                            if (string.Compare( mMethod, "contains",false)==0)
                            {
                                sb.AppendFormat(" {2}{0} like {3}{0}{1}", mName, i, asName, config.Flag);
                                leftList.Add(mName);
                                rightList.Add(string.Format("%{0}%", mValue));
                                i++;
                            }
                            else if (string.Compare( mMethod, "endswith",false)==0)
                            {
                                sb.AppendFormat(" {2}{0} like {3}{0}{1}", mName, i, asName, config.Flag);
                                leftList.Add(mName);
                                rightList.Add(string.Format("%{0}", mValue));
                            }
                            else if (string.Compare( mMethod,"startswith",false)==0)
                            {
                                sb.AppendFormat(" {2}{0} like {3}{0}{1}", mName, i, asName, config.Flag);
                                leftList.Add(mName);
                                rightList.Add(string.Format("{0}%", mValue));
                                i++;
                            }
                            else if (string.Compare( mMethod,"substring",false)==0)
                            {
                                var tempType = "";
                                if (expType == ExpressionType.Goto)
                                    tempType = "=";
                                else
                                    tempType = ExpressionTypeCast(expType);

                                if (config.DbType == DataDbType.SqlServer)
                                    sb.AppendFormat(" substring({4}{0},{2},{3}) {6} {5}{0}{1}", mName, i, mStar, mLength, asName, config.Flag, tempType);
                                else if (config.DbType == DataDbType.Oracle || config.DbType == DataDbType.MySql || config.DbType == DataDbType.DB2)
                                    sb.AppendFormat(" substr({4}{0},{2},{3}) {6} {5}{0}{1}", mName, i, mStar, mLength, asName, config.Flag, tempType);

                                leftList.Add(mName);
                                //rightList.Add(mValue.ToString());
                                i++;
                            }
                            else if (string.Compare( mMethod, "toupper",false)==0)
                            {
                                var tempType = "";
                                if (expType == ExpressionType.Goto)
                                    tempType = "=";
                                else
                                    tempType = ExpressionTypeCast(expType);
                                sb.AppendFormat(" upper({0}{1}) {4} {2}{1}{3}", asName, mName, config.Flag, i, tempType);

                                leftList.Add(mName);
                                //rightList.Add(mValue.ToString());
                                i++;
                            }
                            else if (string.Compare( mMethod, "tolower",false)==0)
                            {
                                var tempType = "";
                                if (expType == ExpressionType.Goto)
                                    tempType = "=";
                                else
                                    tempType = ExpressionTypeCast(expType);
                                sb.AppendFormat(" lower({0}{1}) {4} {2}{1}{3}", asName, mName, config.Flag, i, tempType);

                                leftList.Add(mName);
                                //rightList.Add(mValue.ToString());
                                i++;
                            }
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
                return RouteExpressionHandler(config, ue.Operand, expType, ref leftList, ref rightList, ref typeList, ref sb, ref strType, ref i, isRight);
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
        private static string BinaryExpressionHandler(ConfigModel config, Expression left, Expression right, ExpressionType expType, ref List<string> leftList, ref List<string> rightList, ref List<System.Type> typeList, ref StringBuilder sb, ref string strType, ref int i, bool isRight = false)
        {
            string needParKey = "=,>,<,>=,<=,<>";

            string leftPar = RouteExpressionHandler(config, left, expType, ref leftList, ref rightList, ref typeList, ref sb, ref strType, ref i, isRight);

            string typeStr = ExpressionTypeCast(expType);

            isRight = needParKey.IndexOf(typeStr) > -1;

            if (!isRight)
            {
                strType = typeStr;

                sb.Append(string.Format(" {0} ", strType));
            }

            string rightPar = RouteExpressionHandler(config, right, expType, ref leftList, ref rightList, ref typeList, ref sb, ref strType, ref i, isRight);

            if (rightPar.ToUpper() == "NULL" || (config.DbType == DataDbType.Oracle && string.IsNullOrEmpty(rightPar)))
            {
                if (typeStr == "=")
                    rightPar = "IS NULL";
                else if (typeStr == "<>")
                    rightPar = "IS NOT NULL";

                if (left is UnaryExpression)
                    left = (left as UnaryExpression).Operand;

                if (left is MemberExpression)
                    sb.AppendFormat("{2}.{0} {1} ", leftPar, rightPar, ((left as MemberExpression).Expression as ParameterExpression).Name);

                if (left is MethodCallExpression)
                {
                    var meExp = (MethodCallExpression)(left.ReduceExtensions().Reduce());

                    if (string.Compare(meExp.Method.Name, "substring", false) == 0 ||
                        string.Compare(meExp.Method.Name, "toupper", false) == 0 ||
                        string.Compare(meExp.Method.Name, "tolower", false) == 0)
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
                                        , ((left as MemberExpression).Expression as ParameterExpression).Name, leftPar
                                        , typeStr
                                        , ((right as MemberExpression).Expression as ParameterExpression).Name, rightPar);
                    }
                    else if (!(left is MethodCallExpression))
                    {
                        sb.AppendFormat("{0}.{1}{2}{5}{3}{4} ", ((left as MemberExpression).Expression as ParameterExpression).Name, leftPar
                                                            , typeStr, leftPar, i.ToString(), config.Flag);
                        rightList.Add(rightPar);
                        leftList.Add(leftPar);
                        i++;
                    }
                    else if (left is MethodCallExpression)
                    {
                        var meExp = (MethodCallExpression)(left.ReduceExtensions().Reduce());

                        if (string.Compare(meExp.Method.Name, "substring", false) == 0 ||
                        string.Compare(meExp.Method.Name, "toupper", false) == 0 ||
                        string.Compare(meExp.Method.Name, "tolower", false) == 0)
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

        #region 替换
        /// <summary>
        /// 替换
        /// </summary>
        /// <param name="param"></param>
        /// <param name="sql"></param>
        /// <returns></returns>
        private static VisitModel BaseReplace(VisitModel item, ConfigModel config)
        {
            var result = new VisitModel();
            result.Where = item.Where;
            result.Param = item.Param;

            item.Param.ForEach(p => {
                var replace = string.Format("#{0}#", p.ParameterName.ToLower());
                if (item.Where.ToLower().IndexOf(replace) >= 0)
                {
                    result.Param.RemoveAll(a => a.ParameterName == p.ParameterName);
                    result.Where = result.Where.ToLower().Replace(replace, p.Value.ToString());
                }
            });

            return result;
        }
        #endregion

    }
}
