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

namespace Liuk_Music_CS_Core.Modules
{
	public class Music : ModuleBase<SocketCommandContext>
	{
		private readonly MusicService _musicService;

		public Music(MusicService musicService)
		{
			_musicService = musicService;
		}

		[Command("Join")]
		[Summary("The bot joins the channel the user is in.")]
		public async Task Join()
		{
			var user = Context.User as SocketGuildUser;
			if (user.VoiceChannel is null)
			{
				await ReplyAsync("You need to connect to a voice channel.");
				return;
			}

			// Console.WriteLine($"Trying to connect user {user.Username} to {user.VoiceChannel.Name}@{user.VoiceChannel.Position} from chat {Context.Channel.Name}");
			await _musicService.ConnectToVoiceChannelAsync(user.VoiceChannel, Context.Channel as ITextChannel);
			await ReplyAsync($"Connected to {user.VoiceChannel.Name}");
		}
	}
}