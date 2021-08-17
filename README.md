
.net core orm(db first,code frist) for sqlserver mysql etl. 

nuget url: https://www.nuget.org/packages/Fast.Data.Core/

in Startup.cs Startup mothod

            Configuration = configuration;

            // old pagepackages init model Properties cahce 
            FastMap.InstanceProperties("DataModel","db.json");

            //old pagepackages init code first
            FastMap.InstanceTable("DataModel.Base", "db.json");

            //old pagepackages by Repository
            services.AddFastRedis(a => { a.Server = "127.0.0.1:6379,abortConnect=true,allowAdmin=true,connectTimeout=10000,syncTimeout=10000"; });
            services.AddFastData();
            
            //old pagepackages init map cache
            FastData.Core.FastMap.InstanceMap("dbKey", "db.json", "map.json");
            
            //old pagepackages init map cache by Resource （xml file， db.json， map.json）
            FastData.Core.FastMap.InstanceMapResource("dbKey", "db.json", "map.json");
            
               public class TestAop : FastData.Core.Aop.IFastAop
                {
                    public void After(AfterContext context)
                    {
                        throw new NotImplementedException();
                    }

                    public void Before(BeforeContext context)
                    {
                        throw new NotImplementedException();
                    }

                    public void Map(MapContext context)
                    {
                        throw new NotImplementedException();
                    }
                    
                     public void Exception(Exception ex, string name)
                    {
                        throw new NotImplementedException();
                    }
                }
            
            //new pagepackages
            services.AddFastData(new ConfigData { mapFile = "map.json", dbKey = "dbkey", IsResource = true, dbFile = "db.json",NamespaceProperties = "DataModel." });
               or
            services.AddFastData(a=> { a.mapFile = "map.json"; a.dbKey = "dbkey"; a.IsResource = true; a.dbFile = "db.json";
                a.NamespaceProperties = "DataModel."; 
                a.aop = new TestAop();
            });
  
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
              <select id="GetUser" log="true">
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
                  //<if condition="!FastUntility.Core.Base.BaseRegular.IsZhString(#areaId#, false)" prepend=" and " property="areaId" references="Fast.Untility.Core">a.areaId=?areaId</if>
                  <choose property="userNo">
                     <condition prepend=" and " property="userNo>5">a.userNo=:userNo and a.userNo=5</condition>
                     //<condition prepend=" and " property="FastUntility.Core.Base.BaseRegular.IsZhString(#userNo#, false)"  references="Fast.Untility.Core">a.userNo=:userNo and a.userNo=5</condition>
                     <condition prepend=" and " property="userNo>6">a.userNo=:userNo and a.userNo=6</condition>
                     <other prepend=" and ">a.userNo=:userNo and a.userNo=7</other><!--by above 2.3.4-->
                  </choose>                  
                 <foreach name="data" field="userId">
                    select ypxh from base_role where userId=:userId
                 </foreach>
                </dynamic>
              </select>
              
              <select id="Patient.Test">
                select * from base_user where 1=1
                <dynamic prepend="">
                  <isNotNullOrEmpty prepend=" and " property="userid">userid = :userid</isNotNullOrEmpty>
                </dynamic>
                <foreach name="data1" field="areaid" type="Test1.Model.BASE_AREA,Test1">
                  select * from base_area where areaid=:areaid
                </foreach>
                <foreach name="data2" field="roleid" type="Test1.Model.BASE_ROLE,Test1">
                  select * from base_role where roleid=:roleid
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
        param.Add(new OracleParameter { ParameterName = "userid", Value = "dd5c99f2-0892-4179-83db-c2ccf243104c" });
        var tt = FastMap.Query<TestResult>("Patient.Test", param.ToArray(), null, "test");
        
        namespace Test1.Model
        {
            public class TestResult
            {
                public string USERID { get; set; }
                public string USERPASS { get; set; }
                public string FULLNAME { get; set; }
                public string ORGID { get; set; }
                public string EXTENDORGID { get; set; }
                public string HOSPITALID { get; set; }
                public string EXTENDHOSPITALID { get; set; }
                public string AREAID { get; set; }
                public string EXTENDAREAID { get; set; }
                public string USERNO { get; set; }
                public string ROLEID { get; set; }
                public string EXTENDROLEID { get; set; }
                public string ISADMIN { get; set; }
                public string ISDEL { get; set; }
                public DateTime? ADDTIME { get; set; }
                public string ADDUSERID { get; set; }
                public string ADDUSERNAME { get; set; }
                public DateTime? DELTIME { get; set; }
                public string DELUSERID { get; set; }
                public string DELUSERNAME { get; set; }
                public List<BASE_AREA> area { get; set; }
                public List<BASE_ROLE> role { get; set; }
            }
            
            public class BASE_ROLE
            {
                public string ROLEID{ get; set; }
                public string ROLENAME{ get; set; }
                public string ROLEREMARK{ get; set; }
                public string DEFAULTPAGE{ get; set; }
                public DateTime? ADDTIME{ get; set; }
                public string ADDUSERID{ get; set; }
                public string ADDUSERNAME{ get; set; }      
            }
            
            public class BASE_AREA
            {
                public string AREAID{ get; set; }
                public string HOSPITALID{ get; set; }
                public string AREANAME{ get; set; }
                public DateTime? ADDTIME{ get; set; }
                public string ADDUSERID{ get; set; }
                public string ADDUSERNAME{ get; set; }
                public DateTime? DELTIME{ get; set; }
                public string DELUSERID{ get; set; }
                public string DELUSERNAME{ get; set; }
                public string ISDEL{ get; set; }      
            }
        }

  
