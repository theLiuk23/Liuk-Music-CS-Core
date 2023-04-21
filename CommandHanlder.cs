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
			_client.MessageReceived += MessageHandlerAsync;
		}

		private async Task MessageHandlerAsync(SocketMessage socketMessage)
		{
			var argPos = 0;
			if (socketMessage.Author.IsBot) return;

			var userMessage = (SocketUserMessage)socketMessage;
			if (userMessage is null) return;

			if (!userMessage.HasMentionPrefix(_client.CurrentUser, ref argPos))
				return;

			var context = new SocketCommandContext(_client, userMessage);
			await _cmdService.ExecuteAsync(context, argPos, _services);
		}

		private Task LogAsync(Discord.LogMessage message)
		{
			Console.WriteLine(message.Message);
			return Task.CompletedTask;
		}
	}
}