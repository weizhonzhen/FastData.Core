using System;
using System.Collections.Generic;
using System.Text;
using PostSharp.Aspects;
using Untility.Core.Base;

namespace Untility.Core.Aop
{
    [Serializable]
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public sealed class LogAttribute : OnMethodBoundaryAspect
    {
        public bool SuccessEnabled { get; set; }
        public bool ExceptionEnabled { get; set; }
        public bool ExitEnabled { get; set; }
        public bool EntryEnabled { get; set; }

        /// <summary>
        /// 方法前触发
        /// </summary>
        /// <param name="args"></param>
        public override void OnEntry(MethodExecutionArgs args)
        {
            if (EntryEnabled)
            {
                var sb = new StringBuilder();

                sb.AppendFormat("执行前:{0}.{1}", args.Instance.GetType().Name, args.Method.Name);

                var param = args.Method.GetParameters();
                var value = args.Arguments.ToArray();

                for (int i = 0; i < param.Length; i++)
                {
                    sb.AppendFormat(",param:{0},value{1}", param[i], value[i]);
                }

                BaseLog.SaveLog(sb.ToString(), "log");
            }

            base.OnEntry(args);
        }

        /// <summary>
        /// 方法后触发
        /// </summary>
        /// <param name="args"></param>
        public override void OnSuccess(MethodExecutionArgs args)
        {
            if (SuccessEnabled)
            {
                var sb = new StringBuilder();

                sb.AppendFormat("执行中:{0}.{1}", args.Instance.GetType().Name, args.Method.Name);

                var param = args.Method.GetParameters();
                var value = args.Arguments.ToArray();

                for (int i = 0; i < param.Length; i++)
                {
                    sb.AppendFormat(",param:{0},value{1}", param[i], value[i]);
                }

                BaseLog.SaveLog(sb.ToString(), "log");
            }

            base.OnSuccess(args);
        }

        /// <summary>
        /// 退出触发
        /// </summary>
        /// <param name="args"></param>
        public override void OnExit(MethodExecutionArgs args)
        {
            if (ExitEnabled)
            {
                var sb = new StringBuilder();

                sb.AppendFormat("退出:{0}", BaseJson.ModelToJson(args.ReturnValue));

                BaseLog.SaveLog(sb.ToString(), "log");
            }

            base.OnExit(args);
        }

        /// <summary>
        /// 异常触发
        /// </summary>
        /// <param name="args"></param>
        public override void OnException(MethodExecutionArgs args)
        {
            if (ExceptionEnabled)
            {
                var sb = new StringBuilder();

                sb.AppendFormat("异常：时间{0},方法{1}{2},发生异常: {3}\n{4}", DateTime.Now, args.Instance.GetType().Name, args.Method.Name, args.Exception.Message, args.Exception.StackTrace);

                BaseLog.SaveLog(sb.ToString(), "log");
            }

            base.OnException(args);
        }
    }
}
