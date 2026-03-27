using System;
using System.Collections.Generic;
using System.Text;
using WpfApp2.model;

namespace WpfApp2.Services.sessionService
{
    public static class SessionService
    {
        public static User CurrentUser { get; set; }

        public static bool IsLoggedIn => CurrentUser != null;

        public static void Logout()
        {
            CurrentUser = null;
        }
    }
}
