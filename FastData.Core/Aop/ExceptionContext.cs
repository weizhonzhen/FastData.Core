using System;

namespace FastData.Core.Aop
{
    public class ExceptionContext
    {
        public string dbType { get; internal set; }

        public AopType type { get; internal set; }

        public string name { get; internal set; }

        public Exception ex { get; internal set; }
    }
}
