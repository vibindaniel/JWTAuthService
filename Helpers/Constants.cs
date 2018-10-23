using Microsoft.AspNetCore.Authorization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using static DuploAuth.Helpers.Constants.Strings;

namespace DuploAuth.Helpers
{
    public static class Constants
    {
        public static class Strings
        {
            public static class JwtClaimIdentifiers
            {
                public const string Rol = "rol", Id = "id";
            }

            public static class JwtClaims
            {
                public const string Admin = "admin";
                public const string SuperAdmin = "super_admin";
                public const string User = "user";
            }
        }
    }

    public class AuthorizeRolesAttribute : AuthorizeAttribute
    {
        public AuthorizeRolesAttribute(params string[] roles) : base()
        {
            Roles = string.Join(",", roles);
        }

        public AuthorizeRolesAttribute() : base()
        {
            Type type = typeof(JwtClaims);
            var values = new List<string>();
            foreach (var p in type.GetFields())
            {
                var v = p.GetValue(null);
                values.Add(v.ToString());
            }
            Roles = string.Join(",", values);
        }
    }
}