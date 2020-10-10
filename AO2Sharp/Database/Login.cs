﻿using SQLite;

namespace AO2Sharp.Database
{
    [Table("logins")]
    public class Login
    {
        [PrimaryKey]
        [Column("username")]
        public string UserName { get; set; }
        [Column("passhash")]
        public string PassHash { get; set; }
    }
}
