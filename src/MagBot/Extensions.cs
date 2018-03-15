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
        public static async Task<PreconditionResult> CheckPreconditionsAsync(this ModuleInfo mod, ICommandContext context, IServiceProvider provider)
        {
            foreach (PreconditionAttribute precondition in mod.Preconditions)
            {
                foreach (var cmd in mod.Commands)
                {
                    var result = await precondition.CheckPermissionsAsync(context, cmd, provider).ConfigureAwait(false);
                    if (result.IsSuccess)
                        return result;
                }
            }

            return PreconditionResult.FromError("User has no permissions for commands in this module.");
        }
    }
}
