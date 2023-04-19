using System;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Victoria;
using Victoria.Node;

namespace Liuk_Music_CS_Core
{
	public class BotClient
	{
		private DiscordSocketClient? _client;
		private CommandService? _cmdService;
		private IServiceProvider? _services;

		// constructor
		public BotClient(DiscordSocketClient? client = null, CommandService? cmdService = null)
		{
			_client = client ?? new DiscordSocketClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = true,
				MessageCacheSize = 100,
				LogLevel = Discord.LogSeverity.Debug
			});

			_cmdService = cmdService ?? new CommandService(new CommandServiceConfig
			{
				CaseSensitiveCommands = false,
				LogLevel = Discord.LogSeverity.Verbose
			});
		}

		public async Task InitializeAsync()
		{
			await _client.LoginAsync(Discord.TokenType.Bot, secrets.token);
			await _client.StartAsync();

			_client.Log += LogAsync;
			_services = SetupServices();

			await Task.Delay(-1);
		}

		private Task LogAsync(Discord.LogMessage message)
		{
			Console.WriteLine(message.Message);
			return Task.CompletedTask;
		}

		private IServiceProvider SetupServices()
			=> new ServiceCollection()
			.AddSingleton(_client)
			.AddSingleton(_cmdService)
			.AddSingleton<LavaNode>()
			.AddSingleton<LavaNode>()
			.BuildServiceProvider();
	}
}
