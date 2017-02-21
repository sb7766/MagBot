using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace MagBot
{
    public static class Extensions
    {
        public static async Task<PreconditionResult> CheckPreconditionsAsync(this ModuleInfo mod, ICommandContext context, CommandInfo command, IDependencyMap map = null)
        {
            if (map == null)
                map = DependencyMap.Empty;
            foreach(var con in mod.Preconditions)
            {
                var result = await con.CheckPermissions(context, command, map).ConfigureAwait(false);
                if (!result.IsSuccess)
                    return result;
            }
            return PreconditionResult.FromSuccess();
        }
    }
}
