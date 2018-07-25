using System;
using System.Collections.Generic;
using System.Text;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Fast.Untility.Core.Base;
using Fast.Data.Core.Type;
using Fast.Data.Core.Model;

namespace Fast.Data.Core.Base
{
    /// <summary>
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
            var sb = new StringBuilder();

            try
            {
                if (item == null)
                    return result;

                result.Where = RouteExpressionHandler(config, item.Body, ref leftList, ref rightList, ref sb, ref strType, ref i);

                result.Where = Remove(result.Where);

                for (i = 0; i < leftList.Count; i++)
                {
                    var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                    temp.ParameterName = leftList[i] + i.ToString();
                    temp.Value = rightList[i];

                    if (rightList[i].IsDate() && config.DbType == DataDbType.Oracle)
                        result.Where = result.Where.Replace(string.Format(":{0}", temp.ParameterName), string.Format("to_date(:{0},'yyyy/MM/dd hh24:mi:ss')", temp.ParameterName));

                    result.Param.Add(temp);
                }
                
                return result;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    DbLog.LogException<T>(config.IsOutError, config.DbType, ex, "LambdaWhere<T>", "");
                });
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
            var mysqlTake = "";
            var oracleTake = "";
            int i = 0;
            string strType = "";
            var num = 0;

            var leftList = new List<string>();
            var rightList = new List<string>();
            var fieldList = new List<string>();
            var sb = new StringBuilder();

            try
            {
                if (item == null)
                    return result;

                result.Where = RouteExpressionHandler(config, item.Body, ref leftList, ref rightList, ref sb, ref strType, ref i);

                result.Where = Remove(result.Where);

                for (i = 0; i < leftList.Count; i++)
                {
                    var temp = DbProviderFactories.GetFactory(config).CreateParameter();
                    temp.ParameterName = leftList[i] + i.ToString();
                    temp.Value = rightList[i];

                    if (rightList[i].IsDate() && config.DbType == DataDbType.Oracle)
                        result.Where = result.Where.Replace(string.Format(":{0}", temp.ParameterName), string.Format("to_date(:{0},'yyyy/MM/dd hh24:mi:ss')", temp.ParameterName));

                    result.Param.Add(temp);
                }

                return result;
            }
            catch (Exception ex)
            {
                Task.Factory.StartNew(() =>
                {
                    DbLog.LogException(config.IsOutError, config.DbType, ex, "LambdaWhere<T1, T2>", "");
                });
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
        private static string RouteExpressionHandler(ConfigModel config, Expression exp, ref List<string> leftList, ref List<string> rightList, ref StringBuilder sb, ref string strType, ref int i, bool isRight = false)
        {
            var isReturnNull = false;
            if (exp is BinaryExpression)
            {
                BinaryExpression be = (BinaryExpression)exp;

                return BinaryExpressionHandler(config, be.Left, be.Right, be.NodeType, ref leftList, ref rightList, ref sb, ref strType, ref i, isRight);
            }
            else if (exp is MemberExpression)
            {
                if ((exp as MemberExpression).Expression is ParameterExpression)
                    return (exp as MemberExpression).Member.Name;
                else
                    return Expression.Lambda(exp).Compile().DynamicInvoke() + "";
            }
            else if (exp is NewArrayExpression)
            {
                NewArrayExpression naExp = (NewArrayExpression)exp;
                StringBuilder sbArray = new StringBuilder();
                foreach (Expression expression in naExp.Expressions)
                {
                    sbArray.AppendFormat(",{0}", RouteExpressionHandler(config, expression, ref leftList, ref rightList, ref sb, ref strType, ref i, isRight));
                }

                return sbArray.Length == 0 ? "" : sbArray.Remove(0, 1).ToString();
            }
            else if (exp is MethodCallExpression)
            {
                if (isRight)
                {
                    return Expression.Lambda(exp).Compile().DynamicInvoke() + "";
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
                            if (meExp.Object is MemberExpression)
                                asName = ((meExp.Object as MemberExpression).Expression as ParameterExpression).Name + ".";
                            else if (meExp.Object is UnaryExpression)
                                asName = (((meExp.Object as UnaryExpression).Operand as MemberExpression).Expression as ParameterExpression).Name + ".";
                        }
                        #endregion

                        if ((MemberExpression)meExp.Object != null)
                        {
                            #region system的方法转sql的系统函数
                            var mMethod = meExp.Method.Name;
                            var mName = ((MemberExpression)meExp.Object).Member.Name;
                            var mValue = "";
                            var mStar = "";
                            var mLength = "";
                            var mCount = 0;

                            foreach (var item in meExp.Arguments)
                            {
                                mCount++;
                                mValue = Expression.Lambda(item).Compile().DynamicInvoke().ToString();

                                if (meExp.Arguments.Count == 2)
                                {
                                    if (mCount == 1)
                                        mStar = Expression.Lambda(item).Compile().DynamicInvoke().ToString();

                                    if (mCount == 2)
                                        mLength = Expression.Lambda(item).Compile().DynamicInvoke().ToString();
                                }
                            }

                            if (mMethod.ToLower() == "contains")
                            {
                                sb.AppendFormat(" {2}{0} like {3}{0}{1}", mName, i, asName, config.Flag);
                                leftList.Add(mName);
                                rightList.Add(string.Format("%{0}%", mValue));
                                i++;
                            }
                            else if (mMethod.ToLower() == "endswith")
                            {
                                sb.AppendFormat(" {2}{0} like {3}{0}{1}", mName, i, asName, config.Flag);
                                leftList.Add(mName);
                                rightList.Add(string.Format("%{0}", mValue));
                                i++;
                            }
                            else if (mMethod.ToLower() == "startswith")
                            {
                                sb.AppendFormat(" {2}{0} like {3}{0}{1}", mName, i, asName, config.Flag);
                                leftList.Add(mName);
                                rightList.Add(string.Format("{0}%", mValue));
                                i++;
                            }
                            else if (mMethod.ToLower() == "substring")
                            {
                                if (config.DbType == DataDbType.SqlServer)
                                    sb.AppendFormat(" substring({4}{0},{2},{3}) = {5}{0}{1}", mName, i, mStar, mLength, asName, config.Flag);
                                else if (config.DbType == DataDbType.Oracle || config.DbType == DataDbType.MySql || config.DbType == DataDbType.DB2)
                                    sb.AppendFormat(" substr({4}{0},{2},{3}) = {5}{0}{1}", mName, i, mStar, mLength, asName, config.Flag);

                                leftList.Add(mName);
                                rightList.Add(mValue.ToString());
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
                ConstantExpression cExp = (ConstantExpression)exp;
                if (cExp.Value == null)
                    return "null";
                else
                    return cExp.Value.ToString();
            }
            else if (exp is UnaryExpression)
            {
                var ue = ((UnaryExpression)exp);
                return RouteExpressionHandler(config, ue.Operand, ref leftList, ref rightList, ref sb, ref strType, ref i, isRight);
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
        private static string BinaryExpressionHandler(ConfigModel config, Expression left, Expression right, ExpressionType type, ref List<string> leftList, ref List<string> rightList, ref StringBuilder sb, ref string strType, ref int i, bool isRight = false)
        {
            string needParKey = "=,>,<,>=,<=,<>";

            string leftPar = RouteExpressionHandler(config, left, ref leftList, ref rightList, ref sb, ref strType, ref i, isRight);

            string typeStr = ExpressionTypeCast(type);

            isRight = needParKey.IndexOf(typeStr) > -1;

            if (!isRight)
            {
                strType = typeStr;

                sb.Append(string.Format("{0} ", strType));
            }

            string rightPar = RouteExpressionHandler(config, right, ref leftList, ref rightList, ref sb, ref strType, ref i, isRight);

            if (rightPar.ToUpper() == "NULL")
            {
                if (typeStr == "=")
                    rightPar = "IS NULL";
                else if (typeStr == "<>")
                    rightPar = "IS NOT NULL";
                
                if (left is UnaryExpression)
                    left = (left as UnaryExpression).Operand;
                               
                 if (left is MemberExpression)
                     sb.AppendFormat("{2}.{0} {1} ", leftPar, rightPar, ((left as MemberExpression).Expression as ParameterExpression).Name);
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
            result.Param = Parameter.ReNewParam(item.Param, config);

            foreach (var temp in item.Param)
            {
                var replace = string.Format("#{0}#", temp.ParameterName.ToLower());
                if (item.Where.ToLower().IndexOf(replace) >= 0)
                {
                    result.Param.RemoveAll(a => a.ParameterName == temp.ParameterName);
                    result.Where = result.Where.ToLower().Replace(replace, temp.Value.ToString());
                }
            }

            return result;
        }
        #endregion
    }
}
