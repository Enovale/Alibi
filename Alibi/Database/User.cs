using System;
using SQLite;

namespace Alibi.Database
{
    [Table("users")]
    public class User
    {
        [PrimaryKey, Indexed]
        [Column("hwid")]
        public string Hwid { get; set; }
        [Column("ips")]
        public string Ips { get; set; }
        [Column("banned")]
        public bool Banned { get; set; }
        [Column("banreason")]
        public string BanReason { get; set; }
        [Column("expiration")]
        public DateTime? BanExpiration { get; set; }
    }
}