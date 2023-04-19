using Discord.Commands;
using Discord.WebSocket;
using System.Reflection;

namespace Liuk_Music_CS_Core
{
	public class CommandHanlder
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _cmdService;
		private readonly IServiceProvider _services;

		public CommandHanlder(DiscordSocketClient client, CommandService cmdService, IServiceProvider services)
		{
			_client = client;
			_cmdService = cmdService;
			_services = services;
		}

		public async Task InitializeAsync()
		{
			await _cmdService.AddModulesAsync(assembly: Assembly.GetEntryAssembly(), _services);
			_cmdService.Log += LogAsync;
			// _client.MessageReceived
		}

		private Task LogAsync(Discord.LogMessage message)
		{
			Console.WriteLine(message.Message);
			return Task.CompletedTask;
		}
	}
}