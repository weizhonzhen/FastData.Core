namespace FastData.Core.Aop
{
    public interface IFastAop
    {
        void Map(MapContext context);

        void Before(BeforeContext context);

        void After(AfterContext context);
    }
}