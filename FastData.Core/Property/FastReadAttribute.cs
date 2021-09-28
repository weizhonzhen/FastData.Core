using System;

namespace FastData.Core.Property
{
    [AttributeUsage(AttributeTargets.Method)]
    public class FastReadAttribute : Attribute
    {
        public string sql { get; set; }

        public string dbKey { get; set; }
    }
}
