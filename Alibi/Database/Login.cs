using SQLite;

namespace Alibi.Database
{
    [Table("logins")]
    public class Login
    {
        [PrimaryKey]
        [Column("username")]
        public string UserName { get; set; }
        [Column("passhash")]
        public string PassHash { get; set; }
        [Column("permissions")]
        public int PermissionsLevel { get; set; }
    }
}