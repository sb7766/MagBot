using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MagBot.DatabaseContexts
{
    [Table("Raffles")]
    public class Raffle
    {
        public int Id { get; set; }
        
        public bool Started { get; set; }
        [NotMapped]
        public ulong Owner { get { return (ulong)OwnerLong; } set { OwnerLong = (long)value; } }
        public long OwnerLong { get; set; }
        [NotMapped]
        public ulong Channel { get { return (ulong)ChannelLong; } set { ChannelLong = (long)value; } }
        public long ChannelLong { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime StartedAt { get; set; }
        public RaffleConfig Config { get; set; }
        public List<RaffleEntry> RaffleEntries { get; set; }

        public int GuildId { get; set; }
        public Guild Guild { get; set; }
    }
}
