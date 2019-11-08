
.net core orm(db first,code frist) for sqlserver mysql etl. 

nuget url: https://www.nuget.org/packages/Fast.Data.Core/

in Startup.cs Startup mothod

            Configuration = configuration;

            //init model Properties cahce
            FastMap.InstanceProperties("DataModel", "Model.dll");

            //init code first
            FastMap.InstanceTable("DataModel.Base", "Model.dll");
            FastMap.InstanceTable("DataModel.Report", "Model.dll");

            // init map cache
            FastMap.InstanceMap();
       
in db.json         
```csharp
 {      
           "DataConfig": [
              {
                "ProviderName": "MySql.Data",
                "DbType": "MySql",
                "ConnStr": "Database=Cloud;Data Source=127.0.0.1;User Id=root;Password=22;CharSet=utf8;port=3306;Allow User Variables=True;pooling=true;Min Pool Size=10;Max Pool Size=100;",
                "IsOutSql": true,
                "IsOutError": true,
                "IsPropertyCache": true,
                "IsMapSave": false,
                "Flag": "?",
                "FactoryClient": "MySql.Data.MySqlClient.MySqlClientFactory",
                "Key": "Write",
                "DesignModel": "CodeFirst",
                "SqlErrorType ":"db",--db,file
                "CacheType":"web",--redis,web
                "IsUpdateCache": false --is auto update cache
              }
            ]
      }
```
  in map.json
```csharp
"SqlMap" :{"Path": [ "map/admin/Api.xml", "map/admin/Area.xml"]}
```
 

       map xml
      ```xml
    <?xml version="1.0" encoding="utf-8" ?>
            <sqlMap>
              <select id="GetUser">
                select a.* from base_user a
                <dynamic prepend=" where 1=1">
                  <isPropertyAvailable prepend=" and " property="userId">a.userId=?userId</isPropertyAvailable>
                  <isEqual compareValue="5" prepend=" and " property="userName">a.userName=?userName</isEqual>
                  <isNotEqual compareValue="5" prepend=" and " property="fullName">a.fullName=?fullName</isNotEqual>
                  <isGreaterThan compareValue="5" prepend=" and " property="orgId">a.orgId=?orgId</isGreaterThan>
                  <isLessThan compareValue="5" prepend=" and " property="userNo">a.userNo=?userNo</isLessThan>
                  <isNullOrEmpty prepend=" and " property="roleId">a.roleId=?roleId</isNullOrEmpty>
                  <isNotNullOrEmpty prepend=" and " property="isAdmin">a.isAdmin=?isAdmin</isNotNullOrEmpty>
                  <if condition="areaId>8" prepend=" and " property="areaId">a.areaId=?areaId</if>
                  <choose property="userNo">
                     <condition prepend=" and " property="userNo>5">a.userNo=:userNo and a.userNo=5</condition>
                     <condition prepend=" and " property="userNo>6">a.userNo=:userNo and a.userNo=6</condition>
                  </choose>                  
                 <foreach name="data" field="userId">
                    select ypxh from base_role where userId=:userId
                 </foreach>
                </dynamic>
              </select>
              
              <select id="Patient.NowAuditList">
                select cfsb,brxm from ms_cf01 where 1=1
                <dynamic prepend="">
                  <isNotNullOrEmpty prepend=" and " property="brid">brid = :brid</isNotNullOrEmpty>
                </dynamic>
                <foreach name="data" field="cfsb" type="Test1.Model.MS_CF02,Test1">
                  select ypxh from ms_cf02 where cfsb=:cfsb
                </foreach>
            </select>
          </sqlMap>
  
  
```
  
             db option
                 FastWrite.Update<Base_LogLogin>(new Base_LogLogin { LoginOutTime = DateTime.Now }, 
                     a => a.Token == item.Token, a => new { a.LoginOutTime });
                     
                 FastWrite.Add(info);
                 
                 FastMap.QueryPage(pageModel, "getuser", param.ToArray());


                 var param = new List<OracleParameter>();
                    param.Add(new OracleParameter { ParameterName = "brid", Value = "550010" });
                    var tt = FastMap.Query<TestResult>("Patient.NowAuditList", param.ToArray(), null, "test");

                    namespace Test1.Model
                    {
                        public class TestResult
                        {
                            public decimal? CFSB { get; set; }

                            public string BRXM { get; set; }

                            public List<MS_CF02> leaf { get; set; }
                        }

                        public class MS_CF02
                        {
                            public decimal? YPXH{ get; set; }
                        }
                    }

  
