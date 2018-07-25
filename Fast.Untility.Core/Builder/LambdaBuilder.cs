using System;
using System.Linq.Expressions;

namespace Fast.Untility.Core.Builder
{
    /// <summary>
    /// lamda动态合并
    /// </summary>
    public static class LambdaBuilder
    {
        #region 创建lamda表达式单个泛型
        /// <summary>
        /// 创建lamda表达式单个泛型
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="expr1">条件1</param>
        /// <param name="expr2">条件2</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Where<T>(this Expression<Func<T, bool>> expr)
        {
            return Expression.Lambda<Func<T, bool>>(expr.Body, expr.Parameters);
        }
        #endregion

        #region 单个泛型lambda and 条件
        /// <summary>
        /// 单个泛型and 条件
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="expr1">条件1</param>
        /// <param name="expr2">条件2</param>
        /// <returns></returns>
        public static Expression<Func<T, bool>> Where<T>(this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
        {
            return Expression.Lambda<Func<T, bool>>(Expression.And(expr1.Body, expr2.Body), expr1.Parameters);
        }
        #endregion


        #region 创建lamda表达式两个泛型
        /// <summary>
        /// 创建lamda表达式两个泛型
        /// </summary>
        /// <typeparam name="inT1">泛型</typeparam>
        /// <typeparam name="inT2">泛型</typeparam>
        /// <param name="expr1">条件1</param>
        /// <param name="expr2">条件2</param>
        /// <returns></returns>
        public static Expression<Func<inT1, inT2, bool>> Where<inT1, inT2>(this Expression<Func<inT1, inT2, bool>> expr)
        {
            return Expression.Lambda<Func<inT1, inT2, bool>>(expr.Body, expr.Parameters);
        }
        #endregion

        #region 两个泛型lambda and 条件
        /// <summary>
        /// 两个泛型and 条件
        /// </summary>
        /// <typeparam name="T">泛型</typeparam>
        /// <param name="expr1">条件1</param>
        /// <param name="expr2">条件2</param>
        /// <returns></returns>
        public static Expression<Func<inT1, inT2, bool>> Where<inT1, inT2>(this Expression<Func<inT1, inT2, bool>> expr1, Expression<Func<inT1, inT2, bool>> expr2)
        {
            return Expression.Lambda<Func<inT1, inT2, bool>>(Expression.And(expr1.Body, expr2.Body), expr1.Parameters);
        }
        #endregion
    }
}
