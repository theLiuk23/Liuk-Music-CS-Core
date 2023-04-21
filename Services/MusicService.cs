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

namespace Liuk_Music_CS_Core.Services
{
	public class MusicService
	{
		private ILogger<LavaNode> logger;
		private DiscordSocketClient _client;
		private LavaNode _lavaNode;

		public MusicService(DiscordSocketClient client)
		{
			_client = client;
			_lavaNode = new LavaNode(_client, new NodeConfiguration(), null);
		}

		public Task InitializeAsync()
		{
			_lavaNode = new LavaNode(_client, new NodeConfiguration(), null);
			_client.Ready += ClientReadyAsync;
			_lavaNode.OnStatsReceived += LogAsync;
			_lavaNode.OnTrackEnd += TrackFinished;
			_lavaNode.OnTrackException += TrackException;
			return Task.CompletedTask;
		}

		private Task LogAsync(StatsEventArg arg)
		{
			return Task.CompletedTask;
		}

		public async Task ConnectToVoiceChannelAsync(SocketVoiceChannel voiceChannel, ITextChannel textChannel)
		{
			Console.WriteLine("yuppy");
			await _lavaNode.JoinAsync(voiceChannel, textChannel);
		}

		private async Task ClientReadyAsync()
		{
			await _lavaNode.ConnectAsync();
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
			Console.WriteLine(arg.Exception.Message);
			return Task.CompletedTask;
		}
	}
}