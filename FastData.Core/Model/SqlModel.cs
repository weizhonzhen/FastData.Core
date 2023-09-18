using FastData.Core.Base;
using FastUntility.Core.Base;
using System.Data.Common;
using System.Text;

namespace FastData.Core.Model
{
    internal class SqlModel
    {
        public int I { get; set; }

        public DbParameter Param { get; set; }

        public string MapName { get; set; }

        public string ParamKey { get { return string.Format("{0}.{1}.{2}", MapName.ToLower(), Param.ParameterName.ToLower(), I); }}

        public string ConditionKey { get { return string.Format("{0}.{1}.condition.{2}", MapName.ToLower(), Param.ParameterName.ToLower(), I); }}

        public string CondtionValueKey { get { return string.Format("{0}.{1}.condition.value.{2}", MapName.ToLower(), Param.ParameterName.ToLower(), I); } }

        public string IncludeKey { get { return string.Format("{0}.condition.{1}", MapName.ToLower(), I); }}

        public string IncludeRefIdKey { get { return string.Format("{0}.condition.include.{1}", MapName.ToLower(), I); }}

        public string CacheType { get; set; }

        public string FlagParam { get { return string.Format("{0}{1}", Flag, Param.ParameterName).ToLower(); }}

        public string Flag { get; set; }

        public string ReplaceKey { get { return string.Format("#{0}#", Param.ParameterName).ToLower(); }}

        public StringBuilder Sql { get; set; } = new StringBuilder();

        public string ConditionValue { get { return DbCache.Get(CacheType, CondtionValueKey).ToStr().ToLower(); } set { } }

        public string ParamSql { get { return DbCache.Get(CacheType, ParamKey.ToLower()).ToLower(); } }

        public string ReferencesKey { get { return string.Format("{0}.{1}.references.{2}", MapName.ToLower(), Param.ParameterName.ToLower(), I); }}
    }
}