using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MagBot.DatabaseContexts
{
    public class Guild
    {
        public ulong GuildId { get; set; }

        public List<TagList> TagLists { get; set; }
    }
}
