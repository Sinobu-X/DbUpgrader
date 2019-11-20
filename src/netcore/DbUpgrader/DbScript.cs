using System;
using System.Collections.Generic;
using System.Text;

namespace DbUpgrader
{
    public class DbScript
    {
        public List<DbScriptBag> Bags{ get; set; }
        public string Table{ get; set; }
        public string Create{ get; set; }
        public string Check{ get; set; }
        public string Add{ get; set; }
        public string Update{ get; set; }

        public int LastVersion => Bags[Bags.Count - 1].ToVersion;
        public int StartVersion => Bags[0].FromVersion;

        public DbScript(string fileText){
            LoadScriptContent(fileText);
        }

        private void LoadScriptContent(string content){
            var lines = content.Replace("\r\n", "\n").Split(new char[]{'\n'});
            var bags = new List<DbScriptBag>();

            var isVersionCtrl = false;
            DbScriptBag curBag = null;
            StringBuilder curBagScripts = null;

            for (var lineIndex = 0; lineIndex < lines.Length; lineIndex++){
                var line = lines[lineIndex];

                if (line.Trim().StartsWith("--|STA|VERSION_CTL")){
                    isVersionCtrl = true;
                }
                else if (line.Trim().StartsWith("--|STA|VERSION,")){
                    if (curBag != null){
                        throw new DbInvalidScriptException(DbInvalidScriptException.LOST_END_TAG,
                            $"End bag tag not found before line {lineIndex + 1}.");
                    }

                    var cells = line.Trim().Split(new char[]{','});
                    if (cells.Length != 3){
                        throw new DbInvalidScriptException(DbInvalidScriptException.INVALID_START_TAG,
                            $"Invalid start bag tag at line {lineIndex + 1}.");
                    }

                    if (!int.TryParse(cells[1], out var fromVersion)){
                        throw new DbInvalidScriptException(DbInvalidScriptException.INVALID_FROM_VERSION,
                            $"Invalid from version at line {lineIndex + 1}.");
                    }

                    if (!int.TryParse(cells[2], out var toVersion)){
                       throw new DbInvalidScriptException(DbInvalidScriptException.INVALID_TO_VERSION,
                           $"Invalid to version at line {lineIndex + 1}.");
                    }

                    if (fromVersion >= toVersion){
                        throw new DbInvalidScriptException(DbInvalidScriptException.FROM_VERSION_GREATER,
                            $"To version is greater than from version at line {lineIndex + 1}.");
                    }

                    if (bags.Count > 0 && fromVersion != bags[bags.Count - 1].ToVersion){
                        throw new DbInvalidScriptException(DbInvalidScriptException.FROM_VERSION_NOT_CONTINUE,
                            $"From version not equals to the last to version at line {lineIndex + 1}.");
                    }

                    curBagScripts = new StringBuilder();
                    curBag = new DbScriptBag();
                    curBag.FromVersion = fromVersion;
                    curBag.ToVersion = toVersion;
                }
                else if (line.Trim().StartsWith("--|END|")){
                    if (isVersionCtrl){
                        isVersionCtrl = false;
                        continue;
                    }

                    if (curBag != null){
                        curBag.Scripts = curBagScripts.ToString();
                        bags.Add(curBag);

                        curBag = null;
                        curBagScripts = null;
                        continue;
                    }

                    throw new DbInvalidScriptException(DbInvalidScriptException.LOST_START_TAG,
                        $"Start tag not found before line {lineIndex + 1}.");
                }
                else if (string.IsNullOrEmpty(line.Trim())){
                    //continue
                }
                else if (line.Trim().StartsWith("--//")){
                    //continue
                }
                else{
                    if (isVersionCtrl){
                        if (line.Trim().StartsWith("--|TABLE|")){
                            this.Table = line.Trim().Substring(9);
                        }
                        else if (line.Trim().StartsWith("--|CREATE|")){
                            this.Create = line.Trim().Substring(10);
                        }
                        else if (line.Trim().StartsWith("--|CHECK|")){
                            this.Check = line.Trim().Substring(9);
                        }
                        else if (line.Trim().StartsWith("--|ADD|")){
                            this.Add = line.Trim().Substring(7);
                        }
                        else if (line.Trim().StartsWith("--|UPDATE|")){
                            this.Update = line.Trim().Substring(10);
                        }
                        continue;
                    }

                    if (curBag != null){
                        curBagScripts.AppendLine(line);
                        continue;
                    }

                    throw new DbInvalidScriptException(DbInvalidScriptException.UNKNOWN_LINE,
                        $"Unknown line at line {lineIndex + 1}.");
                }
            }

            if (isVersionCtrl || curBag != null){
                throw new DbInvalidScriptException(DbInvalidScriptException.LOST_END_TAG,
                    $"End tag not found at last.");
            }

            if (bags.Count == 0){
                throw new DbInvalidScriptException(DbInvalidScriptException.NO_BAGS,
                    $"No Bags.");
            }

            this.Bags = bags;
        }
    }
}