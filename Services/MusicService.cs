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
using System.Reflection.Metadata.Ecma335;

namespace Liuk_Music_CS_Core.Services
{
	public class MusicService
	{
		private readonly DiscordSocketClient _client;
		private LavaNode _lava;
		private LavaPlayer? _player;

		public MusicService(DiscordSocketClient client)
		{
			var logger = LoggerFactory.Create(builder =>
			{
				builder.AddConsole();
				builder.SetMinimumLevel(LogLevel.Error);
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
			_client.Ready += ClientReadyAsync;
			_lava.OnTrackException += TrackException;
			_lava.OnTrackEnd += TrackEnded;

			return Task.CompletedTask;
		}

		public Task TrackException(TrackExceptionEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
		{
			Console.WriteLine(arg.Exception.Message);
			return Task.CompletedTask;
		}

		public virtual async Task TrackEnded(TrackEndEventArg<LavaPlayer<LavaTrack>, LavaTrack> arg)
		{
			if (arg.Player.Vueue.TryDequeue(out var newTrack))
			{
				await arg.Player.TextChannel.SendMessageAsync($"Now playing '{newTrack.Title}'");
			}

			await arg.Player.PlayAsync(newTrack);
		}

		public async Task JoinVoiceChannelAsync(SocketVoiceChannel voiceChannel, ITextChannel textChannel)
			=> _player = (LavaPlayer)await _lava.JoinAsync(voiceChannel, textChannel);

		public async Task LeaveVoiceChannelAsync(SocketVoiceChannel? voiceChannel)
		{
			await _lava.LeaveAsync(voiceChannel);
		}

		public async Task<string> PlayAsync(string query, SocketVoiceChannel voice, ITextChannel text)
		{
			if (_player is null)
			{
				await JoinVoiceChannelAsync(voice, text);
				Console.WriteLine($"PLAYER: {_player}");
			}

			SearchResponse result = await _lava.SearchAsync(SearchType.YouTube, query);
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
			if (_player is null)
				return $"Error while trying to access the bot player.";

			if (_player.Track is null)
				return "The bot is not playing any song.";

			string lyrics = await _player.Track.FetchLyricsFromGeniusAsync();
			var chunks = lyrics.Chunk(1024);
			var fields = new List<EmbedFieldBuilder>();
			foreach (var chunk in chunks)
			{
				fields.Add(new EmbedFieldBuilder
				{
					Name = _player.Track.Title,
					Value = chunk,
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