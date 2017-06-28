using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;

namespace MagBot
{
    public static class Extensions
    {
        public static async Task<PreconditionResult> CheckPreconditionsAsync(this ModuleInfo mod, ICommandContext context, IDependencyMap map = null)
        {
            if (map == null)
                map = DependencyMap.Empty;

            foreach (PreconditionAttribute precondition in mod.Preconditions)
            {
                var result = await precondition.CheckPermissions(context, mod.Commands.FirstOrDefault(), map).ConfigureAwait(false);
                if (!result.IsSuccess)
                    return result;
            }

            return PreconditionResult.FromSuccess();
        }
    }
}
