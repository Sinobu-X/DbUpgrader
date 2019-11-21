using System.Threading.Tasks;
using DbLight;
using DbLight.Common;
using DbUpgrader;
using DbUpgrader.Postgres;
using NUnit.Framework;

namespace DbUpgraderTest
{
    public class TestPostgres
    {

        [Test]
        public void RemoveDb(){
            var masterCn = new DbConnection(DbDatabaseType.Postgres,
                "Host=127.0.0.1;Username=sinobu;Password=-101868;Database=postgres");

            var db = new DbContext(masterCn);
            db.ExecNoQuery("DROP DATABASE test");
        }


        [Test]
        public async Task TestDb(){
            var content = @"

--|STA|CONFIG|

--|STA|VERSION-TABLE|
--|NAME|db_version
--|CREATE|create table {table}(id integer not null constraint {table}_pk primary key, version integer, create_time timestamp, update_time timestamp)
--|ADD|INSERT INTO {table}(id, version, create_time, update_time) VALUES(1, {version}, NOW(), NOW())
--|CHECK|SELECT version FROM {table} WHERE id = 1
--|UPDATE|UPDATE {table} SET version = {version}, update_time = NOW() WHERE id = 1
--|END|

--|END|

--|STA|VERSION|1,4
create table sex
(
	sex_id integer not null
		constraint sex_pk
			primary key,
	sex_name varchar(64)
);
--|END|

--|STA|VERSION|4,6
create table sex_a
(
	sex_id integer not null
		constraint sex_a_pk
			primary key,
	sex_name varchar(64)
);
--|END|

--|STA|VERSION|6,8
--|END|
";
            var masterCn = new DbConnection(DbDatabaseType.Postgres,
                "Host=127.0.0.1;Username=sinobu;Password=-101868;Database=postgres");
            var curCn = new DbConnection(DbDatabaseType.Postgres,
                "Host=127.0.0.1;Username=sinobu;Password=-101868;Database=test");

            var upgrade = new DbUpgradeHelper(new DbScript(content), "test", masterCn, curCn);
            await upgrade.Check();
        }
    }
}