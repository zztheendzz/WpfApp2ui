using System;
using System.Collections.Generic;
using System.Text;

namespace WpfApp2.Services.exception
{
    public class DatabaseLockedException : Exception
    {
        public DatabaseLockedException()
            : base("Database is locked")
        {
        }
    }
}
