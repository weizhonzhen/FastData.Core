using System.Collections.Generic;

namespace FastData.Core.CacheModel
{
    internal class NavigateModel
    {
        public System.Type PropertyType { get; set; }

        public List<string> Name { get; set; } = new List<string>();

        public List<string> Key { get; set; } = new List<string>();

        public List<string> Appand { get; set; } = new List<string>();

        public bool IsList { get; set; }

        public string MemberName { get; set; }

        public System.Type MemberType { get; set; }

        public bool IsAdd { get; set; }

        public bool IsUpdate { get; set; }

        public bool IsDel { get; set; }
    }
}