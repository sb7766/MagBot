using System;
using System.Collections.Generic;
using System.Text;
using Discord.Commands;
using System.Threading.Tasks;
using Discord;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using Google.Apis.YouTube.v3;
using Microsoft.Extensions.Configuration;
using Google.Apis.Services;
using Google.Apis.Customsearch.v1;

namespace MagBot.Modules
{
    [Name("Search")]
    public class SearchModule : ModuleBase
    {
        private readonly IConfiguration _config;

        public SearchModule(IConfiguration config)
        {
            _config = config;
        }

        [Command("derpi")]
        [Summary("Fetch a random image from Derpibooru for the given tags. If no tags are given, a completely random image is returned. " +
            "Will only return NSFW results in a channel marked as NSFW.")]
        public async Task Derpibooru([Remainder] string tags)
        {
            // explicit https://derpibooru.org/search.json?q=&sf=random&filter_id=56027
            // safe https://derpibooru.org/search.json?q=&sf=random&filter_id=100073
            string search = "";
            if (Context.Channel.IsNsfw)
            {
                search = $"https://derpibooru.org/search.json?q={tags}&sf=random&filter_id=56027";
            }
            else
            {
                search = $"https://derpibooru.org/search.json?q={tags}&sf=random&filter_id=100073";
            }
            var result = await GetDerpiEmbedAsync(search);
            if (result.Item2 != null) await ReplyAsync($"Here you go, {Context.User.Mention}!", false, result.Item2);
            await ReplyAsync(result.Item1);
        }

        [Command("derpi")]
        public async Task Derpibooru()
        {
            string search = "";
            if (Context.Channel.IsNsfw)
            {
                search = $"https://derpibooru.org/search.json?q=*&sf=random&filter_id=56027";
            }
            else
            {
                search = $"https://derpibooru.org/search.json?q=*&sf=random&filter_id=100073";
            }
            var result = await GetDerpiEmbedAsync(search);
            if (result.Item2 != null) await ReplyAsync($"Here you go, {Context.User.Mention}!", false, result.Item2);
            await ReplyAsync(result.Item1);
        }

        private async Task<Tuple<string, Embed>> GetDerpiEmbedAsync(string searchUrl)
        {
            var jsonString = await new HttpClient().GetStringAsync(searchUrl);
            var json = JObject.Parse(jsonString);
            if (json["total"] == null || json.Value<int>("total") == 0)
            {
                return new Tuple<string, Embed>($"No results found. Sorry, {Context.User.Mention}!", null);
            }
            var searchResult = json["search"][0];

            EmbedBuilder embed = new EmbedBuilder
            {
                Color = new Color(61, 146, 208),
                Title = $"ID: {searchResult["id"]}",
                Url = $"https://derpibooru.org/{searchResult["id"]}",
                Description = $"Uploaded by {searchResult["uploader"]}",
                ThumbnailUrl = $"https:{searchResult["representations"]["thumb_small"]}"
            };

            string tags = searchResult.Value<string>("tags");
            if (tags.Length > 1024) tags.Remove(1023);
            embed.AddInlineField("Score", searchResult["score"])
                .AddInlineField("Faves", searchResult["faves"])
                .AddField("Tags", tags);


            string imageUrl = $"https:{searchResult.Value<string>("image")}";
            imageUrl = $"{imageUrl.Remove(imageUrl.IndexOf("__"))}.{searchResult["original_format"]}";

            return new Tuple<string, Embed>(imageUrl, embed.Build());
        }

        [Command("youtube")]
        [Alias("yt")]
        [Summary("Get the first YouTube search result for the given search terms.")]
        public async Task Youtube([Remainder] string search)
        {
            var youtube = new YouTubeService(new BaseClientService.Initializer
            {
                ApiKey = _config["youtubeApiKey"]
            });
            var searchRequest = youtube.Search.List("snippet");
            searchRequest.Q = search;
            searchRequest.Type = "video";

            var searchResponse = await searchRequest.ExecuteAsync();

            if (searchResponse.Items == null || searchResponse.Items.Count == 0)
            {
                await ReplyAsync($"No results found. Sorry, {Context.User.Mention}!");
                return;
            }

            string link = $"https://youtu.be/{searchResponse.Items[0].Id.VideoId}";

            await ReplyAsync($"Here you go, {Context.User.Mention}! \n{link}");
        }

        [Command("google")]
        [Alias("g")]
        [Summary("Get the first Google search result for the given search terms.")]
        public async Task Google([Remainder] string search)
        {
            var result = await GetGoogleResultAsync(search);
            await ReplyAsync(result);
        }

        [Command("googleimages")]
        [Alias("gi")]
        [Summary("Get the first Google Images search result for the given search terms. **Use at your own risk!**")]
        public async Task GoogleImages([Remainder] string search)
        {
            var result = await GetGoogleResultAsync(search, true);
            await ReplyAsync(result);
        }

        private async Task<string> GetGoogleResultAsync(string search, bool image = false)
        {
            var google = new CustomsearchService(new BaseClientService.Initializer
            {
                ApiKey = _config["googleSearchApiKey"]
            });

            var searchRequest = google.Cse.List(search);
            searchRequest.Cx = _config["googleSearchApiId"];

            if (image)
            {
                searchRequest.SearchType = 0;
            }

            var searchResult = await searchRequest.ExecuteAsync();

            if(searchResult.Items == null || searchResult.Items.Count == 0)
            {
                return $"No search results found. Sorry, {Context.User.Mention}!";
            }

            string url = searchResult.Items[0].Link;

            return $"Here you go, {Context.User.Mention}! \n{url}";
        }

        [Command("e621")]
        [Summary("Fetch a random image from e621 for the given tags. If no tags are given, a completely random image is returned. " +
            "Will only return NSFW results in a channel marked as NSFW. Maximum of 4 tags in a SFW channel and 5 in a NSFW channel.")]
        public async Task E621([Remainder] string tags)
        {
            // explicit https://derpibooru.org/search.json?q=&sf=random&filter_id=56027
            // safe https://derpibooru.org/search.json?q=&sf=random&filter_id=100073
            string search = "";
            if (Context.Channel.IsNsfw)
            {
                search = $"https://e621.net/post/index.json?tags=order:random+{tags}&limit=5";
            }
            else
            {
                search = $"https://e621.net/post/index.json?tags=order:random+rating:s+{tags}&limit=5";
            }
            var result = await GetE621EmbedAsync(search);
            if (result.Item2 != null) await ReplyAsync($"Here you go, {Context.User.Mention}!", false, result.Item2);
            await ReplyAsync(result.Item1);
        }

        [Command("e621")]
        public async Task E621()
        {
            string search = "";
            if (Context.Channel.IsNsfw)
            {
                search = $"https://e621.net/post/index.json?tags=order:random&limit=5";
            }
            else
            {
                search = $"https://e621.net/post/index.json?tags=order:random+rating:s&limit=5";
            }
            var result = await GetE621EmbedAsync(search);
            if (result.Item2 != null) await ReplyAsync($"Here you go, {Context.User.Mention}!", false, result.Item2);
            await ReplyAsync(result.Item1);
        }

        private async Task<Tuple<string, Embed>> GetE621EmbedAsync(string searchUrl)
        {
            var http = new HttpClient();
            http.DefaultRequestHeaders.Add("User-Agent", "Sunburst/Beta (by Magmatic#2220 on Discord)");
            var jsonString = await http.GetStringAsync(searchUrl);
            var json = JArray.Parse(jsonString);
            if (json == null || json.Count == 0)
            {
                return new Tuple<string, Embed>($"No results found. Sorry, {Context.User.Mention}!", null);
            }
            var searchResult = json[0];

            EmbedBuilder embed = new EmbedBuilder
            {
                Color = new Color(61, 146, 208),
                Title = $"ID: {searchResult["id"]}",
                Url = $"https://e621.net/post/show/{searchResult["id"]}",
                Description = $"Uploaded by {searchResult["author"]}",
                ThumbnailUrl = searchResult.Value<string>("preview_url")
            };

            string tags = searchResult.Value<string>("tags");
            if (tags.Length > 1024) tags.Remove(1023);
            embed.AddInlineField("Score", searchResult["score"])
                .AddInlineField("Faves", searchResult["fav_count"])
                .AddField("Tags", tags);


            string imageUrl = searchResult.Value<string>("file_url");

            return new Tuple<string, Embed>(imageUrl, embed.Build());
        }
    }
}
