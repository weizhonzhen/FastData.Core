using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Untility.Core.Base;

namespace Data.Core.Base
{
    /// <summary>
    /// 动态解析条件
    /// </summary>
    internal static class BaseCodeDom
    {
        public static bool GetResult(string code)
        {
            //动态编译
            var compiler = new CSharpCodeProvider().CreateCompiler();
            var param = new CompilerParameters();
            param.ReferencedAssemblies.Add("System.dll");
            param.GenerateExecutable = false;
            param.GenerateInMemory = true;
            var result = compiler.CompileAssemblyFromSource(param, GetCode(code));

            if (result.Errors.HasErrors)
            {
                Task.Factory.StartNew(() =>
                {
                    var error = new StringBuilder();
                    error.AppendFormat("code:{0},error info:", GetCode(code));
                    foreach (CompilerError info in result.Errors)
                    {
                        error.Append(info.ErrorText);
                    }

                    BaseLog.SaveLog(error.ToString(), "DynamicCompiler");
                });
                return false;
            }
            else
            {
                //反射
                var assembly = result.CompiledAssembly;
                var instance = assembly.CreateInstance("DynamicCode.Condition");
                var method = instance.GetType().GetMethod("OutPut");
                return (bool)method.Invoke(instance, null);
            }
        }

        /// <summary>
        /// 源代码
        /// </summary>
        /// <param name="code"></param>
        /// <returns></returns>
        private static string GetCode(string code)
        {
            var sb = new StringBuilder();
            sb.Append("using System;");
            sb.Append(Environment.NewLine);
            sb.Append("namespace DynamicCode");
            sb.Append(Environment.NewLine);
            sb.Append("{");
            sb.Append(Environment.NewLine);
            sb.Append("    public class Condition");
            sb.Append(Environment.NewLine);
            sb.Append("    {");
            sb.Append(Environment.NewLine);
            sb.Append("        public bool OutPut()");
            sb.Append(Environment.NewLine);
            sb.Append("        {");
            sb.Append(Environment.NewLine);
            sb.AppendFormat("             return {0};", code);
            sb.Append(Environment.NewLine);
            sb.Append("        }");
            sb.Append(Environment.NewLine);
            sb.Append("    }");
            sb.Append(Environment.NewLine);
            sb.Append("}");

            return sb.ToString();
        }
    }
}
