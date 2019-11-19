using System.ComponentModel.DataAnnotations.Schema;

namespace MagBot.DatabaseContexts
{
    [Table("SelfAssignedRoles")]
    public class SelfAssignedRole
    {
        public int Id { get; set; }

        public string Name { get; set; }
        [NotMapped]
        public ulong RoleId { get { return (ulong)RoleIdLong; } set { RoleIdLong = (long)value; } }
        public long RoleIdLong { get; set; }

        public int GuildId { get; set; }
        public Guild Guild { get; set; }
    }
}