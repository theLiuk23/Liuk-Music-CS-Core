using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Liuk_Music_CS_Core.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Victoria.Node;
using Victoria.Player;

namespace Liuk_Music_CS_Core.Modules
{
	public class Music : ModuleBase<SocketCommandContext>
	{
		private readonly MusicService _musicService;

		public Music(MusicService musicService)
		{
			_musicService = musicService;
		}

		[Command("join")]
		[Summary("The bot joins the channel the user is in.")]
		public async Task Join()
		{
			var user = Context.User as SocketGuildUser;
			if (user is null || user.VoiceChannel is null)
			{
				await ReplyAsync("You need to connect to a voice channel.");
				return;
			}

			await _musicService.JoinVoiceChannelAsync(user.VoiceChannel, Context.Channel as ITextChannel);
			await ReplyAsync($"Connected to '{user.VoiceChannel.Name}'.");
		}

		[Command("leave")]
		[Summary("The bot leaves the voice channel.")]
		public async Task Leave()
		{
			IGuildUser? clientUser = await Context.Channel.GetUserAsync(Context.Client.CurrentUser.Id) as IGuildUser;
			if (clientUser is null || clientUser.VoiceChannel is null)
			{
				await ReplyAsync("The bot is not connected to a voice channel at the moment.");
				return;
			}

			await _musicService.LeaveVoiceChannelAsync(clientUser.VoiceChannel as SocketVoiceChannel);
			await ReplyAsync($"Left '{clientUser.VoiceChannel}' voice channel.");
		}

		[Command("play")]
		[Summary("It plays some music by searching the song title to YouTube.")]
		public async Task Play([Remainder]string query)
		{
			var user = Context.User as SocketGuildUser;
			if (user is null || user.VoiceChannel is null)
			{
				await ReplyAsync("You need to connect to a voice channel.");
				return;
			}
			var result = await _musicService.PlayAsync(query, user.VoiceChannel, Context.Channel as ITextChannel);
			await ReplyAsync(result);
		}

		[Command("stop")]
		[Summary("It stops the music.")]
		public async Task Stop()
		{
			string result = await _musicService.StopAsync();
			await ReplyAsync(result);
		}

		[Command("skip")]
		[Summary("It skips to the next song.")]
		public async Task Skip()
		{
			string result = await _musicService.SkipAsync();
			await ReplyAsync(result);
		}

		[Command("queue")]
		[Summary("It shows a list of all the songs in the queue.")]
		public async Task Queue()
		{
			object result = _musicService.QueueAsync(Context.User as IUser);
			if (result is Embed)
				await ReplyAsync(embed: result as Embed);
			else
				await ReplyAsync(result as string);
		}

		[Command("np")]
		[Summary("It shows some info about the currently playing track.")]
		public async Task NowPlaying()
		{
			object result = await _musicService.NowPlayingAsync(Context.User as IUser);
			if (result is Embed)
				await ReplyAsync(embed: result as Embed);
			else
				await ReplyAsync(result as string);
		}

		[Command("lyrics")]
		[Summary("It shows the lyrics of the currently playing track.")]
		public async Task Lyrics([Remainder]string? query=null)
		{
			object result = await _musicService.LyricsAsync(Context.User as IUser, query);
			if (result is Embed)
				await ReplyAsync(embed: result as Embed);
			else
				await ReplyAsync(result as string);
		}
	}
}