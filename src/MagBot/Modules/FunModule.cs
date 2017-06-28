using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using MagBot.DatabaseContexts;
using System.Diagnostics;

namespace MagBot.Modules
{
    [Name("Fun")]
    public class FunModule : ModuleBase
    {
        [Group("tag")]
        [Name("Tag")]
        [RequireContext(ContextType.Guild)]
        public class TagModule : ModuleBase
        {
            [Command("")]
            [Alias("query")]
            [Summary("Gets all tags for the specified keyword.")]
            public async Task TagQuery(string keyword)
            {
                keyword = keyword.ToLower();

                using (var db = new GuildDataContext())
                {
                    var guild = await db.Guilds.FindAsync(Context.Guild.Id);

                    await db.Entry(guild).Collection(g => g.TagLists).LoadAsync();

                    var taglist = guild.TagLists.FirstOrDefault(tl => tl.Keyword == keyword);

                    if (taglist != null)
                    {
                        await db.Entry(taglist).Collection(tl => tl.Tags).LoadAsync();

                        var tags = taglist.Tags.Select(t => t.TagString).ToList();
                        if (tags != null)
                        {
                            string tag = string.Join(", ", tags);
                            await ReplyAsync($"Tags for **{keyword}**: {tag}");
                            return;
                        }
                    }
                }

                await ReplyAsync($"No tags found for **{keyword}**");
            }

            [Command("add")]
            [Summary("Adds a tag to the specifed keyword.")]
            public async Task TagAdd(string keyword, [Remainder] string tag)
            {
                keyword = keyword.ToLower();

                using (var db = new GuildDataContext())
                {
                    var guild = await db.Guilds.FindAsync(Context.Guild.Id);

                    await db.Entry(guild).Collection(g => g.TagLists).LoadAsync();

                    var taglist = guild
                        .TagLists.FirstOrDefault(tl => tl.Keyword == keyword);

                    if (taglist != null)
                    {
                        await db.Entry(taglist).Collection(tl => tl.Tags).LoadAsync();
                        taglist.Tags.Add(new Tag { TagString = tag });
                    }
                    else
                    {
                        taglist = new TagList
                        {
                            Keyword = keyword,
                            Tags = new List<Tag> { new Tag { TagString = tag } }
                        };
                        guild.TagLists.Add(taglist);
                    }

                    await db.SaveChangesAsync();
                    await ReplyAsync($"Tag \"{tag}\" added to **{keyword}**.");
                }
            }

            [Command("remove")]
            [Alias("delete")]
            [Summary("Remove a single tag from a keyword. First tag is 0.")]
            public async Task TagRemove(string keyword, int tagIndex)
            {
                keyword = keyword.ToLower();

                using (var db = new GuildDataContext())
                {
                    var guild = await db.Guilds.FindAsync(Context.Guild.Id);

                    await db.Entry(guild).Collection(g => g.TagLists).LoadAsync();

                    var taglist = guild.TagLists.FirstOrDefault(tl => tl.Keyword == keyword);

                    if (taglist != null)
                    {
                        await db.Entry(taglist).Collection(tl => tl.Tags).LoadAsync();

                        var tag = taglist.Tags[tagIndex];

                        if (tag != null)
                        {
                            taglist.Tags.Remove(tag);
                            if (taglist.Tags.Count == 0)
                            {
                                guild.TagLists.Remove(taglist);
                            }
                            await db.SaveChangesAsync();
                            await ReplyAsync($"Tag \"{tag.TagString}\" removed from **{keyword}**.");
                        }
                        else
                        {
                            await ReplyAsync("Tag index out of range.");
                        }
                    }
                    else
                    {
                        await ReplyAsync("Keyword does not exist.");
                    }
                }
            }

            [Command("clear")]
            [Summary("Clears all of the tags for a keyword.")]
            public async Task TagClear(string keyword)
            {
                keyword = keyword.ToLower();

                using (var db = new GuildDataContext())
                {
                    var guild = await db.Guilds.FindAsync(Context.Guild.Id);

                    await db.Entry(guild).Collection(g => g.TagLists).LoadAsync();

                    var taglist = guild.TagLists.FirstOrDefault(tl => tl.Keyword == keyword);

                    if (taglist != null)
                    {
                        await db.Entry(taglist).Collection(tl => tl.Tags).LoadAsync();
                        taglist.Tags.Clear();
                        guild.TagLists.Remove(taglist);
                        await db.SaveChangesAsync();
                        await ReplyAsync($"All tags for **{keyword}** cleared.");
                    }
                    else
                    {
                        await ReplyAsync($"Keyword does not exist.");
                    }
                }
            }

            [Command("list")]
            [Summary("Sends a DM with all keywords with tags in the current guild.")]
            public async Task TagList()
            {
                using (var db = new GuildDataContext())
                {
                    var guild = await db.Guilds.FindAsync(Context.Guild.Id);

                    await db.Entry(guild).Collection(g => g.TagLists).LoadAsync();

                    var keywords = guild.TagLists.Select(tl => tl.Keyword).ToList();

                    if (keywords != null && keywords.Count != 0)
                    {
                        string list = string.Join(", ", keywords);

                        var userDM = await Context.User.CreateDMChannelAsync();

                        await userDM.SendMessageAsync($"Available keywords in {Context.Guild.Name}: {list}");
                    }
                    else
                    {
                        await ReplyAsync("No keywords found.");
                    }
                }
            }
        }
    }
}
