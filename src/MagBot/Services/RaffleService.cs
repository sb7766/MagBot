using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using MagBot.DatabaseContexts;
using Hangfire;

namespace MagBot.Services
{
    public class RaffleService
    {
        public async Task New(CommandContext context, string length, int winners)
        {
            var lengthTime = await ParseLength(length);

            Raffle raffle = new Raffle {
                Config = new RaffleConfig
                {
                    Length = lengthTime, WinnerCount = winners
                }
            };

            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffles = guild.Raffles;

                raffles.Add(raffle);
                raffle.LocalId = raffles.IndexOf(raffle) + 1;
                raffle.RaffleOwner = (long)context.User.Id;

                await db.SaveChangesAsync();

                int raffleId = raffle.RaffleId;

                var id = BackgroundJob.Schedule(() => Cancel(context, raffleId), TimeSpan.FromSeconds(15));

                raffle.HangfireId = int.Parse(id);

                await db.SaveChangesAsync();
            }

            await context.Channel.SendMessageAsync($"Raffle created. To start the raffle use `m!raffle start {raffle.LocalId}`. You can configure additional settings with the sub-commands of 'm!raffle config'. If you do not start the raffle within 15 minutes, it will be cancelled.");
        }

        private Task<TimeSpan> ParseLength(string length)
        {
            TimeSpan lengthTime = new TimeSpan();

            char last = length.LastOrDefault();
            length = length.TrimEnd(last);

            double lengthNum;
            double.TryParse(length, out lengthNum);
            
            if(lengthNum.Equals(null))
            {
                throw new Exception("Unable to parse length!");
            }
            else
            {
                switch (last)
                {
                    case 's':
                        lengthTime = TimeSpan.FromSeconds(lengthNum);
                        break;
                    case 'm':
                        lengthTime = TimeSpan.FromMinutes(lengthNum);
                        break;
                    case 'h':
                        lengthTime = TimeSpan.FromHours(lengthNum);
                        break;
                    default:
                        throw new Exception("Unable to parse length!");
                }

                return Task.FromResult(lengthTime);
            }
        }

        public async Task Cancel(CommandContext context, int id)
        {
            using (var db = new GuildDataContext())
            {
                var guild = db.Guilds.FirstOrDefault(g => g.GuildId == context.Guild.Id);
                await db.Entry(guild).Collection(g => g.Raffles).LoadAsync();
                var raffle = guild.Raffles.FirstOrDefault(r => r.LocalId == id);

                if (raffle == null)
                {
                    throw new Exception("Raffle does not exist.");
                }
                else if (raffle.RaffleOwner != (long)context.User.Id)
                {
                    throw new Exception("You are not the raffle owner.");
                }

                BackgroundJob.Delete(raffle.HangfireId.ToString());

                guild.Raffles.Remove(raffle);

                await db.SaveChangesAsync();
            }

            await context.Channel.SendMessageAsync($"{context.User.Mention}'s raffle was cancelled.");
        }
    }
}
