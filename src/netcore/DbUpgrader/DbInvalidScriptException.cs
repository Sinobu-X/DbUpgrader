using System;

namespace DbUpgrader
{
    public class DbInvalidScriptException: Exception
    {
        public int ErrorNo{ get; set; }

        public DbInvalidScriptException(int errorNo, string message) : base(message){
            ErrorNo = errorNo;
        }

        public const int LOST_END_TAG = 1;
        public const int INVALID_START_TAG = 2;
        public const int INVALID_FROM_VERSION = 3;
        public const int INVALID_TO_VERSION = 4;
        public const int FROM_VERSION_GREATER = 5;
        public const int FROM_VERSION_NOT_CONTINUE = 6;
        public const int UNKNOWN_LINE = 7;
        public const int LOST_START_TAG = 8;
        public const int NO_BAGS = 9;
    }
}