using System;
using System.Collections.Generic;
using System.Text;
using SQLite;

namespace AO2Sharp.Database
{
    [Table("logins")]
    public class Login
    {
        [Column("username")]
        public string UserName { get; set; }
        [Column("salt")]
        public string PassSalt { get; set; }
        [Column("passhash")]
        public string PassHash { get; set; }
    }
}
