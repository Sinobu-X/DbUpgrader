using System;
using System.Collections.Generic;
using System.Text;

namespace DbUpgrader
{
    public class DbScript
    {
        public DbScriptVersionTable VersionTable{ get; set; } = new DbScriptVersionTable();
        public List<DbScriptBag> Bags{ get; set; } = new List<DbScriptBag>();
        public int LastVersion => Bags[Bags.Count - 1].ToVersion;
        public int StartVersion => Bags[0].FromVersion;

        public DbScript(string fileText){
            LoadScriptContent(fileText);
        }

        private void LoadScriptContent(string content){
            var lines = content.Replace("\r\n", "\n").Split(new []{'\n'});

            var startCount = 0;
            var isConfig = false;
            var isVersionTable = false;
            var isBag = false;

            DbScriptBag curBag = null;
            StringBuilder curBagScripts = null;

            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++){
                var line = lines[lineIndex];

                if (line.Trim().StartsWith("--//")){
                   continue;
                }

                if (line.Trim().StartsWith("--|STA|CONFIG|")){
                    if (startCount == 0 && !isConfig){
                        startCount++;
                        isConfig = true;
                        continue;
                    }

                    throw new DbInvalidScriptException(DbInvalidScriptException.UNKNOWN_LINE,
                        $"Invalid config tag at line {lineIndex + 1}.");
                }

                if (line.Trim().StartsWith("--|STA|VERSION-TABLE|")){
                    if (startCount == 1 && isConfig && !isVersionTable){
                        startCount++;
                        isVersionTable = true;
                        continue;
                    }

                    throw new DbInvalidScriptException(DbInvalidScriptException.UNKNOWN_LINE,
                        $"Invalid version table tag at line {lineIndex + 1}.");
                }

                if (isConfig && isVersionTable && line.Trim().StartsWith("--|NAME|")){
                    VersionTable.TableName = line.Trim().Substring(8);
                    continue;
                }

                if (isConfig && isVersionTable && line.Trim().StartsWith("--|CREATE|")){
                    VersionTable.CreateSql = line.Trim().Substring(10);
                    continue;
                }

                if (isConfig && isVersionTable && line.Trim().StartsWith("--|CHECK|")){
                    VersionTable.CheckSql = line.Trim().Substring(9);
                    continue;
                }

                if (isConfig && isVersionTable && line.Trim().StartsWith("--|ADD|")){
                    VersionTable.AddSql = line.Trim().Substring(7);
                    continue;
                }

                if (isConfig && isVersionTable && line.Trim().StartsWith("--|UPDATE|")){
                    VersionTable.UpdateSql = line.Trim().Substring(10);
                    continue;
                }

                if (line.Trim().StartsWith("--|STA|VERSION|")){
                    if (startCount == 0 && !isBag){
                        //ok
                    }
                    else{
                        throw new DbInvalidScriptException(DbInvalidScriptException.UNKNOWN_LINE,
                            $"Invalid bag tag at line {lineIndex + 1}.");
                    }

                    var cells = line.Trim().Substring(15).Split(new []{','});
                    if (cells.Length != 2){
                        throw new DbInvalidScriptException(DbInvalidScriptException.INVALID_START_BAG_TAG,
                            $"Invalid start bag tag at line {lineIndex + 1}.");
                    }

                    if (!int.TryParse(cells[0], out var fromVersion)){
                        throw new DbInvalidScriptException(DbInvalidScriptException.INVALID_FROM_VERSION,
                            $"Invalid from version at line {lineIndex + 1}.");
                    }

                    if (!int.TryParse(cells[1], out var toVersion)){
                        throw new DbInvalidScriptException(DbInvalidScriptException.INVALID_TO_VERSION,
                            $"Invalid to version at line {lineIndex + 1}.");
                    }

                    if (fromVersion >= toVersion){
                        throw new DbInvalidScriptException(DbInvalidScriptException.FROM_VERSION_GREATER,
                            $"To version is greater than from version at line {lineIndex + 1}.");
                    }

                    if (Bags.Count > 0 && fromVersion != Bags[Bags.Count - 1].ToVersion){
                        throw new DbInvalidScriptException(DbInvalidScriptException.FROM_VERSION_NOT_CONTINUE,
                            $"From version not equals to the last to version at line {lineIndex + 1}.");
                    }

                    curBagScripts = new StringBuilder();
                    curBag = new DbScriptBag();
                    curBag.FromVersion = fromVersion;
                    curBag.ToVersion = toVersion;

                    startCount++;
                    isBag = true;
                    continue;
                }

                if (line.Trim().StartsWith("--|END|")){
                    if (startCount == 0){
                        throw new DbInvalidScriptException(DbInvalidScriptException.LOST_START_TAG,
                            $"Start tag not found before line {lineIndex + 1}.");
                    }

                    if (isVersionTable){
                        isVersionTable = false;
                        startCount--;
                    }
                    else if (isConfig){
                        isConfig = false;
                        startCount--;
                    }
                    else if (isBag){
                        curBag.Scripts = curBagScripts.ToString();
                        Bags.Add(curBag);

                        curBag = null;
                        curBagScripts = null;

                        isBag = false;
                        startCount--;
                    }
                    else{
                        throw new DbInvalidScriptException(DbInvalidScriptException.UNKNOWN_LINE,
                            $"Invalid end tag at line {lineIndex + 1}.");
                    }

                    continue;
                }

                if (string.IsNullOrEmpty(line.Trim())){
                    if (isBag){
                        curBagScripts.AppendLine(line);
                    }
                    continue;
                }

                if (isBag){
                    curBagScripts.AppendLine(line);
                }
                else{
                    throw new DbInvalidScriptException(DbInvalidScriptException.UNKNOWN_LINE,
                        $"Unknown line at line {lineIndex + 1}.");
                }
            }

            if (startCount != 0){
                throw new DbInvalidScriptException(DbInvalidScriptException.LOST_END_TAG,
                    $"End tag not found at last.");
            }

            if (Bags.Count == 0){
                throw new DbInvalidScriptException(DbInvalidScriptException.NO_BAGS,
                    $"No Bags.");
            }
        }
    }
}