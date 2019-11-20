namespace DbUpgrader
{
    public class DbScriptBag
    {
        public int FromVersion{ get; set; }
        public int ToVersion{ get; set; }
        public string Scripts{ get; set; }
    }
}