using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MagBot.DatabaseContexts
{
    [Table("Guilds")]
    public class Guild
    {
        public int Id { get; set; }

        [NotMapped]
        public ulong DiscordId { get { return (ulong)DiscordIdLong; } set { DiscordIdLong = (long)value; } }
        public long DiscordIdLong { get; set; }

        public List<TagList> TagLists { get; set; }
        public List<Raffle> Raffles { get; set; }
    }
}
