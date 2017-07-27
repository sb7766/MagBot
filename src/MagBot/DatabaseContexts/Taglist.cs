using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MagBot.DatabaseContexts
{
    [Table("TagLists")]
    public class TagList
    {
        public int Id { get; set; }

        public string Keyword { get; set; }
        public List<Tag> Tags { get; set; }

        public int GuildId { get; set; }
        public Guild Guild { get; set; }
    }
}
