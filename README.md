.net core orm(db first,code frist) for sqlserver mysql etl. cache for redis

in Startup.cs config
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;

            //init model cache
            var test = new DataModel.Base.Base_Api();
            LambdaMap.InstanceProperties(AppDomain.CurrentDomain.GetAssemblies(), "DataModel", "Model.dll");

            //init code first 
            LambdaMap.InstanceTable(AppDomain.CurrentDomain.GetAssemblies(), "DataModel.Base", "Model.dll");

            //init map file cahce
            LambdaMap.InstanceMap();
        }
        
in appsettings.json config
  "Redis": {
    "WriteServerList": "127.0.0.1:6379",
    "ReadServerList": "127.0.0.1:6379",
    "MaxWritePoolSize": 60,
    "MaxReadPoolSize": 60,
    "AutoStart": true
  },  
   "WriteData": [ --write db
    {
      "ProviderName": "MySql.Data.MySqlClient",
      "DbType": "MySql",
      "ConnStr": "Database=Cloud;Data Source=127.0.0.1;User Id=root;Password=test;CharSet=utf8;port=3306;Allow User Variables=True;pooling=true;Min Pool Size=10;Max Pool Size=100;",
      "IsOutSql": true,
      "IsOutError": true,
      "IsPropertyCache": true,
      "IsMapSave": false,
      "Flag": "?",
      "FactoryClient": "MySql.Data.MySqlClient.MySqlClientFactory",
      "Key": "Write",
      "DesignModel": "CodeFirst"
    }
  ],
  "ReadData": [  -- read db
    {
      "ProviderName": "MySql.Data.MySqlClient",
      "DbType": "MySql",
      "ConnStr": "Database=Cloud;Data Source=127.0.0.1;User Id=root;Password=test;CharSet=utf8;port=3306;Allow User Variables=True;pooling=true;Min Pool Size=10;Max Pool Size=100;",
      "IsOutSql": true,
      "IsOutError": true,
      "IsPropertyCache": true,
      "IsMapSave": false,
      "Flag": "?",
      "FactoryClient": "MySql.Data.MySqlClient.MySqlClientFactory",
      "Key": "Read",
      "DesignModel": "CodeFirst"
    }
  ],
   "SqlMap" :{ --map xml 
    "Path": [
      "map/admin/Sys.xml",
      "map/admin/User.xml"
    ]
  }
  
 User.xml:
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
    </dynamic>
  </select>
</sqlMap>
  
 
 -- read map xml 
 LambdaMap.ExecuteMap<SysFunModel>("getPowerMenu", param.ToArray());
  
 -- lambda read
 var user = LambdaRead.Query<Base_User>(a => a.UserNo == item.UserNo && a.IsDel == "0",a=>{a.UserNo,a.IsDel}).ToItem<Base_User>();
 
 --lambda write
 LambdaWrite.Add(user);
 LambdaWrite.Update<Base_LogLogin>(new Base_LogLogin { LoginOutTime = DateTime.Now }, a => a.Token == item.Token, a => new { a.LoginOutTime });
 LambdaWrite.Del<Base_LogLogin>( a => a.Token == item.Token);
