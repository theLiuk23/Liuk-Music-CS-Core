using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Liuk_Music_CS_Core.Services
{
	public static class MethodService
	{
		public static string[] Divide(this string text, int chuckSize = 1024)
		{
			text = text.Replace("\n", " ");

			if (text.Length <= chuckSize)
				return new string[] { text };

			List<string> result = new List<string>();
			for (int i = 0; i < text.Length / chuckSize; i++)
			{
				if (i * chuckSize + chuckSize > 6000)
					break;
				string chuck = text.Substring(i * chuckSize, chuckSize);
				result.Add(chuck);
			}

			return result.ToArray();
		}
	}
}