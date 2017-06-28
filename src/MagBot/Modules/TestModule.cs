using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using MagBot.DatabaseContexts;
using Microsoft.EntityFrameworkCore;

namespace MagBot.Modules
{

    [Name("Test")]
    [RequireContext(ContextType.Guild)]
    public class TestModule : ModuleBase
    {
        [Command("test1")]
        public async Task Test1()
        {
            await ReplyAsync("Test 1");
        }

        [Command("test2")]
        [RequireContext(ContextType.Guild)]
        public async Task Test2()
        {
            await ReplyAsync("Test 2");
        }
    }
}