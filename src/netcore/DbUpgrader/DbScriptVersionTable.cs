namespace DbUpgrader
{
    public class DbScriptVersionTable
    {
        public string TableName{ get; set; } = "sys_version";

        public string CreateSql{ get; set; } =
            @"create table {table}(id integer not null constraint {table}_pk primary key, version integer, create_time timestamp, update_time timestamp)";

        public string AddSql{ get; set; } =
            @"INSERT INTO {table}(id, version, create_time, update_time) VALUES(1, {version}, NOW(), NOW())";

        public string CheckSql{ get; set; } = @"SELECT version FROM {table} WHERE id = 1";

        public string UpdateSql{ get; set; } =
            @"UPDATE {table} SET version = {version}, update_time = NOW() WHERE id = 1";
    }
}