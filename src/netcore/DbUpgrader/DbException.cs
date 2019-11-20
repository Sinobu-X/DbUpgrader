using System;

namespace DbUpgrader
{
    public class DbException : Exception
    {
        public DbException(string message) : base(message){
        }
    }
}