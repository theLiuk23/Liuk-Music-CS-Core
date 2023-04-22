using Discord.Commands;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Liuk_Music_CS_Core.Modules
{
	public class Ping : ModuleBase<SocketCommandContext>
	{
		[Command("ping")]
		[Summary("It tests if the bot runs properly. It replies with the latency of the user.")]
		public async Task Command()
		{
			await ReplyAsync($"Latency: {Context.Client.Latency} ms.");
		}
	}
}