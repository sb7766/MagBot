using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using MagBot.DatabaseContexts;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

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
            private readonly GuildDataContext _sunburstdb;

            public TagModule(GuildDataContext sunburstdb)
            {
                _sunburstdb = sunburstdb;
            }

            [Command("")]
            [Alias("query")]
            [Summary("Gets all tags for the specified keyword.")]
            public async Task TagQuery(string keyword)
            {
                keyword = keyword.ToLower();

                var guild = await _sunburstdb.Guilds.FindAsync(Context.Guild.Id);

                await _sunburstdb.Entry(guild).Collection(g => g.TagLists).LoadAsync();

                var taglist = guild.TagLists.FirstOrDefault(tl => tl.Keyword == keyword);

                if (taglist != null)
                {
                    await _sunburstdb.Entry(taglist).Collection(tl => tl.Tags).LoadAsync();

                    var tags = taglist.Tags.Select(t => t.TagString).ToList();
                    if (tags != null)
                    {
                        string tag = string.Join(", ", tags);
                        await ReplyAsync($"Tags for **{keyword}**: {tag}");
                        return;
                    }
                }

                await ReplyAsync($"No tags found for **{keyword}**");
            }

            [Command("add")]
            [Summary("Adds a tag to the specifed keyword.")]
            public async Task TagAdd(string keyword, [Remainder] string tag)
            {
                keyword = keyword.ToLower();

                var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(gu => gu.DiscordId == Context.Guild.Id);

                await _sunburstdb.Entry(guild).Collection(g => g.TagLists).LoadAsync();

                var taglist = guild
                    .TagLists.FirstOrDefault(tl => tl.Keyword == keyword);

                if (taglist != null)
                {
                    await _sunburstdb.Entry(taglist).Collection(tl => tl.Tags).LoadAsync();
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

                await _sunburstdb.SaveChangesAsync();
                await ReplyAsync($"Tag \"{tag}\" added to **{keyword}**.");
            }

            [Command("remove")]
            [Alias("delete")]
            [Summary("Remove a single tag from a keyword. First tag is 0.")]
            public async Task TagRemove(string keyword, int tagIndex)
            {
                keyword = keyword.ToLower();

                var guild = await _sunburstdb.Guilds.FindAsync(Context.Guild.Id);

                await _sunburstdb.Entry(guild).Collection(g => g.TagLists).LoadAsync();

                var taglist = guild.TagLists.FirstOrDefault(tl => tl.Keyword == keyword);

                if (taglist != null)
                {
                    await _sunburstdb.Entry(taglist).Collection(tl => tl.Tags).LoadAsync();

                    var tag = taglist.Tags[tagIndex];

                    if (tag != null)
                    {
                        taglist.Tags.Remove(tag);
                        if (taglist.Tags.Count == 0)
                        {
                            guild.TagLists.Remove(taglist);
                        }
                        await _sunburstdb.SaveChangesAsync();
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

            [Command("clear")]
            [Summary("Clears all of the tags for a keyword.")]
            public async Task TagClear(string keyword)
            {
                keyword = keyword.ToLower();

                var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(gu => gu.DiscordId == Context.Guild.Id);

                await _sunburstdb.Entry(guild).Collection(g => g.TagLists).LoadAsync();

                var taglist = guild.TagLists.FirstOrDefault(tl => tl.Keyword == keyword);

                if (taglist != null)
                {
                    await _sunburstdb.Entry(taglist).Collection(tl => tl.Tags).LoadAsync();
                    taglist.Tags.Clear();
                    guild.TagLists.Remove(taglist);
                    await _sunburstdb.SaveChangesAsync();
                    await ReplyAsync($"All tags for **{keyword}** cleared.");
                }
                else
                {
                    await ReplyAsync($"Keyword does not exist.");
                }
            }

            [Command("list")]
            [Summary("Sends a DM with all keywords with tags in the current guild.")]
            public async Task TagList()
            {
                var guild = await _sunburstdb.Guilds.FirstOrDefaultAsync(gu => gu.DiscordId == Context.Guild.Id);

                await _sunburstdb.Entry(guild).Collection(g => g.TagLists).LoadAsync();

                var keywords = guild.TagLists.Select(tl => tl.Keyword).ToList();

                if (keywords != null && keywords.Count != 0)
                {
                    string list = string.Join(", ", keywords);

                    var userDM = await Context.User.GetOrCreateDMChannelAsync();

                    await userDM.SendMessageAsync($"Available keywords in {Context.Guild.Name}: {list}");
                }
                else
                {
                    await ReplyAsync("No keywords found.");
                }
            }
        }

        [Command("mlem")]
        [Summary("Don't mlem me, I'm shy..")]
        public async Task Mlem()
        {
            await ReplyAsync("*eeps and hides under his wizard robe*");
        }

        [Command("roll")]
        [Summary("Generate a random number. You can use a max, min and max, or d20 format.")]
        public async Task Roll(int max)
        {
            var rand = new Random();
            int num = rand.Next(1, max);
            await ReplyAsync($"{Context.User.Mention} rolled a {num}!");
        }

        [Command("roll")]
        public async Task Roll(int min, int max)
        {
            var rand = new Random();
            int num = rand.Next(min, max);
            await ReplyAsync($"{Context.User.Mention} rolled a {num}!");
        }

        [Command("roll")]
        public async Task Roll([Remainder] string d20roll)
        {
            var roll = D20Roll.Parse(d20roll);
            await ReplyAsync($"Roll for {Context.User.Mention}: {roll.Roll()}");
        }
    }

    public class D20Roll
    {
        public int DiceCount { get; set; }
        public int DieNumber { get; set; }
        public string Modifier { get; set; }
        public int ModNum { get; set; }
        public string Op { get; set; }
        public int Difficulty { get; set; }

        public D20Roll(int diceCount, int dieNumber, string modifier = null, int modNum = 0, string op = null, int difficulty = 0)
        {
            DiceCount = diceCount;
            DieNumber = dieNumber;
            Modifier = modifier;
            ModNum = modNum;
            Op = op;
            Difficulty = difficulty;
        }

        public static D20Roll Parse(string input)
        {
            try
            {
                // 3d10 + 4 >= 20
                // 3d10+4>=20
                // int int string int string int
                char[] opChars = { '=', '<', '>', '!' };
                char[] modChars = { '+', '-' };

                int dPos = input.IndexOf('d');

                string op = "";
                int difficulty = 0;
                if (input.Any(c => opChars.Any(opc => opc == c)))
                {
                    int pos = input.IndexOfAny(opChars);
                    int initPos = pos;
                    op += input.ElementAtOrDefault(pos);
                    pos++;
                    if (opChars.Any(opc => opc == input.ElementAtOrDefault(pos)))
                    {
                        op += input.ElementAtOrDefault(pos);
                        pos++;
                    }

                    int.TryParse(input.Substring(pos).Trim(), out difficulty);
                    input = input.Remove(initPos);
                }

                string mod = "";
                int modNum = 0;
                if (input.Any(c => modChars.Any(opc => opc == c)))
                {
                    int pos = input.IndexOfAny(modChars);
                    int initPos = pos;
                    mod += input.ElementAtOrDefault(pos);
                    pos++;

                    int.TryParse(input.Substring(pos).Trim(), out modNum);
                    input = input.Remove(initPos);
                }

                int.TryParse(input.Substring(0, dPos), out int diceCount);
                int.TryParse(input.Substring(dPos + 1).Trim(), out int dieNum);

                return new D20Roll(diceCount, dieNum, mod, modNum, op, difficulty);
            }
            catch
            {
                throw new Exception("Failed to parse string.");
            }
        }

        public string Roll()
        {
            var rand = new Random();
            List<int> rolls = new List<int>();
            for (int i = 0; i < DiceCount; i++)
            {
                rolls.Add(rand.Next(1, DieNumber));
            }

            int total = rolls.Sum();
            int subtotal = total;

            if (Modifier != null && Modifier != "")
            {
                switch (Modifier)
                {
                    case "+":
                        total += ModNum;
                        break;
                    case "-":
                        total -= ModNum;
                        break;
                    default:
                        throw new Exception("Unable to read modifier.");
                }
                
            }

            if (Op != null && Op != "")
            {
                bool success = false;
                switch (Op)
                {
                    case "<":
                        success = total < Difficulty;
                        break;
                    case ">":
                        success = total > Difficulty;
                        break;
                    case "<=":
                        success = total <= Difficulty;
                        break;
                    case ">=":
                        success = total >= Difficulty;
                        break;
                    case "==":
                        success = total == Difficulty;
                        break;
                    case "!=":
                        success = total != Difficulty;
                        break;
                    default:
                        throw new Exception("Unable to read operator.");
                }

                string result = "";
                if (success)
                    result = "**[Success]** ";
                else
                    result = "**[Failure]** ";

                if (Modifier != null && Modifier != "")
                {
                    result += $"{total} {Op} {Difficulty} <-- [{string.Join("+", rolls)}]{DiceCount}d{DieNumber}" +
                        $" {Modifier} {ModNum} {Op} {Difficulty}";
                }
                else
                {
                    result += $"{total} {Op} {Difficulty} <-- [{string.Join("+", rolls)}]{DiceCount}d{DieNumber} {Op} {Difficulty}";
                }

                return result;
            }
            else if (Modifier != null && Modifier != "")
            {
                return $"{total} <-- [{string.Join("+", rolls)}]{DiceCount}d{DieNumber} {Modifier} {ModNum}";
            }
            else
            {
                return $"{total} <-- [{string.Join("+", rolls)}]{DiceCount}d{DieNumber}";
            }
        }
    }
}