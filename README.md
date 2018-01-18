
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
           "WriteData": [
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
            ],
           "ReadData": [
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
                "Key": "Read",
                "DesignModel": "CodeFirst"
              }
            ]
            
            
  
