using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.WebSocket;
using Discord.Commands;
using Discord.Addons.Interactive;
using Discord;

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

        public class EnsureSpecifiedChannel : ICriterion<SocketMessage>
        {
            private readonly ulong _channelId;

            public EnsureSpecifiedChannel(IMessageChannel channel)
                => _channelId = channel.Id;

            public Task<bool> JudgeAsync(SocketCommandContext sourceContext, SocketMessage parameter)
                => Task.FromResult(parameter.Channel.Id == _channelId && parameter.Author != sourceContext.Client.CurrentUser);
        }
    }
}
