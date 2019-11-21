using System;
using System.Threading.Tasks;
using DbLight;
using DbLight.Common;

namespace DbUpgrader.Postgres
{
    public class DbUpgradeHelper
    {
        private DbScript _script;
        private string _dbName;
        private DbConnection _masterDbCn;
        private DbConnection _curDbCn;

        public DbUpgradeHelper(DbScript script, string dbName,
            DbConnection masterDbConnection, DbConnection curDbConnection){
            _script = script;
            _dbName = dbName;
            _masterDbCn = masterDbConnection;
            _curDbCn = curDbConnection;
        }

        public async Task CheckAndUpgrade(){
            if (!await CheckDbExist()){
                await CreateDb();
                return;
            }

            if (await CheckDbStatus()){
                return;
            }

            await UpgradeDb();
        }

        public async Task<bool> CheckDbExist(){
            var db = new DbContext(_masterDbCn);
            var dt = await db.ExecQueryToDataTableAsync(
                $"select 1 from pg_database where datname ilike '{_dbName}'");
            return dt.Rows.Count > 0;
        }

        public async Task CreateDb(){
            var db = new DbContext(_masterDbCn);
            await db.ExecNoQueryAsync($"create database \"{_dbName}\" Encoding = 'UTF8';");

            await CreateVersionTable();
            await UpgradeDb();
        }

        private async Task CreateVersionTable(){
            using (var db = new DbContext(_curDbCn)){
                await db.BeginTransactionAsync();

                await db.ExecNoQueryAsync(_script.VersionTable.CreateSql
                    .Replace("{table}", _script.VersionTable.TableName));

                await db.ExecNoQueryAsync(_script.VersionTable.AddSql
                    .Replace("{table}", _script.VersionTable.TableName)
                    .Replace("{version}", _script.StartVersion.ToString()));

                db.Commit();
            }
        }

        public async Task<bool> CheckDbStatus(){
            var db = new DbContext(_curDbCn);

            {
                var sql =
                    $"SELECT 1 FROM information_schema.tables WHERE table_schema = 'public' AND table_name = '{_script.VersionTable.TableName}'";
                var dt = await db.ExecQueryToDataTableAsync(sql);
                if (dt.Rows.Count == 0){
                    await CreateVersionTable();
                }
            }

            {
                var dt = await db.ExecQueryToDataTableAsync(_script.VersionTable.CheckSql
                    .Replace("{table}", _script.VersionTable.TableName));
                return dt.Rows.Count == 1 && Convert.ToInt32(dt.Rows[0][0]) >= _script.LastVersion;
            }
        }

        public async Task UpgradeDb(){
            var db = new DbContext(_curDbCn);

            int oldVersion;
            {
                var dt = await db.ExecQueryToDataTableAsync(_script.VersionTable.CheckSql
                    .Replace("{table}", _script.VersionTable.TableName));
                if (dt.Rows.Count == 1){
                    oldVersion = Convert.ToInt32(dt.Rows[0][0]);
                }
                else{
                    throw new DbException("Invalid db status, please check db status again.");
                }
            }

            var startBagIndex = -1;
            for (var i = 0; i < _script.Bags.Count; i++){
                if (_script.Bags[i].FromVersion == oldVersion){
                    startBagIndex = i;
                    break;
                }
            }

            if (startBagIndex == -1){
                throw new DbException("Unknown version in database.");
            }

            using (var dbx = new DbContext(_curDbCn)){
                await dbx.BeginTransactionAsync();

                for (var i = startBagIndex; i < _script.Bags.Count; i++){
                    await dbx.ExecNoQueryAsync(_script.Bags[i].Scripts);
                }

                await db.ExecNoQueryAsync(_script.VersionTable.UpdateSql
                    .Replace("{table}", _script.VersionTable.TableName)
                    .Replace("{version}", _script.LastVersion.ToString()));

                dbx.Commit();
            }
        }
    }
}