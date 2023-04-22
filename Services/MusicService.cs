using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria;
using Victoria.Node;
using Victoria.Player;
using Victoria.WebSocket;
using Victoria.Responses;
using Victoria.Node.EventArgs;
using Discord;
using Microsoft.Extensions.Logging;
using Victoria.Responses.Search;

namespace Liuk_Music_CS_Core.Services
{
	public class MusicService
	{
		private DiscordSocketClient _client;
		private LavaNode _lavaNode;
		private LavaPlayer<LavaTrack>? _player; // null when bot is not connected to a voice channel

		public MusicService(DiscordSocketClient client)
		{
			_client = client;
			var lavaConfig = new NodeConfiguration()
			{
				Hostname = "localhost",
				Port = 2333,
				Authorization = "youshallnotpass"
			};
			_lavaNode = new LavaNode(_client, lavaConfig, null);
		}

		public Task InitializeAsync()
		{
			// _lavaNode = new LavaNode(_client, new NodeConfiguration(), _logger);
			_client.Ready += ClientReadyAsync;
			_lavaNode.OnTrackEnd += TrackFinished;
			_lavaNode.OnTrackException += TrackException;
			return Task.CompletedTask;
		}

		public async Task ConnectToVoiceChannelAsync(SocketVoiceChannel voiceChannel, ITextChannel textChannel)
		{
			_player = await _lavaNode.JoinAsync(voiceChannel, textChannel);
		}

		public async Task LeaveVoiceChannelAsync(SocketVoiceChannel? voiceChannel)
			=> await _lavaNode.LeaveAsync(voiceChannel);

		public async Task<string> PlayAsync(string query)
		{
			if (_player is null)
				return $"Error while trying to access the bot player.";

			SearchResponse result = await _lavaNode.SearchAsync(Victoria.Responses.Search.SearchType.YouTube, query);

			if (result.Status == SearchStatus.NoMatches)
				return "No matches found.";

			LavaTrack track = result.Tracks.First();
			
			if (_player.PlayerState == PlayerState.Playing || _player.PlayerState == PlayerState.Paused)
			{
				_player.Vueue.Enqueue(track);
				return $"Added '{track.Title}' to the queue!";
			}
			else
			{
				await _player.PlayAsync(track);
				return $"Now playing '{track.Title}'";
			}
		}

		public async Task<string> StopAsync()
		{
			if (_player is null)
				return $"Error while trying to access the bot player.";

			await _player.StopAsync();
			return "Stopped the music!";
		}

		public async Task<string> SkipAsync()
		{
			if (_player is null)
				return $"Error while trying to access the bot player.";

			if (_player.PlayerState != PlayerState.Playing)
				return "The bot is not playing anything at the moment.";

			if (_player.Vueue.Count <= 0)
				return "Queue is empty.";

			await _player.SkipAsync();
			return $"Skipped this song! Now playing: {_player.Track.Title}";
		}

		public object QueueAsync(IUser user)
		{
			if (_player is null)
				return $"Error while trying to access the bot player.";

			if (_player.Vueue.Count <= 0)
				return "Queue is empty";

			List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
			int i = 1;
			foreach (LavaTrack track in _player.Vueue)
			{
				fields.Add(new EmbedFieldBuilder()
				{
					Name = $"Track #{i}",
					Value = track.Title,
					IsInline = false
				});
				i++;
			}
			
			Embed embed = CreateEmbed("Queue", "It shows a list of all the songs in the queue.", 
				null, null, user, fields);

			return embed;
		}

		public async Task<object> NowPlayingAsync(IUser user)
		{
			if (_player is null)
				return $"Error while trying to access the bot player.";

			if (_player.PlayerState != PlayerState.Playing)
				return "The bot is not playing anything at the moment.";

			LavaTrack track = _player.Track;
			List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>()
			{
				new EmbedFieldBuilder() {Name = "Title", Value = track.Title, IsInline=true },
				new EmbedFieldBuilder() {Name = "Author", Value = track.Author, IsInline=true },
				new EmbedFieldBuilder() {Name = "Timeline", Value = track.Position, IsInline=false },
				new EmbedFieldBuilder() {Name = "Duration", Value = track.Duration, IsInline=true },
				new EmbedFieldBuilder() {Name = "Link", Value = track.Url, IsInline=false }
			};
			Embed embed = CreateEmbed("Now playing", "It shows some info about the currently playing track.",
				track.Url, await track.FetchArtworkAsync(), user, fields);

			return embed;
		}

		private async Task ClientReadyAsync()
			=> await _lavaNode.ConnectAsync();

		private Embed CreateEmbed(string title, string description, string url, string imageUrl, IUser user, List<EmbedFieldBuilder> fields)
		{
			EmbedBuilder embed = new EmbedBuilder()
			{
				Title = title,
				Description = description,
				Url = url,
				Fields = fields,
				Author = user as EmbedAuthorBuilder,
				ImageUrl = imageUrl
			};

			return embed.Build();
		}

		private async Task TrackFinished(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> lava)
		{
			if (lava.Reason != TrackEndReason.Finished)
				return;

			if (!lava.Player.Vueue.TryDequeue(out var item) || !(item is LavaTrack nextTrack))
			{
				await lava.Player.TextChannel.SendMessageAsync("Queue is empty!");
				return;
			}

			await lava.Player.PlayAsync(nextTrack);
		}

		private Task TrackException(TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
		{
			Console.WriteLine($"Message: {arg.Exception.Message}");
			return Task.CompletedTask;
		}
	}
}