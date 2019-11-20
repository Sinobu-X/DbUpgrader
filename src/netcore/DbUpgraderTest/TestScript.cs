using System;
using DbUpgrader;
using Newtonsoft.Json;
using NUnit.Framework;

namespace DbUpgraderTest
{
    public class TestScript
    {
        [Test]
        public void Test(){
            {
                var content = @"";
                Assert.AreEqual(Assert.Catch<DbInvalidScriptException>(() => {
                    var dbScript = new DbScript(content);
                }).ErrorNo, DbInvalidScriptException.NO_BAGS);
            }

            {
                var content = @"abcxx";
                Assert.AreEqual(Assert.Catch<DbInvalidScriptException>(() => {
                    var dbScript = new DbScript(content);
                }).ErrorNo, DbInvalidScriptException.UNKNOWN_LINE);
            }

            {
                var content = @"--|END|";
                Assert.AreEqual(Assert.Catch<DbInvalidScriptException>(() => {
                    var dbScript = new DbScript(content);
                }).ErrorNo, DbInvalidScriptException.LOST_START_TAG);
            }

            {
                var content = @"--|STA|VERSION,";
                Assert.AreEqual(Assert.Catch<DbInvalidScriptException>(() => {
                    var dbScript = new DbScript(content);
                }).ErrorNo, DbInvalidScriptException.INVALID_START_TAG);
            }

            {
                var content = @"--|STA|VERSION,d,d,d";
                Assert.AreEqual(Assert.Catch<DbInvalidScriptException>(() => {
                    var dbScript = new DbScript(content);
                }).ErrorNo, DbInvalidScriptException.INVALID_START_TAG);
            }

            {
                var content = @"--|STA|VERSION,a,b";
                Assert.AreEqual(Assert.Catch<DbInvalidScriptException>(() => {
                    var dbScript = new DbScript(content);
                }).ErrorNo, DbInvalidScriptException.INVALID_FROM_VERSION);
            }

            {
                var content = @"--|STA|VERSION,1,b";
                Assert.AreEqual(Assert.Catch<DbInvalidScriptException>(() => {
                    var dbScript = new DbScript(content);
                }).ErrorNo, DbInvalidScriptException.INVALID_TO_VERSION);
            }

            {
                var content = @"--|STA|VERSION,1,4";
                Assert.AreEqual(Assert.Catch<DbInvalidScriptException>(() => {
                    var dbScript = new DbScript(content);
                }).ErrorNo, DbInvalidScriptException.LOST_END_TAG);
            }

            {
                var content = @"--|STA|VERSION,5,4";
                Assert.AreEqual(Assert.Catch<DbInvalidScriptException>(() => {
                    var dbScript = new DbScript(content);
                }).ErrorNo, DbInvalidScriptException.FROM_VERSION_GREATER);
            }

            {
                var content = @"--|STA|VERSION,1,4
--|END|

--dd
";
                var ex = Assert.Catch<DbInvalidScriptException>(() => {
                    var dbScript = new DbScript(content);
                });
                Assert.AreEqual(ex.ErrorNo, DbInvalidScriptException.UNKNOWN_LINE);
            }

            {
                var content = @"--|STA|VERSION,1,4
--|END|

--//command
";
                var dbScript = new DbScript(content);
            }

            {
                var content = @"
--|STA|VERSION,1,4
--|END|
--|STA|VERSION,1,4
--|END|
";
                var ex = Assert.Catch<DbInvalidScriptException>(() => {
                    var dbScript = new DbScript(content);
                });
                Assert.AreEqual(ex.ErrorNo, DbInvalidScriptException.FROM_VERSION_NOT_CONTINUE);
            }

            {
                var content = @"
--|STA|VERSION,1,4
--|END|
--|STA|VERSION,4,6
--|END|
";
                var dbScript = new DbScript(content);
                Assert.AreEqual(dbScript.Bags.Count, 2);
            }

            {
                var content = @"
--|STA|VERSION,1,4
aa
bb
cc
--|END|
--|STA|VERSION,4,6
aa
bb
cc
--|END|
";
                var dbScript = new DbScript(content);
                Assert.AreEqual(dbScript.Bags.Count, 2);
                Console.WriteLine(JsonConvert.SerializeObject(dbScript.Bags));
            }

            {
                var content = @"
--|STA|VERSION_CTL
--|TABLE|sys_version
--|CREATE|create table sys_version(id integer not null constraint sys_version_pk primary key, version integer, create_time time, update_time time);
--|CHECK|SELECT version FROM sys_version WHERE id = 1
--|ADD|INSERT INTO sys_version(id, version, create_time, update_time) VALUES(1, {version}, NOW(), NOW())
--|UPDATE|UPDATE sys_version SET version = {version} WHERE id = 1
--|END|

--|STA|VERSION,1,4
aa
bb
cc
--|END|
--|STA|VERSION,4,6
aa
bb
cc
--|END|
";
                var dbScript = new DbScript(content);
                Assert.AreEqual(dbScript.Bags.Count, 2);
                Console.WriteLine(JsonConvert.SerializeObject(dbScript));
            }
        }


    }
}