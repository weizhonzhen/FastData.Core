
namespace FastData.Core.Aop
{
    public enum AopType
    {
        Add = 1,
        AddList = 2,

        Update_Lambda = 3,
        Update_PrimaryKey = 4,
        UpdateList = 5,

        Delete_Lambda = 6,
        Delete_PrimaryKey = 7,

        Execute_Sql_Bool = 8,
        Execute_Sql_Model = 9,
        Execute_Sql_Dic = 9,


        Map_List_Model = 10,
        Map_List_Dic = 10,
        Map_Page_Dic = 10,
        Map_Page_Model = 10,
        Map_Write = 11,


        Query_List_Lambda = 10,
        Query_Dic_Lambda = 11,
        Query_DataTable_Lambda = 12,
        Query_Json_Lambda = 13,
        Query_Count_Lambda = 14,
        Query_Page_Lambda_Dic = 15,
        Query_Page_Lambda_Model = 16,
        Query_Page_Sql_Dic = 16,
        Query_Page_Sql_Model = 16,

        DataContext = 11,
        ParsingXml =11,
        CodeFirst=11


    }
}
