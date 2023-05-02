using Discord;
using Victoria;

namespace Liuk_Music_CS_Core
{
	// https://youtu.be/QwYmRNlgzaA?t=6637
	// 1:50:30
	public class Program
	{
		// TODO: 
		/* 
		 - timeline in np command
		 - intents
		 - general simplification
		 */
		static void Main() => new BotClient().InitializeAsync().GetAwaiter().GetResult();
	}
}