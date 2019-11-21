# DbUpgrader
Database upgrade helper for .NET Core, support Postgres

## Nuget
```csharp
Install-Package DbUpgraderCore -Version 1.0.1

dotnet add package DbUpgraderCore --version 1.0.0

<PackageReference Include="DbUpgraderCore" Version="1.0.0" />

paket add DbUpgraderCore --version 1.0.0
```


## Quick Start

#### Prepare Upgrade Script
```csharp
private string GetScript(){
    return @"

--|STA|CONFIG|

--|STA|VERSION-TABLE|
--|NAME|sys_version
--|END|

--|END|

--|STA|VERSION|1,2
create table city
(
	city_id integer not null
		constraint city_pk
			primary key,
	city_name varchar(64)
);
--|END|

--|STA|VERSION|3,5
create table province
(
	province_id integer not null
		constraint province_pk
			primary key,
	province_name varchar(64)
);
--|END|

";
}
```

#### Upgrade
```csharp
public async Task Upgrade(){
    var masterCn = new DbConnection(DbDatabaseType.Postgres,
        "Host=127.0.0.1;Username=test;Password=test;Database=postgres");
    var curCn = new DbConnection(DbDatabaseType.Postgres,
        "Host=127.0.0.1;Username=test;Password=test;Database=test");

    var script = new DbScript(GetScript());
    var dbName = "test";

    var upgrade = new DbUpgradeHelper(script,dbName, masterCn, curCn);
    await upgrade.CheckAndUpgrade();
}
```
