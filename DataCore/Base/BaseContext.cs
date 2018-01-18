using Data.Core.Context;
using Data.Core.Model;
using System.Collections.Generic;
using System.Threading;

namespace Data.Core.Base
{
    internal static class BaseContext
    {
        private static AsyncLocal<Dictionary<string, object>> CallContext = new AsyncLocal<Dictionary<string, object>>();

        #region 获取读上下文
        /// <summary>
        /// 获取读上下文
        /// </summary>
        /// <returns></returns>
        public static ReadContext GetReadContext(DataQuery item)
        {
            return new ReadContext(item.Key, item.Config);

            var dataKey = item.Key ?? "LambdaReadDb";

            var context = CallContext.Value[dataKey] as ReadContext;

            if (context == null)
            {
                context = new ReadContext(item.Key, item.Config);
                CallContext.Value.Add(dataKey, context);
            }

            return context;
        }
        #endregion

        #region 获取读上下文
        /// <summary>
        /// 获取读上下文
        /// </summary>
        /// <returns></returns>
        public static ReadContext GetReadContext(string key)
        {
            return new ReadContext(key);

            var dataKey = key ?? "LambdaReadDb";

            var context = CallContext.Value[dataKey] as ReadContext;

            if (context == null)
            {
                context = new ReadContext(key);
                CallContext.Value.Add(dataKey, context);
            }

            return context;
        }
        #endregion

        #region 获取写上下文
        /// <summary>
        /// 获取写上下文
        /// </summary>
        /// <returns></returns>
        public static WriteContext GetWriteContext(string key)
        {
            return new WriteContext(key);

            var dataKey = key ?? "LambdaWriteDb";

            var context = CallContext.Value[dataKey] as WriteContext;

            if (context == null)
            {
                context = new WriteContext(key);
                CallContext.Value.Add(dataKey, context);
            }

            return context;
        }
        #endregion
    }
}
