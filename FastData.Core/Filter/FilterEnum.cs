using System;
using System.Collections.Generic;
using System.Text;

namespace FastData.Core.Filter
{
    public enum FilterType
    {
        Execute_Sql_Model = 1,
        Query_List_Lambda = 2,
        Query_Dic_Lambda = 3,
        Query_DataTable_Lambda = 4,
        Query_Json_Lambda = 5,
        Query_Count_Lambda = 6,
        Query_Page_Lambda_Dic = 7,
        Query_Page_Lambda_Model = 8,
        Query_Page_Sql_Model = 9,
        Query_Page_Sql_Dic = 10
    }
}
