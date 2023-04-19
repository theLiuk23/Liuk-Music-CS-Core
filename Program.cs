using Discord;
using Victoria;

namespace Liuk_Music_CS_Core
{
	// https://youtu.be/QwYmRNlgzaA?t=3352
	// 55:30
	public class Program
	{
		static void Main(string[] args) => new BotClient().InitializeAsync().GetAwaiter().GetResult();
	}
}