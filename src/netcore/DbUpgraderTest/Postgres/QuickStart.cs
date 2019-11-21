using System.Threading.Tasks;
using DbLight.Common;
using DbUpgrader;
using DbUpgrader.Postgres;
using NUnit.Framework;

namespace DbUpgraderTest
{
    public class QuickStart
    {
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

--|STA|VERSION|2,5
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

        [Test]
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
    }
}