using FastAop.Core;

namespace FastData.Core
{
    public class ConfigRepository
    {
        public FastAopAttribute Aop { get; set; }

        public string NameSpaceModel { get; set; }

        public string NameSpaceServie { get; set; }
    }
}
