using System;

namespace FastData.Core.Aop
{
    public interface IFastAop
    {
        void MapBefore(MapBeforeContext context);

        void MapAfter(MapAfterContext context);

        void Before(BeforeContext context);

        void After(AfterContext context);

        void Exception(ExceptionContext context);
    }
}