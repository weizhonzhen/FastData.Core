using FastData.Core.Context;
using FastData.Core.Model;
using System.Collections.Generic;
using System.Threading;

namespace FastData.Core.Base
{
    internal static class BaseContext
    {
        private static AsyncLocal<Dictionary<string, object>> CallContext = new AsyncLocal<Dictionary<string, object>>();

        #region 获取读上下文
        /// <summary>
        /// 获取读上下文
        /// </summary>
        /// <returns></returns>
        public static DataContext GetContext(DataQuery item)
        {
            return new DataContext(item.Key, item.Config);
        }
        #endregion

        #region 获取读上下文
        /// <summary>
        /// 获取读上下文
        /// </summary>
        /// <returns></returns>
        public static DataContext GetContext(string key)
        {
            return new DataContext(key);
        }
        #endregion
    }
}
