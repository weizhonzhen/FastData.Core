﻿
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
        Execute_Sql_Dic = 10,

        Map_List_Model = 11,
        Map_List_Dic = 12,
        Map_Page_Dic = 13,
        Map_Page_Model = 14,
        Map_Write = 15,

        Query_List_Lambda = 16,
        Query_Dic_Lambda = 17,
        Query_DataTable_Lambda = 18,
        Query_Json_Lambda = 19,
        Query_Count_Lambda = 20,
        Query_Page_Lambda_Dic = 21,
        Query_Page_Lambda_Model = 22,
        Query_Page_Sql_Dic = 23,
        Query_Page_Sql_Model = 24,

        DataContext = 25,
        ParsingXml = 26,
        CodeFirst = 27,

        Navigate = 28,
        FastRead = 29,
        FaseWrite = 30,
        FastRead_Page = 31,

        Execute_Sql_DDL = 32,

        Navigate_Add = 33,
        Navigate_Delete = 34,
        Navigate_Update = 35,
        Navigate_AddList = 36,

        Query_Dyn_Lambda=37,
        Query_Page_Lambda_Dyn=38
    }
}
