
.net core orm(db first,code frist) for sqlserver mysql etl. cache for redis

in Startup.cs Startup mothod

            Configuration = configuration;

            //init model Properties cahce
            var test = new DataModel.Base.Base_Api();
            LambdaMap.InstanceProperties(AppDomain.CurrentDomain.GetAssemblies(), "DataModel", "Model.dll");

            //init code first
            LambdaMap.InstanceTable(AppDomain.CurrentDomain.GetAssemblies(), "DataModel.Base", "Model.dll");
            LambdaMap.InstanceTable(AppDomain.CurrentDomain.GetAssemblies(), "DataModel.Report", "Model.dll");

            // init map cache
            LambdaMap.InstanceMap();
       
in appsettings.json 

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
                select a.userid,a.fullname,c.orgname,a.userno,a.isadmin,b.rolename
                from base_user a
                left join base_role b on a.roleid=b.roleid
                left join base_org c on a.orgid=c.orgid and c.isactive='0' and c.IsDel='0'
                <dynamic prepend=" where 1=1">
                  <isPropertyAvailable prepend=" and " property="userId">a.userId=?userId</isPropertyAvailable>
                  <isPropertyAvailable prepend=" and " property="userName">a.userName=?userName</isPropertyAvailable>
                  <isPropertyAvailable prepend=" and " property="fullName">a.fullName=?fullName</isPropertyAvailable>
                  <isPropertyAvailable prepend=" and " property="orgId">a.orgId=?orgId</isPropertyAvailable>
                  <isPropertyAvailable prepend=" and " property="userNo">a.userNo=?userNo</isPropertyAvailable>
                  <isPropertyAvailable prepend=" and " property="roleId">a.roleId=?roleId</isPropertyAvailable>
                  <isPropertyAvailable prepend=" and " property="isAdmin">a.isAdmin=?isAdmin</isPropertyAvailable>
                  <isPropertyAvailable prepend=" and " property="areaId">a.areaId=?areaId</isPropertyAvailable>
                  <isPropertyAvailable prepend=" and " property="isDel">a.isDel=?isDel</isPropertyAvailable>
                </dynamic>
              </select>
          </sqlMap>
  
  
  
             db option
                 LambdaWrite.Update<Base_LogLogin>(new Base_LogLogin { LoginOutTime = DateTime.Now }, 
                     a => a.Token == item.Token, a => new { a.LoginOutTime });
                     
                 LambdaWrite.Add(info);
                 
                 LambdaMap.ExecuteMapPage(pageModel, "getuser", param.ToArray());




  
