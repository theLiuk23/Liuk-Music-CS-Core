using Discord.WebSocket;
using Victoria;
using Victoria.Node;
using Victoria.Player;
using Victoria.WebSocket;
using Victoria.Responses;
using Victoria.Node.EventArgs;
using Discord;
using Victoria.Responses.Search;
using Microsoft.Extensions.Logging;

namespace Liuk_Music_CS_Core.Services
{
	public class MusicService
	{
		private readonly DiscordSocketClient _client;
		private readonly LavaNode _lava;

		public MusicService(DiscordSocketClient client)
		{
			var logger = LoggerFactory.Create(builder =>
			{
				builder.AddConsole();
				builder.SetMinimumLevel(LogLevel.Debug);
			}).CreateLogger<LavaNode>();

			_client = client;
			_lava = new LavaNode(_client, new NodeConfiguration()
			{
				Hostname = "localhost",
				Port = 2333,
				Authorization = "youshallnotpass",
				SelfDeaf = true
			}, logger);
		}

		public Task InitializeAsync()
		{
			try
			{
				_client.Ready += ClientReadyAsync;
				_lava.OnTrackEnd += TrackEnded;
			}
			catch (Exception error)
			{
				Console.WriteLine(error.Message);
			}

			return Task.CompletedTask;
		}

		private Task TrackEnded(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
		{
			throw new NotImplementedException();
		}

		public async Task JoinVoiceChannelAsync(SocketVoiceChannel voiceChannel, ITextChannel textChannel)
			=> await _lava.JoinAsync(voiceChannel, textChannel);

		public async Task LeaveVoiceChannelAsync(SocketVoiceChannel? voiceChannel)
			=> await _lava.LeaveAsync(voiceChannel);

		public async Task<string> PlayAsync(string query, SocketVoiceChannel voice, ITextChannel text)
		{
			if (!_lava.Players.Any() || !_lava.Players.FirstOrDefault().IsConnected)
				await this.JoinVoiceChannelAsync(voice, text);

			var _player = _lava.Players.FirstOrDefault();

			SearchResponse result = await _lava.SearchAsync(Victoria.Responses.Search.SearchType.YouTube, query);
			if (result.Status == SearchStatus.NoMatches)
				return "No matches found.";

			LavaTrack track = result.Tracks.FirstOrDefault();
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
			var _player = _lava.Players.FirstOrDefault();

			if (_player is null)
				return $"Error while trying to access the bot player.";

			await _player.StopAsync();
			return "Stopped the music!";
		}

		public async Task<string> SkipAsync()
		{
			var _player = _lava.Players.FirstOrDefault();

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
			var _player = _lava.Players.FirstOrDefault();

			if (_player is null)
				return $"Error while trying to access the bot player.";

			if (_player.Vueue.Count <= 0)
				return "Queue is empty";

			List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();
			fields.Add(new EmbedFieldBuilder()
			{
				Name = "Now playing",
				Value = _player.Track.Title,
				IsInline = false
			});

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
			var _player = _lava.Players.FirstOrDefault();

			if (_player is null)
				return $"Error while trying to access the bot player.";

			if (_player.PlayerState != PlayerState.Playing)
				return "The bot is not playing anything at the moment.";

			LavaTrack track = _player.Track;
			List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>()
			{
				new EmbedFieldBuilder() {Name = "Title", Value = track.Title, IsInline=false },
				new EmbedFieldBuilder() {Name = "Author", Value = track.Author, IsInline=true },
				new EmbedFieldBuilder() {Name = "Timeline", Value = track.Position, IsInline=false },
				new EmbedFieldBuilder() {Name = "Duration", Value = track.Duration, IsInline=true },
				new EmbedFieldBuilder() {Name = "Link", Value = track.Url, IsInline=false }
			};
			Embed embed = CreateEmbed("Now playing", "It shows some info about the currently playing track.",
				track.Url, await track.FetchArtworkAsync(), user, fields);

			return embed;
		}

		public async Task<object> LyricsAsync(IUser user)
		{
			var _player = _lava.Players.FirstOrDefault();

			if (_player is null)
				return $"Error while trying to access the bot player.";

			if (_player.Track is null)
				return "The bot is not playing any song.";

			string lyrics = await _player.Track.FetchLyricsFromGeniusAsync();
			List<EmbedFieldBuilder> fields = new List<EmbedFieldBuilder>();

			if (lyrics.Length >= 2000)
			{
				for (int i = 0; i <= lyrics.Length / 2000; i++)
				{
					fields.Add(new EmbedFieldBuilder()
					{
						Name = "‎ ",
						Value = lyrics.Substring(i * 2000, i * 2000 + 2000),
						IsInline = false
					});
				}
			}
			else
			{
				fields.Add(new EmbedFieldBuilder()
				{
					Name = "‎ ",
					Value = lyrics,
					IsInline = false
				});
			}

			Embed embed = CreateEmbed($"Lyrics for: {_player.Track.Title}", "It shows the lyrics of the currently playing track.",
				null, await _player.Track.FetchArtworkAsync(), user, fields);

			return embed;
		}

		private async Task ClientReadyAsync()
			=> await _lava.ConnectAsync();

		private static Embed CreateEmbed(string title, string? description, string? url, string? imageUrl, IUser? user, List<EmbedFieldBuilder> fields)
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
	}
}