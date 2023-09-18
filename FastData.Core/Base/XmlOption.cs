using FastData.Core.Model;
using FastUntility.Core.Base;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reflection;
using System.Xml;

namespace FastData.Core.Base
{
    internal static class XmlOption
    {
        /// <summary>
        /// xml check
        /// </summary>
        /// <param name="result"></param>
        /// <param name="tempKey"></param>
        /// <param name="dyn"></param>
        internal static void CheckXml(XmlModel result, string tempKey, XmlNode dyn)
        {
            //check required
            if (dyn.Attributes["required"] != null)
                result.Check.Add(string.Format("{0}.{1}.required", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["required"].Value.ToStr());

            //check maxlength
            if (dyn.Attributes["maxlength"] != null)
                result.Check.Add(string.Format("{0}.{1}.maxlength", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["maxlength"].Value.ToStr());

            //check existsmap
            if (dyn.Attributes["existsmap"] != null)
                result.Check.Add(string.Format("{0}.{1}.existsmap", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["existsmap"].Value.ToStr());

            //check checkmap
            if (dyn.Attributes["checkmap"] != null)
                result.Check.Add(string.Format("{0}.{1}.checkmap", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["checkmap"].Value.ToStr());

            //check date
            if (dyn.Attributes["date"] != null)
                result.Check.Add(string.Format("{0}.{1}.date", tempKey, dyn.Attributes["property"].Value.ToLower()), dyn.Attributes["date"].Value.ToStr());

        }

        /// <summary>
        /// foreach xml
        /// </summary>
        /// <param name="result"></param>
        /// <param name="tempKey"></param>
        /// <param name="dyn"></param>
        internal static void ForeachXml(XmlModel result, string tempKey, XmlNode dyn, int foreachCount)
        {
            if (string.Compare(dyn.Name, "foreach", true) == 0)
            {
                //type
                if (dyn.Attributes["type"] != null)
                {
                    result.Key.Add(string.Format("{0}.foreach.type.{1}", tempKey, foreachCount));
                    result.Sql.Add(dyn.Attributes["type"].Value);
                }

                //result name
                result.Key.Add(string.Format("{0}.foreach.name.{1}", tempKey, foreachCount));
                if (dyn.Attributes["name"] != null)
                    result.Sql.Add(dyn.Attributes["name"].Value.ToLower());
                else
                    result.Sql.Add("data");

                //field
                if (dyn.Attributes["field"] != null)
                {
                    result.Key.Add(string.Format("{0}.foreach.field.{1}", tempKey, foreachCount));
                    result.Sql.Add(dyn.Attributes["field"].Value.ToLower());
                }

                //sql
                if (dyn.ChildNodes[0] is XmlText)
                {
                    result.Key.Add(string.Format("{0}.foreach.sql.{1}", tempKey, foreachCount));
                    result.Sql.Add(dyn.ChildNodes[0].InnerText.Replace("&lt;", "<").Replace("&gt", ">"));
                }
            }
        }

        /// <summary>
        /// choose xml
        /// </summary>
        /// <param name="result"></param>
        /// <param name="tempKey"></param>
        /// <param name="dyn"></param>
        /// <param name="i"></param>
        internal static void ChooseXml(XmlModel result, string tempKey, XmlNode dyn, int i)
        {
            //条件类型
            result.Key.Add(string.Format("{0}.{1}.condition.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
            result.Sql.Add(dyn.Name);

            if (dyn is XmlElement)
            {
                var count = 0;
                result.Key.Add(string.Format("{0}.{1}.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                result.Sql.Add(dyn.ChildNodes.Count.ToStr());
                foreach (XmlNode child in dyn.ChildNodes)
                {
                    //other
                    if (child.Name == "other")
                    {
                        result.Key.Add(string.Format("{0}.{1}.{2}.choose.other.{3}", tempKey, dyn.Attributes["property"].Value.ToLower(), i, count));
                        result.Sql.Add(string.Format("{0}{1}", child.Attributes["prepend"].Value.ToLower(), child.InnerText));
                    }
                    else
                    {
                        //条件
                        if (child.Attributes["property"] != null)
                        {
                            result.Key.Add(string.Format("{0}.{1}.{2}.choose.condition.{3}", tempKey, dyn.Attributes["property"].Value.ToLower(), i, count));
                            result.Sql.Add(child.Attributes["property"].Value);
                        }

                        //内容
                        result.Key.Add(string.Format("{0}.{1}.{2}.choose.{3}", tempKey, dyn.Attributes["property"].Value.ToLower(), i, count));
                        result.Sql.Add(string.Format("{0}{1}", child.Attributes["prepend"].Value.ToLower(), child.InnerText));

                        //引用dll
                        if (child.Attributes["references"] != null)
                        {
                            result.Key.Add(string.Format("{0}.{1}.{2}.choose.references.{3}", tempKey, dyn.Attributes["property"].Value.ToLower(), i, count));
                            result.Sql.Add(child.Attributes["references"].Value);
                        }
                    }
                    count++;
                }
            }
        }

        /// <summary>
        /// other Condition
        /// </summary>
        /// <param name="result"></param>
        /// <param name="tempKey"></param>
        /// <param name="dyn"></param>
        /// <param name="i"></param>
        internal static void ConditionXml(XmlModel result, string tempKey, XmlNode dyn, int i)
        {
            //属性和值
            result.Key.Add(string.Format("{0}.{1}.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
            result.Sql.Add(string.Format("{0}{1}", dyn.Attributes["prepend"].Value.ToLower(), dyn.InnerText));

            //条件类型
            result.Key.Add(string.Format("{0}.{1}.condition.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
            result.Sql.Add(dyn.Name);

            //判断条件内容
            if (dyn.Attributes["condition"] != null)
            {
                result.Key.Add(string.Format("{0}.{1}.condition.value.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                result.Sql.Add(dyn.Attributes["condition"].Value);
            }

            //比较条件值
            if (dyn.Attributes["compareValue"] != null)
            {
                result.Key.Add(string.Format("{0}.{1}.condition.value.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                result.Sql.Add(dyn.Attributes["compareValue"].Value.ToLower());
            }

            //引用dll
            if (dyn.Attributes["references"] != null)
            {
                result.Key.Add(string.Format("{0}.{1}.references.{2}", tempKey, dyn.Attributes["property"].Value.ToLower(), i));
                result.Sql.Add(dyn.Attributes["references"].Value);
            }
        }

        /// <summary>
        /// include
        /// </summary>
        /// <param name="result"></param>
        /// <param name="temkpKey"></param>
        /// <param name="dyn"></param>
        /// <param name="i"></param>
        internal static void IncludeXml(XmlModel result, string tempKey, XmlNode dyn, int i)
        {
            result.Key.Add(string.Format("{0}.condition.{1}", tempKey, i));
            result.Sql.Add(dyn.Name);
            result.Key.Add(string.Format("{0}.condition.include.{1}", tempKey, i));
            result.Sql.Add(dyn.Attributes["refid"].Value);
        }

        /// <summary>
        /// IsEqual Sql
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tempParam"></param>
        internal static void IsEqualSql(SqlModel model, List<DbParameter> tempParam)
        {
            if (model.ConditionValue == model.Param.Value.ToStr())
            {
                if (model.ParamSql.IndexOf(model.ReplaceKey) >= 0)
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(model.ParamSql.ToString().Replace(model.ReplaceKey, model.Param.Value.ToStr()));
                }
                else if (model.ParamSql.IndexOf(model.FlagParam) < 0 && model.Flag != "")
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(model.ParamSql);
                }
                else
                    model.Sql.Append(model.ParamSql);
            }
            else
                tempParam.Remove(model.Param);
        }

        /// <summary>
        /// IsNotEqual Sql
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tempParam"></param>
        internal static void IsNotEqualSql(SqlModel model, List<DbParameter> tempParam)
        {
            if (model.ConditionValue != model.Param.Value.ToStr())
            {
                if (model.ParamSql.IndexOf(model.ReplaceKey) >= 0)
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(model.ParamSql.ToString().Replace(model.ReplaceKey, model.Param.Value.ToStr()));
                }
                else if (model.ParamSql.IndexOf(model.FlagParam) < 0 && model.Flag != "")
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(model.ParamSql);
                }
                else
                    model.Sql.Append(model.ParamSql);
            }
            else
                tempParam.Remove(model.Param);
        }

        /// <summary>
        /// IsGreaterThan Sql
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tempParam"></param>
        internal static void IsGreaterThanSql(SqlModel model, List<DbParameter> tempParam)
        {
            if (model.Param.Value.ToStr().ToDecimal(0) > model.ConditionValue.ToDecimal(0))
            {
                if (model.ParamSql.IndexOf(model.ReplaceKey) >= 0)
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(model.ParamSql.ToString().Replace(model.ReplaceKey, model.Param.Value.ToStr()));
                }
                else if (model.ParamSql.IndexOf(model.FlagParam) < 0 && model.Flag != "")
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(model.ParamSql);
                }
                else
                    model.Sql.Append(model.ParamSql);
            }
            else
                tempParam.Remove(model.Param);
        }

        /// <summary>
        /// IsLessThan Sql
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tempParam"></param>
        internal static void IsLessThanSql(SqlModel model, List<DbParameter> tempParam)
        {
            if (model.Param.Value.ToStr().ToDecimal(0) < model.ConditionValue.ToDecimal(0))
            {
                if (model.ParamSql.IndexOf(model.ReplaceKey) >= 0)
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(model.ParamSql.ToString().Replace(model.ReplaceKey, model.Param.Value.ToStr()));
                }
                else if (model.ParamSql.IndexOf(model.FlagParam) < 0 && model.Flag != "")
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(model.ParamSql);
                }
                else
                    model.Sql.Append(model.ParamSql);
            }
            else
                tempParam.Remove(model.Param);
        }

        /// <summary>
        /// IsNullOrEmpty Sql
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tempParam"></param>
        internal static void IsNullOrEmptySql(SqlModel model, List<DbParameter> tempParam)
        {
            if (string.IsNullOrEmpty(model.Param.Value.ToStr()))
            {
                if (model.ParamSql.IndexOf(model.ReplaceKey) >= 0)
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(model.ParamSql.ToString().Replace(model.ReplaceKey, model.Param.Value.ToStr()));
                }
                else if (model.ParamSql.IndexOf(model.FlagParam) < 0 && model.Flag != "")
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(model.ParamSql);
                }
                else
                    model.Sql.Append(model.ParamSql);
            }
            else
                tempParam.Remove(model.Param);
        }

        /// <summary>
        /// IsNotNullOrEmpty Sql
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tempParam"></param>
        internal static void IsNotNullOrEmptySql(SqlModel model, List<DbParameter> tempParam)
        {
            if (!string.IsNullOrEmpty(model.Param.Value.ToStr()))
            {
                if (model.ParamSql.IndexOf(model.ReplaceKey) >= 0)
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(model.ParamSql.ToString().Replace(model.ReplaceKey, model.Param.Value.ToStr()));
                }
                else if (model.ParamSql.IndexOf(model.FlagParam) < 0 && model.Flag != "")
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(model.ParamSql);
                }
                else
                    model.Sql.Append(model.ParamSql);
            }
            else
                tempParam.Remove(model.Param);
        }

        /// <summary>
        /// If Sql
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tempParam"></param>
        internal static void IfSql(SqlModel model, List<DbParameter> tempParam)
        {
            model.ConditionValue = model.ConditionValue.Replace(model.Param.ParameterName, model.Param.Value == null ? null : model.Param.Value.ToStr());
            model.ConditionValue = model.ConditionValue.Replace("#", "\"");

            //references
            var ifSuccess = false;
            if (DbCache.Get(model.CacheType, model.ReferencesKey).ToStr() != "")
            {
                var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.FullName.Split(',')[0] == DbCache.Get(model.CacheType, model.ReferencesKey));
                if (assembly == null)
                    assembly = Assembly.Load(DbCache.Get(model.CacheType, model.ReferencesKey));
                if (assembly != null)
                {
                    var options = ScriptOptions.Default.AddReferences(assembly);
                    ifSuccess = CSharpScript.EvaluateAsync<bool>(model.ConditionValue, options).Result;
                }
                else
                    ifSuccess = CSharpScript.EvaluateAsync<bool>(model.ConditionValue).Result;
            }
            else
                ifSuccess = CSharpScript.EvaluateAsync<bool>(model.ConditionValue).Result;

            if (ifSuccess)
            {
                if (model.ParamSql.IndexOf(model.ReplaceKey) >= 0)
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(model.ParamSql.ToString().Replace(model.ReplaceKey, model.Param.Value.ToStr()));
                }
                else if (model.ParamSql.IndexOf(model.FlagParam) < 0 && model.Flag != "")
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(DbCache.Get(model.CacheType, model.ParamKey));
                }
                else
                    model.Sql.Append(DbCache.Get(model.CacheType, model.ParamKey));
            }
            else
                tempParam.Remove(model.Param);
        }

        /// <summary>
        /// IsPropertyAvailable Sql
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tempParam"></param>
        internal static void IsPropertyAvailableSql(SqlModel model, List<DbParameter> tempParam)
        {
            if (model.ParamSql.IndexOf(model.ReplaceKey) >= 0)
            {
                tempParam.Remove(model.Param);
                model.Sql.Append(model.ParamSql.ToString().Replace(model.ReplaceKey, model.Param.Value.ToStr()));
            }
            else if (model.ParamSql.IndexOf(model.FlagParam) < 0 && model.Flag != "")
            {
                tempParam.Remove(model.Param);
                model.Sql.Append(DbCache.Get(model.CacheType, model.ParamKey));
            }
            else
                model.Sql.Append(DbCache.Get(model.CacheType, model.ParamKey));
        }

        /// <summary>
        /// Include Sql
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tempParam"></param>
        internal static void IncludeSql(SqlModel model, List<DbParameter> tempParam)
        {

        }

        /// <summary>
        /// Choose Sql
        /// </summary>
        /// <param name="model"></param>
        /// <param name="tempParam"></param>
        internal static void ChooseSql(SqlModel model, List<DbParameter> tempParam)
        {
            var conditionOther = "";
            var isSuccess = false;
            for (int j = 0; j < DbCache.Get(model.CacheType, model.ParamKey).ToStr().ToInt(0); j++)
            {
                var conditionOtherKey = string.Format("{0}.choose.other.{1}", model.ParamKey, j);
                if (DbCache.Get(model.CacheType, conditionOtherKey).ToStr() != "")
                    conditionOther = DbCache.Get(model.CacheType, conditionOtherKey).ToLower();

                var conditionKey = string.Format("{0}.choose.{1}", model.ParamKey, j);
                var condition = DbCache.Get(model.CacheType, conditionKey).ToStr().ToLower();
                var conditionValueKey = string.Format("{0}.choose.condition.{1}", model.ParamKey, j);
                var conditionValue = DbCache.Get(model.CacheType, conditionValueKey).ToStr();
                conditionValue = conditionValue.Replace(model.Param.ParameterName, model.Param.Value == null ? null : model.Param.Value.ToStr());
                conditionValue = conditionValue.Replace("#", "\"");

                //references
                var referencesKey = string.Format("{0}.choose.references.{1}", model.ParamKey, j);
                if (DbCache.Get(model.CacheType, referencesKey).ToStr() != "")
                {
                    var assembly = AppDomain.CurrentDomain.GetAssemblies().ToList().Find(a => a.FullName.Split(',')[0] == DbCache.Get(model.CacheType, referencesKey));
                    if (assembly == null)
                        assembly = Assembly.Load(DbCache.Get(model.CacheType, referencesKey));
                    if (assembly != null)
                    {
                        var options = ScriptOptions.Default.AddReferences(assembly);
                        isSuccess = CSharpScript.EvaluateAsync<bool>(conditionValue, options).Result;
                    }
                    else
                        isSuccess = CSharpScript.EvaluateAsync<bool>(conditionValue).Result;
                }
                else
                    isSuccess = CSharpScript.EvaluateAsync<bool>(conditionValue).Result;

                if (isSuccess)
                {
                    if (condition.IndexOf(model.ReplaceKey) >= 0)
                    {
                        tempParam.Remove(model.Param);
                        model.Sql.Append(condition.Replace(model.ReplaceKey, model.Param.Value.ToStr()));
                    }
                    else if (condition.IndexOf(model.FlagParam) < 0 && model.Flag != "")
                    {
                        tempParam.Remove(model.Param);
                        model.Sql.Append(condition.Replace(model.ReplaceKey, model.Param.Value.ToStr()));
                    }
                    else
                        model.Sql.Append(condition);
                    break;
                }
            }

            if (!isSuccess)
            {
                if (conditionOther == "")
                    tempParam.Remove(model.Param);
                else if (conditionOther.IndexOf(model.ReplaceKey) >= 0)
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(conditionOther.Replace(model.ReplaceKey, model.Param.Value.ToStr()));
                }
                else if (conditionOther.IndexOf(model.FlagParam) < 0 && model.Flag != "")
                {
                    tempParam.Remove(model.Param);
                    model.Sql.Append(conditionOther.Replace(model.ReplaceKey, model.Param.Value.ToStr()));
                }
                else
                    model.Sql.Append(conditionOther);
            }
        }
    }
}