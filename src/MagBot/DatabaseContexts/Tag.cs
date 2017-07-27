using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace MagBot.DatabaseContexts
{
    [Table("Tags")]
    public class Tag
    {
        public int Id { get; set; }
        
        public string TagString { get; set; }

        public string TagListId { get; set; }
        public TagList TagList { get; set; }
    }
}
