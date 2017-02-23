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

        [Command("dbtests")]
        [RequireContext(ContextType.Guild)]
        public async Task DbTestS(string keyword, string tag)
        {
            string _keyword = keyword.ToLower();

            using (var db = new GuildDataContext())
            {
                var guild = await db.Guilds.FindAsync(Context.Guild.Id);

                await db.Entry(guild).Collection(g => g.TagLists).LoadAsync();

                var taglist = guild
                    .TagLists.FirstOrDefault(tl => tl.Keyword == _keyword);

                if (taglist != null)
                {
                    await db.Entry(taglist).Collection(tl => tl.Tags).LoadAsync();
                    taglist.Tags.Add(new Tag { TagString = tag });
                }
                else
                {
                    taglist = new TagList
                    {
                        Keyword = _keyword,
                        Tags = new List<Tag> { new Tag { TagString = tag } }
                    };
                    guild.TagLists.Add(taglist);
                }

                await db.SaveChangesAsync();
                await ReplyAsync($"Tag \"{tag}\" added to **{_keyword}**");
            }
        }

        [Command("dbtestq")]
        [RequireContext(ContextType.Guild)]
        public async Task DbTestQ(string keyword)
        {
            string _keyword = keyword.ToLower();

            using (var db = new GuildDataContext())
            {
                var guild = await db.Guilds.FindAsync(Context.Guild.Id);

                await db.Entry(guild).Collection(g => g.TagLists).LoadAsync();

                var taglist = guild.TagLists.FirstOrDefault(tl => tl.Keyword == _keyword);

                if (taglist != null)
                {
                    await db.Entry(taglist).Collection(tl => tl.Tags).LoadAsync();

                    var tags = taglist.Tags.Select(t => t.TagString).ToList();
                    if (tags != null)
                    {
                        string tag = string.Join(", ", tags);
                        await ReplyAsync($"Tags for **{_keyword}**: {tag}");
                        return;
                    }
                }
            }

            await ReplyAsync($"No tags found for **{_keyword}**");
        }
    }
}