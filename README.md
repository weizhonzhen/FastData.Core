
.net core orm(db first,code frist) for sqlserver mysql etl. 

nuget url: https://www.nuget.org/packages/Fast.Data.Core/

in Startup.cs Startup mothod

            Configuration = configuration;

            // old pagepackages init model Properties cahce 
            FastMap.InstanceProperties("DataModel","db.json");

            //old pagepackages init code first
            //db.json DesignModel
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
                        //throw new NotImplementedException();
                    }

                    public void Before(BeforeContext context)
                    {
                        //throw new NotImplementedException();
                    }

                    public void Exception(ExceptionContext context)
                    {
                       // throw new NotImplementedException();
                    }

                    public void MapAfter(MapAfterContext context)
                    {
                        //throw new NotImplementedException();
                    }

                    public void MapBefore(MapBeforeContext context)
                    {
                      //  throw new NotImplementedException();
                    }
                }
            
            //new pagepackages
            services.AddFastData(new ConfigData { mapFile = "map.json", dbKey = "dbkey", IsResource = true, dbFile = "db.json",NamespaceProperties = "DataModel." });
               or
            services.AddFastData(a=> { a.mapFile = "map.json"; a.dbKey = "dbkey"; a.IsResource = true; a.dbFile = "db.json";
                a.NamespaceProperties = "DataModel."; 
                a.aop = new TestAop();
                a.NamespaceService = "Test1.Service";
            });
            
            //Filter
            services.AddFastDataFilter<TestResult>(a => a.USERID != "", FilterType.Query_Page_Lambda_Model);
            services.AddFastDataFilter<BASE_AREA>(a => a.HOSPITALID != "", FilterType.Query_Page_Lambda_Model);
            
            var data1 = IFast.Query<TestResult>(a => a.ORGID == "1",null,"test").ToPage<TestResult>(page);
            var data2 = IFast.Query<TestResult>(a => a.ORGID == "1",null,"test").Filter(false).ToPage<TestResult>(page);
            
            var data1 = IFast.Queryable<TestResult>(a => a.ORGID == "1",null,"test").ToPage(page);
            var data2 = IFast.Queryable<TestResult>(a => a.ORGID == "1",null,"test").Filter(false).ToPage(page);
            
           // more db all set change
           services.AddFastDataKey(a => { a.dbKey = "Api"; });
          
interface  Service            
```csharp
    public interface TestService
    {
        [FastReadAttribute(dbKey = "Write", sql = "select * from TestResult where userId=?userId and kid=?kid")]
        List<Dictionary<string, object>> readListDic(string userId, string kid);
        //List<Dictionary<string, object>> readListDic(TestResult model);

        [FastReadAttribute(dbKey = "Write", sql = "select * from TestResult where userId=?userId and kid=?kid")]
        Dictionary<string, object> readDic(string userId, string kid);
        //Dictionary<string, object> readDic(TestResult model);

        [FastReadAttribute(dbKey = "Write", sql = "select * from TestResult where userId=?userId and kid=?kid")]
        List<TestResult> readModel(string userId, string kid);
        //List<TestResult> readModel(TestResult model);

        [FastReadAttribute(dbKey = "Write", sql = "select * from TestResult where userId=?userId and kid=?kid")]
        TestResult readListModel(string userId, string kid);
        //TestResult readListModel(TestResult model);
        
        [FastReadAttribute(dbKey = "Write", sql = "select * from TestResult where userId=?userId and kid=?kid",isPage =true)]
        PageResult<TestResult> readPage(PageModel page ,Dictionary<string, object> item);
        
        [FastReadAttribute(dbKey = "Write", sql = "select * from TestResult where userId=?userId and kid=?kid",isPage =true)]
        PageResult readPage1(PageModel page ,Dictionary<string, object> item);

        [FastWriteAttribute(dbKey = "Write", sql = "update TestResult set userName=?userName where kid=?kid")]
        WriteReturn update(string userName, string kid);
        //WriteReturn update(TestResult model);
        
        [FastMapAttribute(dbKey = "Write", xml = @"<select>select a.DNAME, a.GH, a.DID from TestResult a where rownum &lt;= 15
                            <dynamic prepend=' '>
                                <isNotNullOrEmpty prepend=' and ' property='userName'>userName=:userName'</isNotNullOrEmpty>
                                <isNotNullOrEmpty prepend=' and ' property='userId'>userId=:userId</isNotNullOrEmpty>
                            </dynamic>
                            order by a.REGISTDATE</select>",isPage =true)]
        PageResult read_MapPage(PageModel page ,Dictionary<string, object> item);
        
         [FastMapAttribute(dbKey = "Write", xml = @"<select>select a.DNAME, a.GH, a.DID from TestResult a where rownum &lt;= 15
                            <dynamic prepend=' '>
                                <isNotNullOrEmpty prepend=' and ' property='userName'>userName=:userName'</isNotNullOrEmpty>
                                <isNotNullOrEmpty prepend=' and ' property='userId'>userId=:userId</isNotNullOrEmpty>
                            </dynamic>
                            order by a.REGISTDATE</select>")]
        List<TestResult> read_Map(Dictionary<string, object> item);
    }

//ioc  
 var model = new TestResult();
 model.userName = "管理员";
 model.userId = "admin";
 model.kid = "101";
 
 var write = testService.update("管理员", "admin"); // or  testService.update(model);
 var readDic = testService.readDic("admin", "101");// or  testService.readDic(model);
 var readListDic = testService.readListDic("admin", "101");// or  testService.readListDic(model);
 var readModel = testService.readModel("admin", "101");// or  testService.readModel(model);
 var readListModel = testService.readListModel("admin", "101");// or  testService.readListModel(model);
 
 var page = new PageModel();
 page.PageSize = 2;
 var pageData = testService.readPage(page,model);
 var pageData1 = testService.readPage1(page,model); 
 var page1 = testService.read_MapPage(page,model); 
 var page2 = testService.read_Map(model);
 ```   
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
  
  
```csharp
  
             db option
                 FastWrite.Update<Base_LogLogin>(new Base_LogLogin { LoginOutTime = DateTime.Now }, 
                     a => a.Token == item.Token, a => new { a.LoginOutTime });
                     
                 FastWrite.Add(info);
                 
                 FastMap.QueryPage(pageModel, "getuser", param.ToArray());


                 var param = new List<OracleParameter>();
        param.Add(new OracleParameter { ParameterName = "userid", Value = "dd5c99f2-0892-4179-83db-c2ccf243104c" });
        var tt = FastMap.Query<TestResult>("Patient.Test", param.ToArray(), null, "test");
        
        //Navigate
        var data = FastRead.Query<TestResult>(a => a.USERID != "" , null , "test").toList<TestResult>();
        
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
                
                 //Navigate
                [NavigateType(IsAdd = true,IsUpdate =true,IsDel =true)] //add,update by PrimaryKey,delete by PrimaryKey
                public virtual List<BASE_AREA> area { get; set; }
                [NavigateType(IsAdd = true,IsUpdate =true,IsDel =true)] //add,update by PrimaryKey,delete by PrimaryKey
                public virtual List<BASE_ROLE> role { get; set; }    
                
                [NavigateType(Type = typeof(BASE_ROLE))]
                public virtual List<Dictionary<string, object>> roleList { get; set; }
                                
                [NavigateType(Type = typeof(BASE_ROLE))]
                public virtual Dictionary<string, object> roleDic { get; set; }
            }
            
            public class BASE_ROLE
            {
                //Navigate
                [Navigate(Name = nameof(TestResult.ROLEID))]
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
                //Navigate
                [Navigate(Name = nameof(TestResult.AREAID))]
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

  
