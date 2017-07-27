using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MagBot.DatabaseContexts
{
    [Table("BlacklistedRaffleUsers")]
    public class BlacklistedRaffleUser
    {
        public int Id { get; set; }
        
        [NotMapped]
        public ulong UserId { get { return (ulong)UserIdLong; } set { UserIdLong = (long)value; } }
        private long UserIdLong { get; set; }

        public RaffleConfig RaffleConfig { get; set; }
        public int RaffleConfigId { get; set; }
    }
}
