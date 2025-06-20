﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace User.Api.Common.Constants
{
    public static class RoleConstants
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string User = "User";
        public static readonly List<string> AllRoles = new List<string>
        {
            SuperAdmin,
            Admin,
            User
        };
    }
}
