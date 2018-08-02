
.net core orm(db first,code frist) for sqlserver mysql etl. 

in Startup.cs Startup mothod

            Configuration = configuration;

            //init model Properties cahce
            var test = new DataModel.Base.Base_Api();
            FastMap.InstanceProperties(AppDomain.CurrentDomain.GetAssemblies(), "DataModel", "Model.dll");

            //init code first
            FastMap.InstanceTable(AppDomain.CurrentDomain.GetAssemblies(), "DataModel.Base", "Model.dll");
            FastMap.InstanceTable(AppDomain.CurrentDomain.GetAssemblies(), "DataModel.Report", "Model.dll");

            // init map cache
            FastMap.InstanceMap();
       
in db.json 

          "Redis": { 
            "WriteServerList": "127.0.0.1:6379",
            "ReadServerList": "127.0.0.1:6379",
            "MaxWritePoolSize": 60,
            "MaxReadPoolSize": 60,
            "AutoStart": true
          },
          "SqlMap" :{"Path": [ "map/admin/Api.xml", "map/admin/Area.xml"]},          
           "DataConfig": [
              {
                "ProviderName": "MySql.Data.MySqlClient",
                "DbType": "MySql",
                "ConnStr": "Database=Cloud;Data Source=127.0.0.1;User Id=root;Password=22;CharSet=utf8;port=3306;Allow User Variables=True;pooling=true;Min Pool Size=10;Max Pool Size=100;",
                "IsOutSql": true,
                "IsOutError": true,
                "IsPropertyCache": true,
                "IsMapSave": false,
                "Flag": "?",
                "FactoryClient": "MySql.Data.MySqlClient.MySqlClientFactory",
                "Key": "Write",
                "DesignModel": "CodeFirst"
              }
            ]
      
    map xml
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
                </dynamic>
              </select>
          </sqlMap>
  
  
  
             db option
                 FastWrite.Update<Base_LogLogin>(new Base_LogLogin { LoginOutTime = DateTime.Now }, 
                     a => a.Token == item.Token, a => new { a.LoginOutTime });
                     
                 FastWrite.Add(info);
                 
                 FastMap.ExecuteMapPage(pageModel, "getuser", param.ToArray());




  
