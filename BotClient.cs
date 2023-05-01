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
using Liuk_Music_CS_Core.Services;
using Victoria.WebSocket;
using Victoria.Player;
using Microsoft.Extensions.Logging;

namespace Liuk_Music_CS_Core
{
	public class BotClient
	{
		private readonly DiscordSocketClient _client;
		private readonly CommandService _cmdService;
		private IServiceProvider? _services;

		// constructor
		public BotClient(DiscordSocketClient? client = null, CommandService? cmdService = null)
		{
			_client = client ?? new DiscordSocketClient(new DiscordSocketConfig
			{
				AlwaysDownloadUsers = true,
				MessageCacheSize = 100,
				LogLevel = LogSeverity.Debug,
				GatewayIntents = GatewayIntents.AllUnprivileged
			});

			_cmdService = cmdService ?? new CommandService(new CommandServiceConfig
			{
				CaseSensitiveCommands = false,
				LogLevel = LogSeverity.Verbose
			});
		}

		public async Task InitializeAsync()
		{
			await _client.LoginAsync(TokenType.Bot, Secrets.token);
			await _client.StartAsync();

			_client.Log += LogAsync;
			_services = SetupServices();

			var cmdHandler = new CommandHanlder(_client, _cmdService, _services);
			await cmdHandler.InitializeAsync();
			await _services.GetRequiredService<MusicService>().InitializeAsync();

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
			.AddSingleton<MusicService>()
			.AddSingleton<LavaNode>()
			.BuildServiceProvider();
	}
}
